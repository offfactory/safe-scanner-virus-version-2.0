using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SafeScan.SecurityEngine.Security
{
    public class ScanHistory
    {
        private readonly string _historyPath;

        public ScanHistory(string? historyPath = null)
        {
            _historyPath = historyPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "scan_history.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_historyPath) ?? AppDomain.CurrentDomain.BaseDirectory);
        }

        public List<ScanHistoryEntry> Load()
        {
            try
            {
                if (!File.Exists(_historyPath))
                {
                    return new List<ScanHistoryEntry>();
                }

                var json = File.ReadAllText(_historyPath);
                var entries = JsonSerializer.Deserialize<List<ScanHistoryEntry>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return entries ?? new List<ScanHistoryEntry>();
            }
            catch
            {
                return new List<ScanHistoryEntry>();
            }
        }

        public void Save(List<ScanHistoryEntry> entries)
        {
            try
            {
                var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_historyPath, json);
            }
            catch
            {
                // Ignore malformed history persistence errors.
            }
        }

        public void AddEntry(ScanHistoryEntry entry)
        {
            var entries = Load();
            entries.Add(entry);
            Save(entries);
        }
    }

    public class ScanHistoryEntry
    {
        public DateTime ScanDate { get; set; } = DateTime.UtcNow;
        public string ScanType { get; set; } = "Quick Scan";
        public int FilesScanned { get; set; }
        public int ThreatsFound { get; set; }
        public int DurationSeconds { get; set; }
    }
}
