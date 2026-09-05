using System;

namespace SafeScan.Settings
{
    public class AppSettings
    {
        public bool StartWithWindows { get; set; }
        public bool MinimizeToTray { get; set; } = true;
        public bool Notifications { get; set; } = true;
        public bool ConfirmBeforeQuarantine { get; set; } = true;
        public bool ConfirmBeforeDeletion { get; set; } = true;
        public bool QuickScanDesktop { get; set; } = true;
        public bool QuickScanDownloads { get; set; } = true;
        public bool QuickScanDocuments { get; set; } = true;
        public bool ScanArchives { get; set; } = true;
        public bool ScanHiddenFiles { get; set; } = true;
        public bool ScanLargeFiles { get; set; } = true;
        public long MaximumFileSizeBytes { get; set; } = 100L * 1024 * 1024;
        public bool ScanSubfolders { get; set; } = true;
        public bool RealTimeProtectionEnabled { get; set; }
        public bool AutomaticQuarantineEnabled { get; set; } = true;
        public string HeuristicSensitivity { get; set; } = "Balanced";
        public bool OfflineMode { get; set; } = true;
        public bool NoTelemetry { get; set; } = true;
        public bool ScanHistoryEnabled { get; set; } = true;
        public string ThemeName { get; set; } = "SafeScan Dark";
        public string AccentColor { get; set; } = "#2E9BFF";
        public bool CompactMode { get; set; }
        public bool AnimationEnabled { get; set; } = true;
        public string[] QuickScanLocations { get; set; } = Array.Empty<string>();
    }
}
