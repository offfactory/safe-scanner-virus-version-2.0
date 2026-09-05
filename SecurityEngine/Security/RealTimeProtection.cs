using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SafeScan.SecurityEngine.Models;
using SafeScan.SecurityEngine.Scanning;

namespace SafeScan.SecurityEngine.Security
{
    public sealed class RealTimeProtection : IDisposable
    {
        private readonly FileScanner _fileScanner;
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private readonly HashSet<string> _pendingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new object();

        public bool IsEnabled { get; private set; }
        public event EventHandler<ScanResult>? SuspiciousFileDetected;

        public RealTimeProtection(FileScanner fileScanner)
        {
            _fileScanner = fileScanner;
        }

        public void Start(IEnumerable<string> folders)
        {
            Stop();

            foreach (var folder in folders.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var watcher = new FileSystemWatcher(folder)
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                    Filter = "*.*",
                    EnableRaisingEvents = true
                };
                watcher.Created += OnFileCreated;
                watcher.Renamed += OnFileRenamed;
                _watchers.Add(watcher);
            }

            IsEnabled = _watchers.Count > 0;
        }

        public void Stop()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Created -= OnFileCreated;
                watcher.Renamed -= OnFileRenamed;
                watcher.Dispose();
            }

            _watchers.Clear();
            lock (_sync)
            {
                _pendingPaths.Clear();
            }

            IsEnabled = false;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _ = ScanChangedFileAsync(e.FullPath);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            _ = ScanChangedFileAsync(e.FullPath);
        }

        private async Task ScanChangedFileAsync(string path)
        {
            if (!IsCandidate(path))
            {
                return;
            }

            lock (_sync)
            {
                if (!_pendingPaths.Add(path))
                {
                    return;
                }
            }

            try
            {
                await Task.Delay(500).ConfigureAwait(false);
                var result = await _fileScanner.ScanFileAsync(path).ConfigureAwait(false);
                if (result.ThreatDetected)
                {
                    SuspiciousFileDetected?.Invoke(this, result);
                }
            }
            finally
            {
                lock (_sync)
                {
                    _pendingPaths.Remove(path);
                }
            }
        }

        private static bool IsCandidate(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension is ".exe" or ".dll" or ".scr" or ".msi" or ".bat" or ".cmd" or ".ps1" or ".vbs" or ".js";
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
