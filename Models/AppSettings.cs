using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ShowMyMusic.Models
{
    public enum GlassStyle
    {
        iOSFrostedGlass,
        Win11Acrylic,
        DarkGlass,
        LightGlass,
        Solid
    }

    public enum InteractionMode
    {
        Interactive,
        Passive
    }

    public enum OverlayOrientation
    {
        Horizontal,
        Vertical
    }

    public enum PositionMode
    {
        Simple,
        Advanced
    }

    public enum SimpleZone
    {
        Top,
        Bottom,
        Left,
        Right
    }

    public enum AdvancedZone
    {
        TopCenter,
        TopLeft,
        TopRight,
        BottomCenter,
        BottomLeft,
        BottomRight,
        LeftCenter,
        RightCenter,
        Custom
    }

    public enum AnimationType
    {
        SlideAndFade,
        Slide,
        Fade
    }

    public enum FilterMode
    {
        None,
        Whitelist,
        Blacklist
    }

    public class AppSettings : INotifyPropertyChanged
    {
        private string _monitorDeviceName = "Primary";
        private PositionMode _positionMode = PositionMode.Simple;
        private SimpleZone _simpleZone = SimpleZone.Top;
        private AdvancedZone _advancedZone = AdvancedZone.TopCenter;
        private OverlayOrientation _orientation = OverlayOrientation.Horizontal;
        
        private double _marginX = 24.0;
        private double _marginY = 24.0;
        private double _customX = 100.0;
        private double _customY = 100.0;
        private double _cardWidth = 380.0;
        private double _cardHeight = 88.0;

        private GlassStyle _glassStyle = GlassStyle.iOSFrostedGlass;
        private double _cornerRadiusCardPercent = 45.0;
        private double _cornerRadiusArtPercent = 30.0;
        private double _cardOpacity = 0.90;
        private double _blurRadius = 30.0;
        
        private bool _useAdaptiveColor = true;
        private string _customBackgroundColor = "#1A1A24";
        private string _customAccentColor = "#8B5CF6";
        
        private bool _adaptiveGlowEnabled = true;
        private double _glowIntensity = 0.70;
        private double _glowRadius = 25.0;

        private double _borderWidth = 1.0;
        private string _borderColor = "#33FFFFFF";
        private bool _showDropShadow = true;

        // Passive State
        private bool _showArtworkPassive = true;
        private bool _showArtistPassive = true;
        private bool _showProgressBarPassive = false;
        private bool _showVolumeBadgePassive = false;
        private bool _showMediaControlsPassive = false;

        // Active State
        private bool _showArtworkActive = true;
        private bool _showArtistActive = true;
        private bool _showProgressBarActive = true;
        private bool _showVolumeBadgeActive = true;
        private bool _showMediaControlsActive = true;

        private int _hoverAnimationSpeedMs = 250;

        private InteractionMode _interactionMode = InteractionMode.Interactive;
        private bool _showOnTrackChange = true;
        private bool _showOnVolumeChange = true;
        private bool _showOnPlayPause = true;
        private bool _isPinned = false;
        private double _displayDurationSeconds = 4.0;
        private int _animationSpeedMs = 350;
        private AnimationType _enterAnimation = AnimationType.SlideAndFade;
        private AnimationType _exitAnimation = AnimationType.SlideAndFade;

        private bool _hotkeyEnabled = true;
        private string _hotkeyModifiers = "Control+Alt";
        private string _hotkeyKey = "M";

        private bool _autostart = false;
        private FilterMode _filterMode = FilterMode.None;
        private List<string> _appFilterList = new List<string> { "Spotify", "YandexMusic", "AppleMusic", "chrome", "msedge" };
        private string _language = "ru-RU";
        private string _activePresetName = "iOS Frosted";

        public string MonitorDeviceName { get => _monitorDeviceName; set => SetField(ref _monitorDeviceName, value); }
        public PositionMode PositionMode { get => _positionMode; set => SetField(ref _positionMode, value); }
        public SimpleZone SimpleZone { get => _simpleZone; set => SetField(ref _simpleZone, value); }
        public AdvancedZone AdvancedZone { get => _advancedZone; set => SetField(ref _advancedZone, value); }
        public OverlayOrientation Orientation { get => _orientation; set => SetField(ref _orientation, value); }
        
        public double MarginX { get => _marginX; set => SetField(ref _marginX, value); }
        public double MarginY { get => _marginY; set => SetField(ref _marginY, value); }
        public double CustomX { get => _customX; set => SetField(ref _customX, value); }
        public double CustomY { get => _customY; set => SetField(ref _customY, value); }
        public double CardWidth { get => _cardWidth; set => SetField(ref _cardWidth, value); }
        public double CardHeight { get => _cardHeight; set => SetField(ref _cardHeight, value); }

        public GlassStyle GlassStyle { get => _glassStyle; set => SetField(ref _glassStyle, value); }
        public double CornerRadiusCardPercent { get => _cornerRadiusCardPercent; set => SetField(ref _cornerRadiusCardPercent, value); }
        public double CornerRadiusArtPercent { get => _cornerRadiusArtPercent; set => SetField(ref _cornerRadiusArtPercent, value); }
        public double CardOpacity { get => _cardOpacity; set => SetField(ref _cardOpacity, value); }
        public double BlurRadius { get => _blurRadius; set => SetField(ref _blurRadius, value); }
        
        public bool UseAdaptiveColor { get => _useAdaptiveColor; set => SetField(ref _useAdaptiveColor, value); }
        public string CustomBackgroundColor { get => _customBackgroundColor; set => SetField(ref _customBackgroundColor, value); }
        public string CustomAccentColor { get => _customAccentColor; set => SetField(ref _customAccentColor, value); }
        
        public bool AdaptiveGlowEnabled { get => _adaptiveGlowEnabled; set => SetField(ref _adaptiveGlowEnabled, value); }
        public double GlowIntensity { get => _glowIntensity; set => SetField(ref _glowIntensity, value); }
        public double GlowRadius { get => _glowRadius; set => SetField(ref _glowRadius, value); }

        public double BorderWidth { get => _borderWidth; set => SetField(ref _borderWidth, value); }
        public string BorderColor { get => _borderColor; set => SetField(ref _borderColor, value); }
        public bool ShowDropShadow { get => _showDropShadow; set => SetField(ref _showDropShadow, value); }

        public bool ShowArtworkPassive { get => _showArtworkPassive; set => SetField(ref _showArtworkPassive, value); }
        public bool ShowArtistPassive { get => _showArtistPassive; set => SetField(ref _showArtistPassive, value); }
        public bool ShowProgressBarPassive { get => _showProgressBarPassive; set => SetField(ref _showProgressBarPassive, value); }
        public bool ShowVolumeBadgePassive { get => _showVolumeBadgePassive; set => SetField(ref _showVolumeBadgePassive, value); }
        public bool ShowMediaControlsPassive { get => _showMediaControlsPassive; set => SetField(ref _showMediaControlsPassive, value); }

        public bool ShowArtworkActive { get => _showArtworkActive; set => SetField(ref _showArtworkActive, value); }
        public bool ShowArtistActive { get => _showArtistActive; set => SetField(ref _showArtistActive, value); }
        public bool ShowProgressBarActive { get => _showProgressBarActive; set => SetField(ref _showProgressBarActive, value); }
        public bool ShowVolumeBadgeActive { get => _showVolumeBadgeActive; set => SetField(ref _showVolumeBadgeActive, value); }
        public bool ShowMediaControlsActive { get => _showMediaControlsActive; set => SetField(ref _showMediaControlsActive, value); }

        public int HoverAnimationSpeedMs { get => _hoverAnimationSpeedMs; set => SetField(ref _hoverAnimationSpeedMs, value); }

        public bool ShowArtwork { get => ShowArtworkPassive; set => ShowArtworkPassive = value; }
        public bool ShowArtist { get => ShowArtistPassive; set => ShowArtistPassive = value; }
        public bool ShowProgressBar { get => ShowProgressBarActive; set => ShowProgressBarActive = value; }
        public bool ShowVolumeBadge { get => ShowVolumeBadgeActive; set => ShowVolumeBadgeActive = value; }
        public bool ShowMediaControlsOnHover { get => ShowMediaControlsActive; set => ShowMediaControlsActive = value; }

        public InteractionMode InteractionMode { get => _interactionMode; set => SetField(ref _interactionMode, value); }
        public bool ShowOnTrackChange { get => _showOnTrackChange; set => SetField(ref _showOnTrackChange, value); }
        public bool ShowOnVolumeChange { get => _showOnVolumeChange; set => SetField(ref _showOnVolumeChange, value); }
        public bool ShowOnPlayPause { get => _showOnPlayPause; set => SetField(ref _showOnPlayPause, value); }
        public bool IsPinned { get => _isPinned; set => SetField(ref _isPinned, value); }
        public double DisplayDurationSeconds { get => _displayDurationSeconds; set => SetField(ref _displayDurationSeconds, value); }
        public int AnimationSpeedMs { get => _animationSpeedMs; set => SetField(ref _animationSpeedMs, value); }
        public AnimationType EnterAnimation { get => _enterAnimation; set => SetField(ref _enterAnimation, value); }
        public AnimationType ExitAnimation { get => _exitAnimation; set => SetField(ref _exitAnimation, value); }

        public bool HotkeyEnabled { get => _hotkeyEnabled; set => SetField(ref _hotkeyEnabled, value); }
        public string HotkeyModifiers { get => _hotkeyModifiers; set => SetField(ref _hotkeyModifiers, value); }
        public string HotkeyKey { get => _hotkeyKey; set => SetField(ref _hotkeyKey, value); }

        public bool Autostart { get => _autostart; set => SetField(ref _autostart, value); }
        public FilterMode FilterMode { get => _filterMode; set => SetField(ref _filterMode, value); }
        public List<string> AppFilterList { get => _appFilterList; set => SetField(ref _appFilterList, value); }
        public string Language { get => _language; set => SetField(ref _language, value); }
        public string ActivePresetName { get => _activePresetName; set => SetField(ref _activePresetName, value); }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public AppSettings Clone()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public void CopyFrom(AppSettings other)
        {
            MonitorDeviceName = other.MonitorDeviceName;
            PositionMode = other.PositionMode;
            SimpleZone = other.SimpleZone;
            AdvancedZone = other.AdvancedZone;
            Orientation = other.Orientation;
            MarginX = other.MarginX;
            MarginY = other.MarginY;
            CustomX = other.CustomX;
            CustomY = other.CustomY;
            CardWidth = other.CardWidth;
            CardHeight = other.CardHeight;
            GlassStyle = other.GlassStyle;
            CornerRadiusCardPercent = other.CornerRadiusCardPercent;
            CornerRadiusArtPercent = other.CornerRadiusArtPercent;
            CardOpacity = other.CardOpacity;
            BlurRadius = other.BlurRadius;
            UseAdaptiveColor = other.UseAdaptiveColor;
            CustomBackgroundColor = other.CustomBackgroundColor;
            CustomAccentColor = other.CustomAccentColor;
            AdaptiveGlowEnabled = other.AdaptiveGlowEnabled;
            GlowIntensity = other.GlowIntensity;
            GlowRadius = other.GlowRadius;
            BorderWidth = other.BorderWidth;
            BorderColor = other.BorderColor;
            ShowDropShadow = other.ShowDropShadow;
            ShowArtworkPassive = other.ShowArtworkPassive;
            ShowArtistPassive = other.ShowArtistPassive;
            ShowProgressBarPassive = other.ShowProgressBarPassive;
            ShowVolumeBadgePassive = other.ShowVolumeBadgePassive;
            ShowMediaControlsPassive = other.ShowMediaControlsPassive;
            ShowArtworkActive = other.ShowArtworkActive;
            ShowArtistActive = other.ShowArtistActive;
            ShowProgressBarActive = other.ShowProgressBarActive;
            ShowVolumeBadgeActive = other.ShowVolumeBadgeActive;
            ShowMediaControlsActive = other.ShowMediaControlsActive;
            HoverAnimationSpeedMs = other.HoverAnimationSpeedMs;
            InteractionMode = other.InteractionMode;
            ShowOnTrackChange = other.ShowOnTrackChange;
            ShowOnVolumeChange = other.ShowOnVolumeChange;
            ShowOnPlayPause = other.ShowOnPlayPause;
            IsPinned = other.IsPinned;
            DisplayDurationSeconds = other.DisplayDurationSeconds;
            AnimationSpeedMs = other.AnimationSpeedMs;
            EnterAnimation = other.EnterAnimation;
            ExitAnimation = other.ExitAnimation;
            HotkeyEnabled = other.HotkeyEnabled;
            HotkeyModifiers = other.HotkeyModifiers;
            HotkeyKey = other.HotkeyKey;
            Autostart = other.Autostart;
            Language = other.Language;
            ActivePresetName = other.ActivePresetName;
        }
    }
}