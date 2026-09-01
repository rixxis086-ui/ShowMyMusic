using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ShowMyMusic.Helpers;
using ShowMyMusic.Models;
using ShowMyMusic.Services;
using ShowMyMusic.ViewModels;

namespace ShowMyMusic.Views
{
    public partial class OverlayWindow : Window
    {
        private readonly OverlayViewModel _viewModel;
        private bool _isAnimatingOut = false;

        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_NOZORDER = 0x0004;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private static readonly System.Collections.Generic.HashSet<string> _positionProps = new()
        {
            nameof(AppSettings.PositionMode), nameof(AppSettings.SimpleZone), nameof(AppSettings.AdvancedZone),
            nameof(AppSettings.MarginX), nameof(AppSettings.MarginY), nameof(AppSettings.CustomX),
            nameof(AppSettings.CustomY), nameof(AppSettings.CardWidth), nameof(AppSettings.CardHeight),
            nameof(AppSettings.MonitorDeviceName)
        };

        private static readonly System.Collections.Generic.HashSet<string> _styleProps = new()
        {
            nameof(AppSettings.GlassStyle), nameof(AppSettings.InteractionMode)
        };

        public OverlayWindow(OverlayViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.RequestShow += OnRequestShow;
            _viewModel.RequestHide += OnRequestHide;

            _viewModel.Settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == null) return;
                Dispatcher.Invoke(() =>
                {
                    if (_positionProps.Contains(e.PropertyName))
                        UpdatePosition();
                    if (_styleProps.Contains(e.PropertyName))
                        ApplyWindowStyles();
                });
            };

            Loaded += OverlayWindow_Loaded;
        }

        private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePosition();
            ApplyWindowStyles();

            if (_viewModel.Settings.IsPinned)
            {
                Show();
                _viewModel.ShowOverlay();
            }
            else
            {
                Hide();
            }
        }

        public void UpdatePosition()
        {
            var screens = DisplayService.GetScreens();
            var bounds = DisplayService.CalculateWindowBounds(_viewModel.Settings, screens);

            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                SetWindowPos(hwnd, HWND_TOPMOST, bounds.PixelLeft, bounds.PixelTop, bounds.PixelWidth, bounds.PixelHeight, SWP_NOACTIVATE | SWP_NOZORDER);
            }

            Left = bounds.DipLeft;
            Top = bounds.DipTop;
        }

        public void ApplyWindowStyles()
        {
            bool isClickThrough = _viewModel.Settings.InteractionMode == InteractionMode.Passive;
            WindowStyleHelper.SetClickThrough(this, isClickThrough);
            WindowStyleHelper.EnsureTopmost(this);
            WindowBlurHelper.ApplyBackdrop(
                this,
                _viewModel.Settings.GlassStyle,
                _viewModel.Settings.CardWidth,
                _viewModel.Settings.CardHeight,
                _viewModel.Settings.CornerRadiusCardPercent);
        }

        private void OnRequestShow(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _isAnimatingOut = false;
                UpdatePosition();
                ApplyWindowStyles();
                Show();

                var zone = _viewModel.Settings.PositionMode == PositionMode.Simple
                    ? _viewModel.Settings.SimpleZone
                    : (_viewModel.Settings.AdvancedZone.ToString().StartsWith("Bottom") ? SimpleZone.Bottom : SimpleZone.Top);

                AnimationHelper.AnimateIn(
                    OverlayContainer,
                    _viewModel.Settings.EnterAnimation,
                    zone,
                    _viewModel.Settings.AnimationSpeedMs);
            });
        }

        private void OnRequestHide(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_isAnimatingOut) return;
                _isAnimatingOut = true;

                var zone = _viewModel.Settings.PositionMode == PositionMode.Simple
                    ? _viewModel.Settings.SimpleZone
                    : (_viewModel.Settings.AdvancedZone.ToString().StartsWith("Bottom") ? SimpleZone.Bottom : SimpleZone.Top);

                AnimationHelper.AnimateOut(
                    OverlayContainer,
                    _viewModel.Settings.ExitAnimation,
                    zone,
                    _viewModel.Settings.AnimationSpeedMs,
                    () =>
                    {
                        if (_isAnimatingOut)
                        {
                            Hide();
                            _isAnimatingOut = false;
                        }
                    });
            });
        }

        private void OverlayContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            _viewModel.OnMouseEnter();
        }

        private void OverlayContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            _viewModel.OnMouseLeave();
        }

        private void OverlayContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.Settings.InteractionMode == InteractionMode.Interactive)
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    try
                    {
                        DragMove();
                        _viewModel.Settings.PositionMode = PositionMode.Advanced;
                        _viewModel.Settings.AdvancedZone = AdvancedZone.Custom;
                        _viewModel.Settings.CustomX = Left;
                        _viewModel.Settings.CustomY = Top;
                    }
                    catch { }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    }
}