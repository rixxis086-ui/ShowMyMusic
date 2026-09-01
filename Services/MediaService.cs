using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Media.Control;
using Windows.Storage.Streams;
using ShowMyMusic.Models;

namespace ShowMyMusic.Services
{
    public class MediaService : IDisposable
    {
        private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;
        private GlobalSystemMediaTransportControlsSession? _currentSession;
        private readonly DispatcherTimer _heartbeatTimer;
        private readonly SemaphoreSlim _updateLock = new(1, 1);
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private bool _isDisposed;

        private TimeSpan _lastGsmtcRawPosition = TimeSpan.Zero;
        private DateTimeOffset _lastGsmtcLastUpdated = DateTimeOffset.MinValue;
        private DateTime _lastGsmtcSnapshotTime = DateTime.UtcNow;

        public event EventHandler<TrackInfo>? TrackChanged;
        public event EventHandler<TrackInfo>? PlaybackStateChanged;
        public event EventHandler<TrackInfo>? TimelineChanged;

        public TrackInfo CurrentTrack { get; } = new TrackInfo();

        public MediaService()
        {
            _heartbeatTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1500)
            };
            _heartbeatTimer.Tick += async (s, e) => await HeartbeatCheckAsync();
        }

        public async Task InitializeAsync()
        {
            try
            {
                await EnsureSessionManagerAsync();
                _heartbeatTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MediaService initialization failed: {ex.Message}");
                _heartbeatTimer.Start();
            }
        }

        private async Task EnsureSessionManagerAsync()
        {
            if (_sessionManager != null) return;

            try
            {
                _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                if (_sessionManager != null)
                {
                    _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                    _sessionManager.SessionsChanged += OnSessionsChanged;
                    await UpdateCurrentSessionAsync(_sessionManager.GetCurrentSession());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error requesting GSMTC session manager: {ex.Message}");
                _sessionManager = null;
            }
        }

        private async Task HeartbeatCheckAsync()
        {
            if (_isDisposed) return;
            if (_refreshLock.CurrentCount == 0) return;

            try
            {
                if (_sessionManager == null)
                {
                    await EnsureSessionManagerAsync();
                    return;
                }

                var activeSession = _sessionManager.GetCurrentSession();
                if (activeSession != null)
                {
                    if (_currentSession == null || _currentSession.SourceAppUserModelId != activeSession.SourceAppUserModelId)
                        await UpdateCurrentSessionAsync(activeSession);
                    else
                        await PollTimelineAsync();
                }
                else
                {
                    var sessions = _sessionManager.GetSessions();
                    var fallback = sessions.FirstOrDefault(s =>
                    {
                        try { return s.GetPlaybackInfo()?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing; }
                        catch { return false; }
                    }) ?? sessions.FirstOrDefault();

                    if (fallback != null && fallback != _currentSession)
                        await UpdateCurrentSessionAsync(fallback);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HeartbeatCheck error: {ex.Message}");
                _sessionManager = null;
                _currentSession = null;
            }
        }

        private async void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            try { await UpdateCurrentSessionAsync(sender.GetCurrentSession()); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"OnCurrentSessionChanged error: {ex.Message}"); }
        }

        private async void OnSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            try { await UpdateCurrentSessionAsync(sender.GetCurrentSession()); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"OnSessionsChanged error: {ex.Message}"); }
        }

        private async Task UpdateCurrentSessionAsync(GlobalSystemMediaTransportControlsSession? session)
        {
            await _updateLock.WaitAsync();
            try
            {
                if (_currentSession != null)
                {
                    try
                    {
                        _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                        _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                        _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
                    }
                    catch { }
                }

                _currentSession = session;

                if (_currentSession != null)
                {
                    try
                    {
                        _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                        _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                        _currentSession.TimelinePropertiesChanged += OnTimelinePropertiesChanged;
                    }
                    catch { }
                    await RefreshMediaPropertiesAsync();
                }
            }
            finally
            {
                _updateLock.Release();
            }
        }

        private async void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            try { await RefreshMediaPropertiesAsync(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"OnMediaPropertiesChanged error: {ex.Message}"); }
        }

        private async void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            try { await RefreshPlaybackInfoAsync(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"OnPlaybackInfoChanged error: {ex.Message}"); }
        }

        private void OnTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            try { PollTimeline(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"OnTimelinePropertiesChanged error: {ex.Message}"); }
        }

        public async Task RefreshMediaPropertiesAsync()
        {
            if (_currentSession == null) return;

            if (!await _refreshLock.WaitAsync(0)) return;
            try
            {
                var mediaProps = await _currentSession.TryGetMediaPropertiesAsync();
                if (mediaProps == null) return;

                string appId = string.Empty;
                try { appId = _currentSession.SourceAppUserModelId ?? string.Empty; } catch { }

                string title = string.IsNullOrWhiteSpace(mediaProps.Title) ? "Unknown Title" : mediaProps.Title;
                string artist = string.IsNullOrWhiteSpace(mediaProps.Artist) ? "Unknown Artist" : mediaProps.Artist;
                string album = mediaProps.AlbumTitle ?? string.Empty;
                string appName = FormatAppName(appId);

                BitmapSource? thumbnail = null;
                string thumbHash = $"{title}|{artist}|{album}";

                if (mediaProps.Thumbnail != null)
                {
                    try
                    {
                        using var streamRef = await mediaProps.Thumbnail.OpenReadAsync();
                        if (streamRef != null)
                        {
                            using var stream = streamRef.AsStreamForRead();
                            using var memStream = new MemoryStream();
                            await stream.CopyToAsync(memStream);
                            memStream.Position = 0;

                            var bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.StreamSource = memStream;
                            bmp.EndInit();
                            bmp.Freeze();
                            thumbnail = bmp;
                        }
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Thumbnail load error: {ex.Message}"); }
                }

                bool trackChanged = CurrentTrack.Title != title || CurrentTrack.Artist != artist;
                bool thumbChanged = CurrentTrack.ThumbnailHash != thumbHash 
                                 || (thumbnail != null && CurrentTrack.Thumbnail == null)
                                 || (thumbnail == null && CurrentTrack.Thumbnail != null);

                if (trackChanged)
                {
                    _lastGsmtcRawPosition = TimeSpan.Zero;
                    _lastGsmtcLastUpdated = DateTimeOffset.MinValue;
                    _lastGsmtcSnapshotTime = DateTime.UtcNow;
                }

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    CurrentTrack.Title = title;
                    CurrentTrack.Artist = artist;
                    CurrentTrack.AlbumTitle = album;
                    CurrentTrack.AppSource = appName;
                    CurrentTrack.Thumbnail = thumbnail;
                    CurrentTrack.ThumbnailHash = thumbHash;
                });

                await RefreshPlaybackInfoAsync();
                PollTimeline();

                if (trackChanged || thumbChanged)
                    TrackChanged?.Invoke(this, CurrentTrack);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to refresh media properties: {ex.Message}");
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private async Task RefreshPlaybackInfoAsync()
        {
            if (_currentSession == null) return;
            try
            {
                var playbackInfo = _currentSession.GetPlaybackInfo();
                if (playbackInfo != null)
                {
                    bool isPlaying = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                    bool stateChanged = CurrentTrack.IsPlaying != isPlaying;
                    Application.Current?.Dispatcher.Invoke(() => CurrentTrack.IsPlaying = isPlaying);
                    if (stateChanged) PlaybackStateChanged?.Invoke(this, CurrentTrack);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"RefreshPlaybackInfo error: {ex.Message}"); }
            await Task.CompletedTask;
        }

        private void PollTimeline()
        {
            if (_currentSession == null) return;
            try
            {
                var timeline = _currentSession.GetTimelineProperties();
                if (timeline != null)
                {
                    var playbackInfo = _currentSession.GetPlaybackInfo();
                    bool isPlaying = playbackInfo?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

                    if (timeline.Position != _lastGsmtcRawPosition || timeline.LastUpdatedTime != _lastGsmtcLastUpdated)
                    {
                        _lastGsmtcRawPosition = timeline.Position;
                        _lastGsmtcLastUpdated = timeline.LastUpdatedTime;
                        _lastGsmtcSnapshotTime = DateTime.UtcNow;
                    }

                    TimeSpan realPosition = timeline.Position;
                    if (isPlaying)
                    {
                        if (timeline.LastUpdatedTime.Year >= 2000 && timeline.LastUpdatedTime <= DateTimeOffset.UtcNow.AddSeconds(5))
                        {
                            var elapsed = DateTimeOffset.UtcNow - timeline.LastUpdatedTime.ToUniversalTime();
                            if (elapsed >= TimeSpan.Zero && elapsed < TimeSpan.FromHours(24))
                                realPosition = timeline.Position + elapsed;
                            else
                                realPosition = _lastGsmtcRawPosition + (DateTime.UtcNow - _lastGsmtcSnapshotTime);
                        }
                        else
                        {
                            realPosition = _lastGsmtcRawPosition + (DateTime.UtcNow - _lastGsmtcSnapshotTime);
                        }
                    }

                    if (timeline.EndTime > TimeSpan.Zero && realPosition > timeline.EndTime)
                        realPosition = timeline.EndTime;
                    if (realPosition < TimeSpan.Zero)
                        realPosition = TimeSpan.Zero;

                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        CurrentTrack.Position = realPosition;
                        CurrentTrack.Duration = timeline.EndTime;
                        CurrentTrack.IsPlaying = isPlaying;
                    });

                    TimelineChanged?.Invoke(this, CurrentTrack);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"PollTimeline error: {ex.Message}"); }
        }

        private async Task PollTimelineAsync()
        {
            PollTimeline();
            await Task.CompletedTask;
        }

        private static string FormatAppName(string appId)
        {
            if (string.IsNullOrEmpty(appId)) return "Media Player";
            string lower = appId.ToLowerInvariant();
            if (lower.Contains("spotify")) return "Spotify";
            if (lower.Contains("yandexmusic") || lower.Contains("yandex")) return "Yandex Music";
            if (lower.Contains("apple")) return "Apple Music";
            if (lower.Contains("chrome")) return "Google Chrome";
            if (lower.Contains("msedge") || lower.Contains("edge")) return "Microsoft Edge";
            if (lower.Contains("firefox")) return "Firefox";
            if (lower.Contains("vlc")) return "VLC";
            if (lower.Contains("aimp")) return "AIMP";
            if (lower.Contains("telegram")) return "Telegram";

            int lastDot = appId.LastIndexOf('.');
            if (lastDot >= 0 && lastDot < appId.Length - 1)
                return appId.Substring(lastDot + 1);
            return appId;
        }

        #region Media Controls
        public async Task<bool> TogglePlayPauseAsync()
        {
            if (_currentSession == null) return false;
            try { return await _currentSession.TryTogglePlayPauseAsync(); }
            catch { return false; }
        }

        public async Task<bool> SkipNextAsync()
        {
            if (_currentSession == null) return false;
            try { return await _currentSession.TrySkipNextAsync(); }
            catch { return false; }
        }

        public async Task<bool> SkipPreviousAsync()
        {
            if (_currentSession == null) return false;
            try { return await _currentSession.TrySkipPreviousAsync(); }
            catch { return false; }
        }

        public async Task<bool> SeekToPercentAsync(double percent)
        {
            if (_currentSession == null) return false;
            try
            {
                if (CurrentTrack.Duration.TotalSeconds > 0)
                {
                    double targetSec = (percent / 100.0) * CurrentTrack.Duration.TotalSeconds;
                    long hns = (long)(targetSec * 10000000);
                    bool result = await _currentSession.TryChangePlaybackPositionAsync(hns);
                    if (result)
                    {
                        _lastGsmtcRawPosition = TimeSpan.FromSeconds(targetSec);
                        _lastGsmtcLastUpdated = DateTimeOffset.UtcNow;
                        _lastGsmtcSnapshotTime = DateTime.UtcNow;
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            CurrentTrack.Position = _lastGsmtcRawPosition;
                        });
                    }
                    return result;
                }
            }
            catch { }
            return false;
        }
        #endregion

        public void Dispose()
        {
            _isDisposed = true;
            _heartbeatTimer.Stop();
            if (_currentSession != null)
            {
                try
                {
                    _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                    _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                    _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
                }
                catch { }
            }
            if (_sessionManager != null)
            {
                try
                {
                    _sessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
                    _sessionManager.SessionsChanged -= OnSessionsChanged;
                }
                catch { }
            }
            _updateLock.Dispose();
            _refreshLock.Dispose();
        }
    }
}