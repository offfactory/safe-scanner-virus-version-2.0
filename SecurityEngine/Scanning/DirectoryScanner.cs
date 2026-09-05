using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SafeScan.SecurityEngine.Scanning
{
    public class DirectoryScanner
    {
        public async Task<List<string>> ScanDirectoryAsync(string rootPath, bool recursive = true, CancellationToken cancellationToken = default)
        {
            var results = new List<string>();
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                return results;
            }

            try
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", searchOption))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    results.Add(file);
                }

                await Task.CompletedTask;
            }
            catch
            {
                // Ignore inaccessible files and do not crash the scan.
            }

            return results;
        }

        public List<string> GetDefaultScanTargets()
        {
            var targets = new List<string>();
            var specialFolders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };

            foreach (var folder in specialFolders)
            {
                if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
                {
                    targets.Add(folder);
                }
            }

            return targets;
        }
    }
}
