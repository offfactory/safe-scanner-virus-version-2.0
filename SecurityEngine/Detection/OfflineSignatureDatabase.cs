using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SafeScan.SecurityEngine.Detection
{
    public class OfflineSignatureDatabase
    {
        private readonly string _databasePath;
        private readonly List<ThreatSignatureEntry> _entries = new List<ThreatSignatureEntry>();

        public OfflineSignatureDatabase(string? databasePath = null)
        {
            _databasePath = databasePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SecurityEngine", "Database", "signatures.json");
            LoadAsync().GetAwaiter().GetResult();
        }

        public async Task LoadAsync()
        {
            try
            {
                if (!File.Exists(_databasePath))
                {
                    return;
                }

                var json = await File.ReadAllTextAsync(_databasePath);
                var entries = JsonSerializer.Deserialize<List<ThreatSignatureEntry>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (entries != null)
                {
                    _entries.Clear();
                    _entries.AddRange(entries);
                }
            }
            catch
            {
                _entries.Clear();
            }
        }

        public ThreatSignatureEntry? FindByHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                return null;
            }

            return _entries.FirstOrDefault(x => string.Equals(x.Hash, hash, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<ThreatSignatureEntry> Entries => _entries;
    }

    public class ThreatSignatureEntry
    {
        public string Hash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Malware";
        public string Severity { get; set; } = "High";
    }
}
