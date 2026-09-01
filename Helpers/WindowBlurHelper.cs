using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ShowMyMusic.Models;

namespace ShowMyMusic.Helpers
{
    public static class WindowBlurHelper
    {
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void ApplyBackdrop(Window window, GlassStyle style, double cardWidth = 380, double cardHeight = 88, double cornerRadiusPercent = 45)
        {
            var hwnd = new WindowInteropHelper(window).EnsureHandle();
            if (hwnd == IntPtr.Zero) return;

            try
            {
                int isDarkMode = (style == GlassStyle.LightGlass) ? 0 : 1;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref isDarkMode, sizeof(int));

                // Always enforce DWMSBT_NONE on transparent floating window to prevent DWM rectangular box artifact
                if (Environment.OSVersion.Version.Build >= 22000)
                {
                    int val = 1; // DWMSBT_NONE
                    DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref val, sizeof(int));
                }

                SetWindowCompositionAttribute(hwnd, AccentState.ACCENT_DISABLED, 0);
            }
            catch { }
        }

        #region Win32 Composition P/Invoke
        private enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public uint AccentFlags;
            public uint GradientColor;
            public uint AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private static void SetWindowCompositionAttribute(IntPtr hwnd, AccentState state, uint color)
        {
            var policy = new AccentPolicy
            {
                AccentState = state,
                AccentFlags = 0,
                GradientColor = color
            };

            int size = Marshal.SizeOf(policy);
            IntPtr pMem = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(policy, pMem, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = 19, // WCA_ACCENT_POLICY
                Data = pMem,
                SizeOfData = size
            };

            SetWindowCompositionAttribute(hwnd, ref data);
            Marshal.FreeHGlobal(pMem);
        }
        #endregion
    }
}