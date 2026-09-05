namespace SafeScan.SecurityEngine.Models
{
    public class ScanStatistics
    {
        public int TotalFiles { get; set; }
        public int FilesScanned { get; set; }
        public int SuspiciousFiles { get; set; }
        public int ThreatsDetected { get; set; }
        public long DurationMs { get; set; }
        public string ScanType { get; set; } = "Quick Scan";
    }
}
