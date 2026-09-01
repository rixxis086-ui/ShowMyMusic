using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace ShowMyMusic.Services
{
    public class AutostartService
    {
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ShowMyMusic";

        public static bool IsAutostartEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }

        public static void SetAutostart(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                if (key == null) return;

                if (enable)
                {
                    string exePath = Process.GetCurrentProcess().MainModule?.FileName 
                        ?? Path.Combine(AppContext.BaseDirectory, "ShowMyMusic.exe");
                    key.SetValue(AppName, $"\"{exePath}\" --minimized");
                }
                else
                {
                    if (key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set autostart: {ex.Message}");
            }
        }
    }
}
