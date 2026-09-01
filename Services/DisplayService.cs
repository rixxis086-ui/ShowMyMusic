using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using ShowMyMusic.Models;

namespace ShowMyMusic.Services
{
    public class DisplayService
    {
        public const double ShadowPadding = 30.0;

        public static List<ScreenInfo> GetScreens()
        {
            var screens = new List<ScreenInfo>();
            int index = 1;

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RectStruct lprcMonitor, IntPtr dwData) =>
            {
                var mi = new MONITORINFOEX();
                mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));

                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    bool isPrimary = (mi.dwFlags & 1) != 0;
                    string devName = new string(mi.szDevice).TrimEnd('\0');

                    // Compute per-monitor DPI
                    double scaleX = 1.0;
                    double scaleY = 1.0;
                    try
                    {
                        if (GetDpiForMonitor(hMonitor, MonitorDpiType.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY) == 0 && dpiX > 0)
                        {
                            scaleX = dpiX / 96.0;
                            scaleY = dpiY / 96.0;
                        }
                    }
                    catch
                    {
                        scaleX = 1.0;
                        scaleY = 1.0;
                    }

                    int monW = mi.rcMonitor.Right - mi.rcMonitor.Left;
                    int monH = mi.rcMonitor.Bottom - mi.rcMonitor.Top;
                    int workW = mi.rcWork.Right - mi.rcWork.Left;
                    int workH = mi.rcWork.Bottom - mi.rcWork.Top;

                    screens.Add(new ScreenInfo
                    {
                        DeviceName = devName,
                        DisplayName = isPrimary ? $"Display {index} (Primary)" : $"Display {index}",
                        IsPrimary = isPrimary,
                        PixelX = mi.rcMonitor.Left,
                        PixelY = mi.rcMonitor.Top,
                        PixelWidth = monW,
                        PixelHeight = monH,
                        PixelWorkAreaX = mi.rcWork.Left,
                        PixelWorkAreaY = mi.rcWork.Top,
                        PixelWorkAreaWidth = workW,
                        PixelWorkAreaHeight = workH,
                        DpiScaleX = scaleX,
                        DpiScaleY = scaleY
                    });
                    index++;
                }
                return true;
            }, IntPtr.Zero);

            if (screens.Count == 0)
            {
                screens.Add(new ScreenInfo
                {
                    DeviceName = "Primary",
                    DisplayName = "Primary Display",
                    IsPrimary = true,
                    PixelX = 0,
                    PixelY = 0,
                    PixelWidth = (int)SystemParameters.PrimaryScreenWidth,
                    PixelHeight = (int)SystemParameters.PrimaryScreenHeight,
                    PixelWorkAreaX = (int)SystemParameters.WorkArea.Left,
                    PixelWorkAreaY = (int)SystemParameters.WorkArea.Top,
                    PixelWorkAreaWidth = (int)SystemParameters.WorkArea.Width,
                    PixelWorkAreaHeight = (int)SystemParameters.WorkArea.Height,
                    DpiScaleX = 1.0,
                    DpiScaleY = 1.0
                });
            }

            return screens;
        }

        public static (double DipLeft, double DipTop, int PixelLeft, int PixelTop, int PixelWidth, int PixelHeight) CalculateWindowBounds(
            AppSettings settings, 
            List<ScreenInfo> screens)
        {
            var screen = screens.Find(s => s.DeviceName == settings.MonitorDeviceName) 
                         ?? screens.Find(s => s.IsPrimary) 
                         ?? screens[0];

            double cardW = settings.CardWidth;
            double cardH = settings.CardHeight;

            double cardDipX = screen.DipWorkAreaX;
            double cardDipY = screen.DipWorkAreaY;

            if (settings.PositionMode == PositionMode.Simple)
            {
                switch (settings.SimpleZone)
                {
                    case SimpleZone.Top:
                        cardDipX = screen.DipWorkAreaX + (screen.DipWorkAreaWidth - cardW) / 2.0;
                        cardDipY = screen.DipWorkAreaY + settings.MarginY;
                        break;
                    case SimpleZone.Bottom:
                        cardDipX = screen.DipWorkAreaX + (screen.DipWorkAreaWidth - cardW) / 2.0;
                        cardDipY = screen.DipWorkAreaY + screen.DipWorkAreaHeight - cardH - settings.MarginY;
                        break;
                    case SimpleZone.Left:
                        cardDipX = screen.DipWorkAreaX + settings.MarginX;
                        cardDipY = screen.DipWorkAreaY + (screen.DipWorkAreaHeight - cardH) / 2.0;
                        break;
                    case SimpleZone.Right:
                        cardDipX = screen.DipWorkAreaX + screen.DipWorkAreaWidth - cardW - settings.MarginX;
                        cardDipY = screen.DipWorkAreaY + (screen.DipWorkAreaHeight - cardH) / 2.0;
                        break;
                }
            }
            else
            {
                switch (settings.AdvancedZone)
                {
                    case AdvancedZone.TopCenter:
                        cardDipX = screen.DipWorkAreaX + (screen.DipWorkAreaWidth - cardW) / 2.0;
                        cardDipY = screen.DipWorkAreaY + settings.MarginY;
                        break;
                    case AdvancedZone.TopLeft:
                        cardDipX = screen.DipWorkAreaX + settings.MarginX;
                        cardDipY = screen.DipWorkAreaY + settings.MarginY;
                        break;
                    case AdvancedZone.TopRight:
                        cardDipX = screen.DipWorkAreaX + screen.DipWorkAreaWidth - cardW - settings.MarginX;
                        cardDipY = screen.DipWorkAreaY + settings.MarginY;
                        break;
                    case AdvancedZone.BottomCenter:
                        cardDipX = screen.DipWorkAreaX + (screen.DipWorkAreaWidth - cardW) / 2.0;
                        cardDipY = screen.DipWorkAreaY + screen.DipWorkAreaHeight - cardH - settings.MarginY;
                        break;
                    case AdvancedZone.BottomLeft:
                        cardDipX = screen.DipWorkAreaX + settings.MarginX;
                        cardDipY = screen.DipWorkAreaY + screen.DipWorkAreaHeight - cardH - settings.MarginY;
                        break;
                    case AdvancedZone.BottomRight:
                        cardDipX = screen.DipWorkAreaX + screen.DipWorkAreaWidth - cardW - settings.MarginX;
                        cardDipY = screen.DipWorkAreaY + screen.DipWorkAreaHeight - cardH - settings.MarginY;
                        break;
                    case AdvancedZone.LeftCenter:
                        cardDipX = screen.DipWorkAreaX + settings.MarginX;
                        cardDipY = screen.DipWorkAreaY + (screen.DipWorkAreaHeight - cardH) / 2.0;
                        break;
                    case AdvancedZone.RightCenter:
                        cardDipX = screen.DipWorkAreaX + screen.DipWorkAreaWidth - cardW - settings.MarginX;
                        cardDipY = screen.DipWorkAreaY + (screen.DipWorkAreaHeight - cardH) / 2.0;
                        break;
                    case AdvancedZone.Custom:
                        cardDipX = screen.DipX + settings.CustomX;
                        cardDipY = screen.DipY + settings.CustomY;
                        break;
                }
            }

            // Window includes shadow padding so glow is never clipped
            double windowDipX = cardDipX - ShadowPadding;
            double windowDipY = cardDipY - ShadowPadding;
            double windowDipW = cardW + (ShadowPadding * 2);
            double windowDipH = cardH + (ShadowPadding * 2);

            int pixelX = (int)Math.Round(windowDipX * screen.DpiScaleX);
            int pixelY = (int)Math.Round(windowDipY * screen.DpiScaleY);
            int pixelW = (int)Math.Round(windowDipW * screen.DpiScaleX);
            int pixelH = (int)Math.Round(windowDipH * screen.DpiScaleY);

            return (windowDipX, windowDipY, pixelX, pixelY, pixelW, pixelH);
        }

        #region Win32 P/Invoke
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RectStruct lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("SHCore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

        private enum MonitorDpiType
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFOEX
        {
            public int cbSize;
            public RectStruct rcMonitor;
            public RectStruct rcWork;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RectStruct
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion
    }
}