using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;
using ShowMyMusic.Models;

namespace ShowMyMusic.Services
{
    public class SettingsService
    {
        private static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShowMyMusic");
        private static readonly string SettingsFilePath = Path.Combine(AppDataFolder, "settings.json");
        private static readonly string CustomPresetsFilePath = Path.Combine(AppDataFolder, "custom_presets.json");

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly DispatcherTimer _saveDebounceTimer;

        public AppSettings CurrentSettings { get; private set; } = new();
        public List<PresetTheme> AllPresets { get; private set; } = new();

        public event EventHandler<AppSettings>? SettingsChanged;

        public SettingsService()
        {
            _saveDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
            _saveDebounceTimer.Tick += (s, e) =>
            {
                _saveDebounceTimer.Stop();
                FlushSettingsToDisk();
            };

            CurrentSettings.PropertyChanged += (s, e) =>
            {
                _saveDebounceTimer.Stop();
                _saveDebounceTimer.Start();
            };
        }

        public void LoadSettings()
        {
            try
            {
                if (!Directory.Exists(AppDataFolder))
                    Directory.CreateDirectory(AppDataFolder);

                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                    if (loaded != null)
                    {
                        CurrentSettings.CopyFrom(loaded);
                    }
                }
                else
                {
                    FlushSettingsToDisk();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }

            LoadPresets();
        }

        public void SaveSettings()
        {
            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }

        public void SaveSettingsImmediate()
        {
            _saveDebounceTimer.Stop();
            FlushSettingsToDisk();
        }

        private void FlushSettingsToDisk()
        {
            try
            {
                if (!Directory.Exists(AppDataFolder))
                    Directory.CreateDirectory(AppDataFolder);

                string json = JsonSerializer.Serialize(CurrentSettings, _jsonOptions);
                File.WriteAllText(SettingsFilePath, json);
                SettingsChanged?.Invoke(this, CurrentSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to flush settings: {ex.Message}");
            }
        }

        public void LoadPresets()
        {
            AllPresets = PresetTheme.GetBuiltInPresets();

            try
            {
                if (File.Exists(CustomPresetsFilePath))
                {
                    string json = File.ReadAllText(CustomPresetsFilePath);
                    var custom = JsonSerializer.Deserialize<List<PresetTheme>>(json, _jsonOptions);
                    if (custom != null)
                        AllPresets.AddRange(custom);
                }
            }
            catch { }
        }

        public void SaveCustomPreset(PresetTheme preset)
        {
            try
            {
                var customPresets = new List<PresetTheme>();
                if (File.Exists(CustomPresetsFilePath))
                {
                    string json = File.ReadAllText(CustomPresetsFilePath);
                    customPresets = JsonSerializer.Deserialize<List<PresetTheme>>(json, _jsonOptions) ?? new();
                }
                customPresets.RemoveAll(p => p.Name.Equals(preset.Name, StringComparison.OrdinalIgnoreCase));
                customPresets.Add(preset);
                File.WriteAllText(CustomPresetsFilePath, JsonSerializer.Serialize(customPresets, _jsonOptions));
                LoadPresets();
            }
            catch { }
        }

        public void ExportSettings(string targetFilePath)
        {
            string json = JsonSerializer.Serialize(CurrentSettings, _jsonOptions);
            File.WriteAllText(targetFilePath, json);
        }

        public void ImportSettings(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath)) return;
            try
            {
                string json = File.ReadAllText(sourceFilePath);
                var imported = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                if (imported != null)
                {
                    CurrentSettings.CopyFrom(imported);
                    SaveSettingsImmediate();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to import settings: {ex.Message}");
            }
        }
    }
}