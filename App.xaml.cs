using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ShowMyMusic.Models;
using ShowMyMusic.Services;
using ShowMyMusic.ViewModels;
using ShowMyMusic.Views;

namespace ShowMyMusic
{
    public partial class App : Application
    {
        private static Mutex? _mutex;
        private TrayService? _trayService;
        private bool _disposed = false;
        private SettingsService _settingsService = null!;
        private AudioVolumeService _volumeService = null!;
        private MediaService _mediaService = null!;
        private HotkeyService _hotkeyService = null!;

        private OverlayViewModel _overlayViewModel = null!;
        private SettingsViewModel _settingsViewModel = null!;
        private OverlayWindow _overlayWindow = null!;
        private SettingsWindow _settingsWindow = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "ShowMyMusic_SingleInstance_Mutex_987654";
            _mutex = new Mutex(true, mutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                MessageBox.Show("ShowMyMusic is already running in the system tray!", "ShowMyMusic", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            base.OnStartup(e);

            // 1. Initialize core services
            _settingsService = new SettingsService();
            _settingsService.LoadSettings();

            _volumeService = new AudioVolumeService();
            _volumeService.Initialize();

            _mediaService = new MediaService();
            await _mediaService.InitializeAsync();

            _hotkeyService = new HotkeyService();

            // 2. Initialize view models
            _overlayViewModel = new OverlayViewModel(_mediaService, _volumeService, _settingsService);
            _settingsViewModel = new SettingsViewModel(_settingsService, _overlayViewModel);

            // 3. Initialize windows
            _overlayWindow = new OverlayWindow(_overlayViewModel);
            _settingsWindow = new SettingsWindow(_settingsViewModel);

            // Ensure overlay window handle is created for hotkey registration
            _overlayWindow.Show();
            _overlayWindow.Hide();

            var hwnd = new System.Windows.Interop.WindowInteropHelper(_overlayWindow).Handle;
            _hotkeyService.Initialize(hwnd);
            UpdateHotkey();

            _hotkeyService.HotkeyPressed += (s, ev) =>
            {
                if (_overlayViewModel.IsVisible)
                {
                    _overlayViewModel.HideOverlay();
                }
                else
                {
                    _overlayViewModel.ShowOverlay();
                }
            };

            // 4. Setup System Tray Icon via Win32 Shell_NotifyIcon & modern custom styled WPF ContextMenu
            SetupSystemTray();

            // Show settings window on first start if not launched with --minimized
            bool startMinimized = false;
            foreach (var arg in e.Args)
            {
                if (arg.Equals("--minimized", StringComparison.OrdinalIgnoreCase))
                {
                    startMinimized = true;
                    break;
                }
            }

            if (!startMinimized)
            {
                _settingsWindow.Show();
            }
        }

        private void UpdateHotkey()
        {
            if (_settingsService.CurrentSettings.HotkeyEnabled)
            {
                _hotkeyService.Register(_settingsService.CurrentSettings.HotkeyModifiers, _settingsService.CurrentSettings.HotkeyKey);
            }
            else
            {
                _hotkeyService.Unregister();
            }
        }

        private void SetupSystemTray()
        {
            var menu = new ContextMenu
            {
                MinWidth = 210
            };

            var headerItem = new MenuItem
            {
                Header = "🎵  ShowMyMusic",
                IsEnabled = false,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(168, 85, 247))
            };

            var settingsItem = new MenuItem { Header = "⚙  Settings..." };
            settingsItem.Click += (s, e) => ShowSettingsWindow();

            var toggleItem = new MenuItem { Header = "👁  Toggle Overlay" };
            toggleItem.Click += (s, e) =>
            {
                if (_overlayViewModel.IsVisible) _overlayViewModel.HideOverlay();
                else _overlayViewModel.ShowOverlay();
            };

            var playPauseItem = new MenuItem { Header = "⏯  Play / Pause" };
            playPauseItem.Click += async (s, e) => await _mediaService.TogglePlayPauseAsync();

            var nextItem = new MenuItem { Header = "⏭  Next Track" };
            nextItem.Click += async (s, e) => await _mediaService.SkipNextAsync();

            var pinnedItem = new MenuItem
            {
                Header = "📌  Pin on Screen",
                IsCheckable = true,
                IsChecked = _settingsService.CurrentSettings.IsPinned
            };
            pinnedItem.Click += (s, e) =>
            {
                _settingsService.CurrentSettings.IsPinned = pinnedItem.IsChecked;
                _settingsService.SaveSettings();
                if (pinnedItem.IsChecked) _overlayViewModel.ShowOverlay();
            };

            var exitItem = new MenuItem { Header = "❌  Exit" };
            exitItem.Click += (s, e) => ExitApplication();

            menu.Items.Add(headerItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(settingsItem);
            menu.Items.Add(toggleItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(playPauseItem);
            menu.Items.Add(nextItem);
            menu.Items.Add(pinnedItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(exitItem);

            _trayService = new TrayService();
            _trayService.Initialize(menu);
            _trayService.DoubleClick += (s, e) => ShowSettingsWindow();
            _trayService.Click += (s, e) => ShowSettingsWindow();
        }

        private void ShowSettingsWindow()
        {
            _settingsViewModel.RefreshScreens();
            _settingsViewModel.RefreshPresets();
            _settingsWindow.Show();
            _settingsWindow.WindowState = WindowState.Normal;
            _settingsWindow.Activate();
        }

        private void ExitApplication()
        {
            if (_disposed) return;
            _disposed = true;

            _trayService?.Dispose();
            _volumeService?.Dispose();
            _mediaService?.Dispose();
            _hotkeyService?.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayService?.Dispose();
            _volumeService?.Dispose();
            _mediaService?.Dispose();
            _hotkeyService?.Dispose();
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}