using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace ShowMyMusic.Services
{
    public class HotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;
        private HwndSource? _source;
        private IntPtr _hwnd = IntPtr.Zero;
        private bool _isRegistered = false;

        public event EventHandler? HotkeyPressed;

        public void Initialize(IntPtr windowHandle)
        {
            _hwnd = windowHandle;
            _source = HwndSource.FromHwnd(_hwnd);
            _source?.AddHook(HwndHook);
        }

        public bool Register(string modifierStr, string keyStr)
        {
            if (_hwnd == IntPtr.Zero) return false;

            Unregister();

            uint modifiers = 0;
            if (modifierStr.Contains("Control", StringComparison.OrdinalIgnoreCase) || modifierStr.Contains("Ctrl", StringComparison.OrdinalIgnoreCase))
                modifiers |= 0x0002; // MOD_CONTROL
            if (modifierStr.Contains("Alt", StringComparison.OrdinalIgnoreCase))
                modifiers |= 0x0001; // MOD_ALT
            if (modifierStr.Contains("Shift", StringComparison.OrdinalIgnoreCase))
                modifiers |= 0x0004; // MOD_SHIFT
            if (modifierStr.Contains("Win", StringComparison.OrdinalIgnoreCase) || modifierStr.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                modifiers |= 0x0008; // MOD_WIN

            if (Enum.TryParse<Key>(keyStr, true, out var key))
            {
                int vk = KeyInterop.VirtualKeyFromKey(key);
                _isRegistered = RegisterHotKey(_hwnd, HOTKEY_ID, modifiers, (uint)vk);
                return _isRegistered;
            }

            return false;
        }

        public void Unregister()
        {
            if (_isRegistered && _hwnd != IntPtr.Zero)
            {
                UnregisterHotKey(_hwnd, HOTKEY_ID);
                _isRegistered = false;
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
                _source = null;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
