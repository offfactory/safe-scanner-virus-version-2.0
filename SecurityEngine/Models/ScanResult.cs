using System;

namespace SafeScan.SecurityEngine.Models
{
    public class ScanResult
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Status { get; set; } = "Unknown";
        public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.Green;
        public string DetectionName { get; set; } = "Unknown File";
        public string Reason { get; set; } = "No suspicious indicators were identified.";
        public string Location { get; set; } = string.Empty;
        public string Sha256 { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
        public bool ThreatDetected { get; set; }
        public string Source { get; set; } = "Local";
    }
}
