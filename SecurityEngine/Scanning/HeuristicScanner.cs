using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SafeScan.SecurityEngine.Detection;
using SafeScan.SecurityEngine.Models;

namespace SafeScan.SecurityEngine.Scanning
{
    public class HeuristicScanner
    {
        private readonly SuspiciousBehaviorRules _behaviorRules = new SuspiciousBehaviorRules();

        public ThreatInfo? Evaluate(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            var lower = filePath.ToLowerInvariant();
            var fileName = Path.GetFileName(filePath) ?? string.Empty;
            var lowerName = fileName.ToLowerInvariant();
            var reasons = new List<string>();

            if (lowerName.Contains(".exe.") || lowerName.Contains(".dll.") || lowerName.Contains(".scr.") || lowerName.Contains(".bat.") || lowerName.Contains(".cmd."))
            {
                reasons.Add("Double file extension attempts to hide an executable behind a benign document name.");
            }

            if (lower.Contains("appdata\\local\\temp") || lower.Contains("temp\\") || lower.Contains("windows\\temp") || lower.Contains("downloads") && (lowerName.Contains("setup") || lowerName.Contains("installer") || lowerName.Contains("patch")))
            {
                reasons.Add("File is in a suspicious or temporary location commonly used for payload delivery.");
            }

            if (lowerName.Contains("updater") || lowerName.Contains("payload") || lowerName.Contains("dropper") || lowerName.Contains("loader") || lowerName.Contains("injector"))
            {
                reasons.Add("Filename contains executable delivery keywords associated with suspicious activity.");
            }

            if (lower.Contains("startup") || lower.Contains("run") || lower.Contains("autorun"))
            {
                reasons.Add("File is placed in a startup or autorun path, which may be used for persistence.");
            }

            var suspiciousRule = _behaviorRules.Evaluate(filePath);
            if (suspiciousRule != null)
            {
                reasons.Add(suspiciousRule.Reason);
            }

            if (!reasons.Any())
            {
                return null;
            }

            return new ThreatInfo
            {
                Name = "Suspicious File Detected",
                Type = "Suspicious",
                Severity = "Medium",
                ThreatLevel = ThreatLevel.Orange,
                IsSuspicious = true,
                Reason = string.Join(" ", reasons.Distinct()),
                FilePath = filePath
            };
        }
    }
}
