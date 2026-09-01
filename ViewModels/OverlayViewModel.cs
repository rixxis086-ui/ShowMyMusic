using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ShowMyMusic.Helpers;
using ShowMyMusic.Models;
using ShowMyMusic.Services;

namespace ShowMyMusic.ViewModels
{
    public class OverlayViewModel : INotifyPropertyChanged
    {
        private readonly MediaService _mediaService;
        private readonly AudioVolumeService _volumeService;
        private readonly SettingsService _settingsService;
        private readonly DispatcherTimer _hideTimer;

        // High-precision smooth live progress timer
        private readonly DispatcherTimer _progressTimer;
        private DateTime _lastPositionTimestamp = DateTime.UtcNow;
        private TimeSpan _lastKnownPosition = TimeSpan.Zero;
        private bool _isPlaying = false;

        private bool _isHovered = false;
        private bool _isVisible = false;

        public TrackInfo Track => _mediaService.CurrentTrack;
        public AppSettings Settings => _settingsService.CurrentSettings;

        public event EventHandler? RequestShow;
        public event EventHandler? RequestHide;

        public ICommand TogglePlayPauseCommand { get; }
        public ICommand SkipNextCommand { get; }
        public ICommand SkipPreviousCommand { get; }

        public bool IsVisible
        {
            get => _isVisible;
            set { if (_isVisible != value) { _isVisible = value; OnPropertyChanged(); } }
        }

        public OverlayViewModel(MediaService mediaService, AudioVolumeService volumeService, SettingsService settingsService)
        {
            _mediaService = mediaService;
            _volumeService = volumeService;
            _settingsService = settingsService;

            _hideTimer = new DispatcherTimer();
            _hideTimer.Tick += (s, e) =>
            {
                _hideTimer.Stop();
                if (!Settings.IsPinned && !_isHovered)
                    HideOverlay();
            };

            // High frequency (100ms) for silky-smooth progress bar movement
            _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _progressTimer.Tick += OnProgressTimerTick;
            _progressTimer.Start();

            TogglePlayPauseCommand = new RelayCommand(async _ => await _mediaService.TogglePlayPauseAsync());
            SkipNextCommand = new RelayCommand(async _ => await _mediaService.SkipNextAsync());
            SkipPreviousCommand = new RelayCommand(async _ => await _mediaService.SkipPreviousAsync());

            _mediaService.TrackChanged += async (s, track) =>
            {
                _lastKnownPosition = track.Position;
                _lastPositionTimestamp = DateTime.UtcNow;
                _isPlaying = track.IsPlaying;

                await UpdateColorsAsync();
                if (Settings.ShowOnTrackChange)
                    ShowOverlay();
            };

            _mediaService.PlaybackStateChanged += (s, track) =>
            {
                _isPlaying = track.IsPlaying;
                _lastKnownPosition = track.Position;
                _lastPositionTimestamp = DateTime.UtcNow;

                if (Settings.ShowOnPlayPause)
                    ShowOverlay();
            };

            _mediaService.TimelineChanged += (s, track) =>
            {
                // Sync ground truth from MediaService calculation
                _lastKnownPosition = track.Position;
                _lastPositionTimestamp = DateTime.UtcNow;
                _isPlaying = track.IsPlaying;
            };

            _volumeService.VolumeChanged += (s, vol) =>
            {
                Track.VolumePercent = vol;
                if (Settings.ShowOnVolumeChange)
                    ShowOverlay();
            };

            _settingsService.SettingsChanged += async (s, set) =>
            {
                OnPropertyChanged(nameof(Settings));
                await UpdateColorsAsync();
                if (Settings.IsPinned)
                    ShowOverlay();
            };

            // Catch late thumbnail arrivals directly
            Track.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(TrackInfo.Thumbnail) || e.PropertyName == nameof(TrackInfo.ThumbnailHash))
                {
                    await UpdateColorsAsync();
                }
            };
        }

        private void OnProgressTimerTick(object? sender, EventArgs e)
        {
            if (!_isPlaying || Track.Duration.TotalSeconds <= 0)
                return;

            var elapsed = DateTime.UtcNow - _lastPositionTimestamp;
            var interpolated = _lastKnownPosition + elapsed;

            if (interpolated > Track.Duration)
                interpolated = Track.Duration;

            Track.Position = interpolated;
        }

        public async Task UpdateColorsAsync()
        {
            if (Settings.UseAdaptiveColor)
            {
                Color fallbackColor;
                try { fallbackColor = (Color)ColorConverter.ConvertFromString(Settings.CustomAccentColor); }
                catch { fallbackColor = Color.FromRgb(139, 92, 246); }

                var (accent, glow) = await ColorExtractorService.ExtractColorsAsync(
                    Track.Thumbnail,
                    Track.ThumbnailHash,
                    Settings.GlowIntensity,
                    fallbackColor);

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    // AccentColor must always have A=255 for solid DropShadowEffect rendering
                    Track.AccentColor = Color.FromRgb(accent.R, accent.G, accent.B);
                    Track.GlowColor = glow;
                });
            }
            else
            {
                Color customAccent;
                try { customAccent = (Color)ColorConverter.ConvertFromString(Settings.CustomAccentColor); }
                catch { customAccent = Color.FromRgb(139, 92, 246); }

                byte glowAlpha = (byte)Math.Clamp(Settings.GlowIntensity * 255, 0, 255);
                var customGlow = Color.FromArgb(glowAlpha, customAccent.R, customAccent.G, customAccent.B);

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Track.AccentColor = Color.FromRgb(customAccent.R, customAccent.G, customAccent.B);
                    Track.GlowColor = customGlow;
                });
            }
        }

        public void ShowOverlay()
        {
            _hideTimer.Stop();
            IsVisible = true;
            RequestShow?.Invoke(this, EventArgs.Empty);

            if (!Settings.IsPinned && !_isHovered)
            {
                _hideTimer.Interval = TimeSpan.FromSeconds(Math.Max(1.0, Settings.DisplayDurationSeconds));
                _hideTimer.Start();
            }
        }

        public void HideOverlay()
        {
            if (Settings.IsPinned) return;
            _hideTimer.Stop();
            IsVisible = false;
            RequestHide?.Invoke(this, EventArgs.Empty);
        }

        public void OnMouseEnter()
        {
            if (Settings.InteractionMode == InteractionMode.Interactive)
            {
                _isHovered = true;
                _hideTimer.Stop();
            }
        }

        public void OnMouseLeave()
        {
            if (Settings.InteractionMode == InteractionMode.Interactive)
            {
                _isHovered = false;
                if (!Settings.IsPinned && IsVisible)
                {
                    _hideTimer.Interval = TimeSpan.FromSeconds(Math.Max(1.0, Settings.DisplayDurationSeconds));
                    _hideTimer.Start();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}