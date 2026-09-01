using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ShowMyMusic.Helpers;
using ShowMyMusic.Models;
using ShowMyMusic.Services;

namespace ShowMyMusic.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private readonly OverlayViewModel _overlayViewModel;
        private TrackInfo _previewTrack;
        private ScreenInfo? _selectedScreen;
        private PresetTheme? _selectedPreset;
        private int _sampleTrackIndex = 0;

        public AppSettings Settings => _settingsService.CurrentSettings;

        public TrackInfo PreviewTrack
        {
            get => _previewTrack;
            set { if (_previewTrack != value) { _previewTrack = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<ScreenInfo> Screens { get; } = new();
        public ObservableCollection<PresetTheme> Presets { get; } = new();

        public ScreenInfo? SelectedScreen
        {
            get => _selectedScreen;
            set
            {
                if (_selectedScreen != value)
                {
                    _selectedScreen = value;
                    OnPropertyChanged();
                    if (value != null)
                    {
                        Settings.MonitorDeviceName = value.DeviceName;
                        SaveSettings();
                    }
                }
            }
        }

        public PresetTheme? SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (_selectedPreset != value)
                {
                    _selectedPreset = value;
                    OnPropertyChanged();
                    if (value != null)
                    {
                        ApplyPreset(value);
                    }
                }
            }
        }

        public ICommand SaveSettingsCommand { get; }
        public ICommand ExportSettingsCommand { get; }
        public ICommand ImportSettingsCommand { get; }
        public ICommand ResetDefaultsCommand { get; }
        public ICommand TestShowOverlayCommand { get; }
        public ICommand NextSampleTrackCommand { get; }
        public ICommand SaveCustomPresetCommand { get; }

        public SettingsViewModel(SettingsService settingsService, OverlayViewModel overlayViewModel)
        {
            _settingsService = settingsService;
            _overlayViewModel = overlayViewModel;

            _previewTrack = new TrackInfo
            {
                Title = "Blinding Lights",
                Artist = "The Weeknd",
                AlbumTitle = "After Hours",
                AppSource = "Spotify",
                IsPlaying = true,
                Duration = TimeSpan.FromSeconds(200),
                Position = TimeSpan.FromSeconds(75),
                VolumePercent = 70,
                Thumbnail = CreateSampleAlbumArt(Color.FromRgb(220, 38, 38), Color.FromRgb(147, 51, 234))
            };

            // Reactively sync changes from UI sliders to overlay and preview in real-time
            Settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AppSettings.UseAdaptiveColor) || 
                    e.PropertyName == nameof(AppSettings.CustomAccentColor) ||
                    e.PropertyName == nameof(AppSettings.GlowIntensity))
                {
                    UpdatePreviewColorsAsync();
                }
                SaveSettings();
            };

            RefreshScreens();
            RefreshPresets();

            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
            ExportSettingsCommand = new RelayCommand(_ => ExportSettings());
            ImportSettingsCommand = new RelayCommand(_ => ImportSettings());
            ResetDefaultsCommand = new RelayCommand(_ => ResetDefaults());
            TestShowOverlayCommand = new RelayCommand(_ =>
            {
                SaveSettings();
                _overlayViewModel.ShowOverlay();
            });
            NextSampleTrackCommand = new RelayCommand(_ => CycleSampleTrack());
            SaveCustomPresetCommand = new RelayCommand(_ => PromptSavePreset());

            UpdatePreviewColorsAsync();
        }

        public void RefreshScreens()
        {
            Screens.Clear();
            var screens = DisplayService.GetScreens();
            foreach (var s in screens)
            {
                Screens.Add(s);
            }

            SelectedScreen = Screens.Count > 0 
                ? (screens.Find(s => s.DeviceName == Settings.MonitorDeviceName) ?? screens[0]) 
                : null;
        }

        public void RefreshPresets()
        {
            Presets.Clear();
            foreach (var p in _settingsService.AllPresets)
            {
                Presets.Add(p);
            }
        }

        public void ApplyPreset(PresetTheme preset)
        {
            preset.ApplyTo(Settings);
            UpdatePreviewColorsAsync();
            SaveSettings();
        }

        public async void UpdatePreviewColorsAsync()
        {
            if (Settings.UseAdaptiveColor)
            {
                var fallbackColor = (Color)ColorConverter.ConvertFromString(Settings.CustomAccentColor);
                var (accent, glow) = await ColorExtractorService.ExtractColorsAsync(
                    PreviewTrack.Thumbnail, 
                    PreviewTrack.ThumbnailHash, 
                    Settings.GlowIntensity, 
                    fallbackColor);

                PreviewTrack.AccentColor = accent;
                PreviewTrack.GlowColor = glow;
            }
            else
            {
                var customAccent = (Color)ColorConverter.ConvertFromString(Settings.CustomAccentColor);
                byte glowAlpha = (byte)Math.Clamp(Settings.GlowIntensity * 255, 0, 255);
                PreviewTrack.AccentColor = customAccent;
                PreviewTrack.GlowColor = Color.FromArgb(glowAlpha, customAccent.R, customAccent.G, customAccent.B);
            }
        }

        private void CycleSampleTrack()
        {
            _sampleTrackIndex = (_sampleTrackIndex + 1) % 3;
            if (_sampleTrackIndex == 0)
            {
                PreviewTrack.Title = "Blinding Lights";
                PreviewTrack.Artist = "The Weeknd";
                PreviewTrack.AppSource = "Spotify";
                PreviewTrack.Thumbnail = CreateSampleAlbumArt(Color.FromRgb(220, 38, 38), Color.FromRgb(147, 51, 234));
            }
            else if (_sampleTrackIndex == 1)
            {
                PreviewTrack.Title = "Midnight City";
                PreviewTrack.Artist = "M83";
                PreviewTrack.AppSource = "Яндекс Музыка";
                PreviewTrack.Thumbnail = CreateSampleAlbumArt(Color.FromRgb(6, 182, 212), Color.FromRgb(59, 130, 246));
            }
            else
            {
                PreviewTrack.Title = "Cyberpunk 2077 Theme";
                PreviewTrack.Artist = "Marcin Przybyłowicz";
                PreviewTrack.AppSource = "Apple Music";
                PreviewTrack.Thumbnail = CreateSampleAlbumArt(Color.FromRgb(234, 179, 8), Color.FromRgb(244, 63, 94));
            }

            PreviewTrack.ThumbnailHash = $"{PreviewTrack.Title}|{PreviewTrack.Artist}";
            UpdatePreviewColorsAsync();
        }

        private static BitmapSource CreateSampleAlbumArt(Color start, Color end)
        {
            int width = 120;
            int height = 120;
            var drawingVisual = new DrawingVisual();
            using (var ctx = drawingVisual.RenderOpen())
            {
                var gradient = new LinearGradientBrush(start, end, new Point(0, 0), new Point(1, 1));
                ctx.DrawRectangle(gradient, null, new Rect(0, 0, width, height));

                var pen = new Pen(new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)), 4);
                ctx.DrawEllipse(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)), null, new Point(45, 75), 14, 10);
                ctx.DrawEllipse(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)), null, new Point(80, 65), 14, 10);
                ctx.DrawLine(pen, new Point(55, 75), new Point(55, 35));
                ctx.DrawLine(pen, new Point(90, 65), new Point(90, 25));
                ctx.DrawLine(new Pen(pen.Brush, 6), new Point(55, 35), new Point(90, 25));
            }

            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            bmp.Freeze();
            return bmp;
        }

        public void SaveSettings()
        {
            AutostartService.SetAutostart(Settings.Autostart);
            _settingsService.SaveSettings();
        }

        private void ExportSettings()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "ShowMyMusic Config (*.json)|*.json",
                FileName = "ShowMyMusic_Theme.json"
            };

            if (dialog.ShowDialog() == true)
            {
                _settingsService.ExportSettings(dialog.FileName);
                MessageBox.Show("Settings exported successfully!", "ShowMyMusic", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ImportSettings()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "ShowMyMusic Config (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                _settingsService.ImportSettings(dialog.FileName);
                OnPropertyChanged(nameof(Settings));
                UpdatePreviewColorsAsync();
                MessageBox.Show("Settings imported successfully!", "ShowMyMusic", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResetDefaults()
        {
            if (MessageBox.Show("Reset all settings to default?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Settings.CopyFrom(new AppSettings());
                SaveSettings();
                UpdatePreviewColorsAsync();
            }
        }

        private void PromptSavePreset()
        {
            string name = $"Theme {DateTime.Now:HH:mm:ss}";
            var newPreset = new PresetTheme
            {
                Name = name,
                Description = "Custom user preset",
                GlassStyle = Settings.GlassStyle,
                CornerRadiusCardPercent = Settings.CornerRadiusCardPercent,
                CornerRadiusArtPercent = Settings.CornerRadiusArtPercent,
                CardOpacity = Settings.CardOpacity,
                BlurRadius = Settings.BlurRadius,
                UseAdaptiveColor = Settings.UseAdaptiveColor,
                BackgroundColor = Settings.CustomBackgroundColor,
                AccentColor = Settings.CustomAccentColor,
                AdaptiveGlowEnabled = Settings.AdaptiveGlowEnabled,
                GlowIntensity = Settings.GlowIntensity,
                GlowRadius = Settings.GlowRadius,
                BorderWidth = Settings.BorderWidth,
                BorderColor = Settings.BorderColor
            };

            _settingsService.SaveCustomPreset(newPreset);
            RefreshPresets();
            MessageBox.Show($"Пресет \"{name}\" сохранен!", "ShowMyMusic", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}