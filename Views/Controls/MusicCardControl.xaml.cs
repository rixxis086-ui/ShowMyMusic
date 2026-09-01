using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ShowMyMusic.Models;

namespace ShowMyMusic.Views.Controls
{
    public partial class MusicCardControl : UserControl
    {
        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register(
            nameof(Track), typeof(TrackInfo), typeof(MusicCardControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings), typeof(AppSettings), typeof(MusicCardControl), new PropertyMetadata(null, OnSettingsChangedStatic));

        public static readonly DependencyProperty TogglePlayPauseCommandProperty = DependencyProperty.Register(
            nameof(TogglePlayPauseCommand), typeof(ICommand), typeof(MusicCardControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SkipNextCommandProperty = DependencyProperty.Register(
            nameof(SkipNextCommand), typeof(ICommand), typeof(MusicCardControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SkipPreviousCommandProperty = DependencyProperty.Register(
            nameof(SkipPreviousCommand), typeof(ICommand), typeof(MusicCardControl), new PropertyMetadata(null));

        public static readonly DependencyProperty IsHoveredProperty = DependencyProperty.Register(
            nameof(IsHovered), typeof(bool), typeof(MusicCardControl), new PropertyMetadata(false, OnIsHoveredChangedStatic));

        public TrackInfo? Track
        {
            get => (TrackInfo?)GetValue(TrackProperty);
            set => SetValue(TrackProperty, value);
        }

        public AppSettings? Settings
        {
            get => (AppSettings?)GetValue(SettingsProperty);
            set => SetValue(SettingsProperty, value);
        }

        public ICommand? TogglePlayPauseCommand
        {
            get => (ICommand?)GetValue(TogglePlayPauseCommandProperty);
            set => SetValue(TogglePlayPauseCommandProperty, value);
        }

        public ICommand? SkipNextCommand
        {
            get => (ICommand?)GetValue(SkipNextCommandProperty);
            set => SetValue(SkipNextCommandProperty, value);
        }

        public ICommand? SkipPreviousCommand
        {
            get => (ICommand?)GetValue(SkipPreviousCommandProperty);
            set => SetValue(SkipPreviousCommandProperty, value);
        }

        public bool IsHovered
        {
            get => (bool)GetValue(IsHoveredProperty);
            set => SetValue(IsHoveredProperty, value);
        }

        private AppSettings? _subscribedSettings;

        public MusicCardControl()
        {
            InitializeComponent();
            Loaded += (s, e) => ApplyVisualStates(false);
        }

        private static void OnSettingsChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not MusicCardControl control) return;

            if (control._subscribedSettings != null)
                control._subscribedSettings.PropertyChanged -= control.OnSettingsPropertyChanged;

            control._subscribedSettings = e.NewValue as AppSettings;

            if (control._subscribedSettings != null)
                control._subscribedSettings.PropertyChanged += control.OnSettingsPropertyChanged;

            control.ApplyVisualStates(false);
        }

        private void OnSettingsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AppSettings.ShowArtworkPassive):
                case nameof(AppSettings.ShowArtworkActive):
                case nameof(AppSettings.ShowArtistPassive):
                case nameof(AppSettings.ShowArtistActive):
                case nameof(AppSettings.ShowProgressBarPassive):
                case nameof(AppSettings.ShowProgressBarActive):
                case nameof(AppSettings.ShowVolumeBadgePassive):
                case nameof(AppSettings.ShowVolumeBadgeActive):
                case nameof(AppSettings.ShowMediaControlsPassive):
                case nameof(AppSettings.ShowMediaControlsActive):
                case nameof(AppSettings.InteractionMode):
                    ApplyVisualStates(false);
                    break;
            }
        }

        private static void OnIsHoveredChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MusicCardControl control)
                control.ApplyVisualStates(true);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Settings?.InteractionMode == InteractionMode.Interactive)
                IsHovered = true;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Settings?.InteractionMode == InteractionMode.Interactive)
                IsHovered = false;
        }

        public void ApplyVisualStates(bool animate)
        {
            if (Settings == null) return;

            bool isInteractive = Settings.InteractionMode == InteractionMode.Interactive;
            bool active = isInteractive && IsHovered;

            bool showArt      = active ? Settings.ShowArtworkActive        : Settings.ShowArtworkPassive;
            bool showArtist   = active ? Settings.ShowArtistActive         : Settings.ShowArtistPassive;
            bool showProgress = active ? Settings.ShowProgressBarActive    : Settings.ShowProgressBarPassive;
            bool showVolume   = active ? Settings.ShowVolumeBadgeActive    : Settings.ShowVolumeBadgePassive;
            bool showControls = active ? Settings.ShowMediaControlsActive  : Settings.ShowMediaControlsPassive;

            SetElementState(ArtworkContainer,    showArt,      animate);
            SetElementState(ArtistTextBlock,     showArtist,   animate);
            SetElementState(ProgressBarContainer,showProgress, animate);
            SetElementState(VolumeContainer,     showVolume,   animate);
            SetElementState(ControlsContainer,   showControls, animate);
        }

        private void SetElementState(UIElement? element, bool isVisible, bool animate)
        {
            if (element == null) return;

            int durationMs = Settings?.HoverAnimationSpeedMs ?? 250;
            var duration = TimeSpan.FromMilliseconds(Math.Max(50, durationMs));

            if (!animate)
            {
                element.Opacity = isVisible ? 1.0 : 0.0;
                element.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            if (isVisible)
            {
                element.Visibility = Visibility.Visible;
                var anim = new DoubleAnimation
                {
                    From = element.Opacity,
                    To = 1.0,
                    Duration = duration,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                element.BeginAnimation(UIElement.OpacityProperty, anim);
            }
            else
            {
                var anim = new DoubleAnimation
                {
                    From = element.Opacity,
                    To = 0.0,
                    Duration = duration,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                anim.Completed += (s, e) =>
                {
                    if (element.Opacity < 0.01)
                        element.Visibility = Visibility.Collapsed;
                };
                element.BeginAnimation(UIElement.OpacityProperty, anim);
            }
        }
    }
}