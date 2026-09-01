using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShowMyMusic.Models
{
    public class TrackInfo : INotifyPropertyChanged
    {
        private string _title = "Not Playing";
        private string _artist = "Waiting for music...";
        private string _albumTitle = string.Empty;
        private string _appSource = "Windows Media";
        private BitmapSource? _thumbnail;
        private string _thumbnailHash = string.Empty;
        private bool _isPlaying = false;
        private TimeSpan _position = TimeSpan.Zero;
        private TimeSpan _duration = TimeSpan.Zero;
        private int _volumePercent = 50;
        private Color _accentColor = Color.FromRgb(139, 92, 246);
        private Color _glowColor = Color.FromArgb(180, 139, 92, 246);

        public string Title
        {
            get => _title;
            set { if (_title != value) { _title = value; OnPropertyChanged(); } }
        }

        public string Artist
        {
            get => _artist;
            set { if (_artist != value) { _artist = value; OnPropertyChanged(); } }
        }

        public string AlbumTitle
        {
            get => _albumTitle;
            set { if (_albumTitle != value) { _albumTitle = value; OnPropertyChanged(); } }
        }

        public string AppSource
        {
            get => _appSource;
            set { if (_appSource != value) { _appSource = value; OnPropertyChanged(); } }
        }

        public BitmapSource? Thumbnail
        {
            get => _thumbnail;
            set { if (_thumbnail != value) { _thumbnail = value; OnPropertyChanged(); } }
        }

        public string ThumbnailHash
        {
            get => _thumbnailHash;
            set { if (_thumbnailHash != value) { _thumbnailHash = value; OnPropertyChanged(); } }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set { if (_isPlaying != value) { _isPlaying = value; OnPropertyChanged(); } }
        }

        public TimeSpan Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PositionFormatted));
                    OnPropertyChanged(nameof(ProgressPercent));
                }
            }
        }

        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DurationFormatted));
                    OnPropertyChanged(nameof(ProgressPercent));
                }
            }
        }

        public int VolumePercent
        {
            get => _volumePercent;
            set { if (_volumePercent != value) { _volumePercent = value; OnPropertyChanged(); } }
        }

        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                if (_accentColor != value)
                {
                    _accentColor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AccentBrush));
                }
            }
        }

        public Color GlowColor
        {
            get => _glowColor;
            set
            {
                if (_glowColor != value)
                {
                    _glowColor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(GlowBrush));
                }
            }
        }

        public SolidColorBrush AccentBrush => new SolidColorBrush(AccentColor);
        public SolidColorBrush GlowBrush => new SolidColorBrush(GlowColor);

        public string PositionFormatted => Position.TotalHours >= 1 
            ? Position.ToString(@"h\:mm\:ss") 
            : Position.ToString(@"m\:ss");

        public string DurationFormatted => Duration.TotalHours >= 1 
            ? Duration.ToString(@"h\:mm\:ss") 
            : Duration.ToString(@"m\:ss");

        public double ProgressPercent => Duration.TotalSeconds > 0 
            ? Math.Clamp(Position.TotalSeconds / Duration.TotalSeconds * 100.0, 0.0, 100.0) 
            : 0.0;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
