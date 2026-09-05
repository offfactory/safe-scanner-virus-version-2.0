namespace SafeScan.SecurityEngine.Models
{
    public class ThreatInfo
    {
        public string Name { get; set; } = "Unknown File";
        public string Type { get; set; } = "Unknown";
        public string Severity { get; set; } = "Unknown";
        public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.Blue;
        public string Reason { get; set; } = "No suspicious indicators were identified.";
        public bool IsKnownMalware { get; set; }
        public bool IsSuspicious { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
