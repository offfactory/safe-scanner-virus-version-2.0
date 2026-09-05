using System;
using System.IO;
using System.Text.Json;

namespace SafeScan.Settings
{
    public class SettingsManager
    {
        private readonly string _settingsPath;

        public AppSettings Settings { get; private set; } = new AppSettings();

        public SettingsManager(string? settingsPath = null)
        {
            _settingsPath = settingsPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SafeScan", "settings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (loaded != null)
                    {
                        Settings = loaded;
                    }
                }
            }
            catch
            {
                Settings = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // Best-effort persistence only.
            }
        }
    }
}
