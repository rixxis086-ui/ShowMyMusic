using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ShowMyMusic.Helpers
{
    public static class WindowStyleHelper
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_LAYERED = 0x00080000;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOZORDER = 0x0004;

        public static void SetClickThrough(Window window, bool isClickThrough)
        {
            var hwnd = new WindowInteropHelper(window).EnsureHandle();
            if (hwnd == IntPtr.Zero) return;

            window.IsHitTestVisible = !isClickThrough;

            IntPtr extendedStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
            long style = extendedStyle.ToInt64();

            if (isClickThrough)
            {
                style |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            }
            else
            {
                style &= ~WS_EX_TRANSPARENT;
                style |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            }

            SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(style));

            // Force DWM to update frame without recreating HWND
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        public static void EnsureTopmost(Window window)
        {
            var hwnd = new WindowInteropHelper(window).EnsureHandle();
            if (hwnd == IntPtr.Zero) return;

            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        #region Win32 P/Invoke
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : SetWindowLong32(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        #endregion
    }
}
