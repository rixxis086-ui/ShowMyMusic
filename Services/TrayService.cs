using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShowMyMusic.Services
{
    public class TrayService : IDisposable
    {
        private const int WM_USER = 0x0400;
        private const int WM_TRAYICON = WM_USER + 101;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONUP = 0x0205;

        private const int NIM_ADD = 0x00000000;
        private const int NIM_MODIFY = 0x00000001;
        private const int NIM_DELETE = 0x00000002;

        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;

        private const uint TRAY_ID = 1001;

        private HwndSource? _hwndSource;
        private IntPtr _hIcon = IntPtr.Zero;
        private ContextMenu? _contextMenu;
        private bool _isCreated = false;
        private bool _disposed = false;

        public event EventHandler? DoubleClick;
        public event EventHandler? Click;

        public void Initialize(ContextMenu contextMenu)
        {
            _contextMenu = contextMenu;

            var parameters = new HwndSourceParameters("ShowMyMusicTrayReceiver")
            {
                WindowStyle = 0,
                ExtendedWindowStyle = 0x00000080, // WS_EX_TOOLWINDOW
                Width = 0,
                Height = 0,
                PositionX = 0,
                PositionY = 0
            };

            _hwndSource = new HwndSource(parameters);
            _hwndSource.AddHook(WndProc);

            _hIcon = CreateDefaultIcon();

            var nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                hWnd = _hwndSource.Handle,
                uID = TRAY_ID,
                uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                uCallbackMessage = WM_TRAYICON,
                hIcon = _hIcon,
                szTip = "ShowMyMusic — Music Overlay"
            };

            _isCreated = Shell_NotifyIcon(NIM_ADD, ref nid);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_TRAYICON && wParam.ToInt32() == TRAY_ID)
            {
                int mouseMsg = lParam.ToInt32();
                if (mouseMsg == WM_LBUTTONUP)
                {
                    Click?.Invoke(this, EventArgs.Empty);
                }
                else if (mouseMsg == WM_LBUTTONDBLCLK)
                {
                    DoubleClick?.Invoke(this, EventArgs.Empty);
                }
                else if (mouseMsg == WM_RBUTTONUP)
                {
                    ShowContextMenu();
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void ShowContextMenu()
        {
            if (_contextMenu == null || _hwndSource == null) return;

            GetCursorPos(out var pt);
            SetForegroundWindow(_hwndSource.Handle);

            _contextMenu.IsOpen = false;
            _contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
            _contextMenu.HorizontalOffset = pt.X;
            _contextMenu.VerticalOffset = pt.Y;
            _contextMenu.IsOpen = true;
        }

        private IntPtr CreateDefaultIcon()
        {
            int size = 32;
            var visual = new DrawingVisual();
            using (var ctx = visual.RenderOpen())
            {
                // Gradient circle
                var brush = new LinearGradientBrush(
                    System.Windows.Media.Color.FromRgb(139, 92, 246), 
                    System.Windows.Media.Color.FromRgb(236, 72, 153), 
                    new System.Windows.Point(0, 0), 
                    new System.Windows.Point(1, 1));
                ctx.DrawEllipse(brush, null, new System.Windows.Point(size / 2.0, size / 2.0), size / 2.0 - 1, size / 2.0 - 1);

                // Music Note glyph
                var noteBrush = new SolidColorBrush(Colors.White);
                var pen = new System.Windows.Media.Pen(noteBrush, 2.5);
                ctx.DrawEllipse(noteBrush, null, new System.Windows.Point(11, 20), 3.5, 2.5);
                ctx.DrawEllipse(noteBrush, null, new System.Windows.Point(21, 17), 3.5, 2.5);
                ctx.DrawLine(pen, new System.Windows.Point(14.5, 20), new System.Windows.Point(14.5, 9));
                ctx.DrawLine(pen, new System.Windows.Point(24.5, 17), new System.Windows.Point(24.5, 6));
                ctx.DrawLine(new System.Windows.Media.Pen(noteBrush, 3.5), new System.Windows.Point(14.5, 9), new System.Windows.Point(24.5, 6));
            }

            var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            rtb.Freeze();

            int stride = size * 4;
            byte[] pixelData = new byte[size * stride];
            rtb.CopyPixels(pixelData, stride, 0);

            IntPtr hBitmap = CreateBitmap(size, size, 1, 32, pixelData);
            IntPtr hMonoMask = CreateBitmap(size, size, 1, 1, IntPtr.Zero);

            var iconInfo = new ICONINFO
            {
                fIcon = true,
                hbmColor = hBitmap,
                hbmMask = hMonoMask
            };

            IntPtr hIcon = CreateIconIndirect(ref iconInfo);

            DeleteObject(hBitmap);
            DeleteObject(hMonoMask);

            return hIcon;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_isCreated && _hwndSource != null)
            {
                var nid = new NOTIFYICONDATA
                {
                    cbSize = Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                    hWnd = _hwndSource.Handle,
                    uID = TRAY_ID
                };
                Shell_NotifyIcon(NIM_DELETE, ref nid);
                _isCreated = false;
            }

            if (_hIcon != IntPtr.Zero)
            {
                DestroyIcon(_hIcon);
                _hIcon = IntPtr.Zero;
            }

            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
                _hwndSource.Dispose();
                _hwndSource = null;
            }
        }

        #region Win32 P/Invoke
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct NOTIFYICONDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public uint uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, [In] byte[] lpvBits);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref ICONINFO piconinfo);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        #endregion
    }
}
