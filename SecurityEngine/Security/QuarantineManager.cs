using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SafeScan.SecurityEngine.Security
{
    public class QuarantineManager
    {
        public string QuarantineDirectory { get; }

        public QuarantineManager(string? quarantineDirectory = null)
        {
            QuarantineDirectory = quarantineDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SafeScan", "Quarantine");
            Directory.CreateDirectory(QuarantineDirectory);
            ProtectQuarantine();
        }

        public QuarantineEntry QuarantineFile(string originalPath, string reason, bool askForConfirmation = true)
        {
            if (string.IsNullOrWhiteSpace(originalPath) || !File.Exists(originalPath))
            {
                throw new FileNotFoundException("The source file was not found.", originalPath);
            }

            var uniqueName = $"{Guid.NewGuid():N}_{Path.GetFileName(originalPath)}";
            var quarantinePath = Path.Combine(QuarantineDirectory, uniqueName);
            var metadataPath = Path.Combine(QuarantineDirectory, uniqueName + ".json");

            var metadata = new QuarantineEntry
            {
                OriginalPath = originalPath,
                QuarantinePath = quarantinePath,
                Reason = reason,
                QuarantinedAt = DateTime.UtcNow,
                FileName = Path.GetFileName(originalPath)
            };

            File.Move(originalPath, quarantinePath);
            File.WriteAllText(metadataPath, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
            ProtectQuarantine();
            return metadata;
        }

        public void RestoreFile(string quarantinePath)
        {
            if (!File.Exists(quarantinePath))
            {
                throw new FileNotFoundException("Quarantined file was not found.", quarantinePath);
            }

            var originalMetadataPath = quarantinePath + ".json";
            if (File.Exists(originalMetadataPath))
            {
                var metadata = JsonSerializer.Deserialize<QuarantineEntry>(File.ReadAllText(originalMetadataPath));
                if (metadata != null && !string.IsNullOrWhiteSpace(metadata.OriginalPath))
                {
                    var restorePath = metadata.OriginalPath;
                    if (File.Exists(restorePath))
                    {
                        File.Delete(restorePath);
                    }

                    File.Move(quarantinePath, restorePath);
                    File.Delete(originalMetadataPath);
                    return;
                }
            }

            var restoreDirectory = Path.GetDirectoryName(quarantinePath) ?? QuarantineDirectory;
            var restoredName = Path.GetFileName(quarantinePath);
            File.Move(quarantinePath, Path.Combine(restoreDirectory, restoredName));
        }

        public void DeleteQuarantinedFile(string quarantinePath)
        {
            if (File.Exists(quarantinePath))
            {
                File.Delete(quarantinePath);
            }

            var metadataPath = quarantinePath + ".json";
            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }
        }

        public List<QuarantineEntry> GetEntries()
        {
            var entries = new List<QuarantineEntry>();
            foreach (var file in Directory.GetFiles(QuarantineDirectory, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var entry = JsonSerializer.Deserialize<QuarantineEntry>(json);
                    if (entry != null) { entries.Add(entry); }
                }
                catch
                {
                    // Ignore malformed quarantine metadata entries.
                }
            }

            return entries;
        }

        public void ProtectQuarantine()
        {
            try
            {
                Directory.CreateDirectory(QuarantineDirectory);
                var attrs = File.GetAttributes(QuarantineDirectory);
                File.SetAttributes(QuarantineDirectory, attrs | FileAttributes.Hidden | FileAttributes.ReadOnly);
            }
            catch
            {
                // Do not crash if the quarantine directory cannot be protected.
            }
        }
    }

    public class QuarantineEntry
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalPath { get; set; } = string.Empty;
        public string QuarantinePath { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime QuarantinedAt { get; set; } = DateTime.UtcNow;
    }
}
