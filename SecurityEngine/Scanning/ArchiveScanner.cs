using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using SafeScan.SecurityEngine.Models;

namespace SafeScan.SecurityEngine.Scanning
{
    public class ArchiveScanner
    {
        public async Task<ThreatInfo?> ScanArchiveAsync(string archivePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(archivePath) || !File.Exists(archivePath))
            {
                return null;
            }

            var extension = Path.GetExtension(archivePath).ToLowerInvariant();
            if (extension != ".zip" && extension != ".rar" && extension != ".7z")
            {
                return null;
            }

            try
            {
                using var archive = ZipFile.OpenRead(archivePath);
                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var name = entry.FullName.ToLowerInvariant();
                    if (name.Contains(".exe") || name.Contains(".vbs") || name.Contains(".js") || name.Contains(".ps1") || name.Contains(".bat"))
                    {
                        return new ThreatInfo
                        {
                            Name = "Suspicious Archive",
                            Type = "Archive",
                            Severity = "Medium",
                            ThreatLevel = ThreatLevel.Orange,
                            IsSuspicious = true,
                            Reason = "Archive contains executable content that may be used for malicious payload delivery.",
                            FilePath = archivePath
                        };
                    }
                }
            }
            catch
            {
                return null;
            }

            await Task.CompletedTask;
            return null;
        }
    }
}
