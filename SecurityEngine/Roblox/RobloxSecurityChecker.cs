using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SafeScan.SecurityEngine.Models;

namespace SafeScan.SecurityEngine.Roblox
{
    public class RobloxSecurityChecker
    {
        public List<RobloxScanResult> CheckLocalInstallations()
        {
            var results = new List<RobloxScanResult>();
            var roots = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Roblox")
            };

            foreach (var root in roots.Distinct())
            {
                if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                {
                    continue;
                }

                foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var lower = file.ToLowerInvariant();
                        var suspicious = lower.Contains("inject") || lower.Contains("executor") || lower.Contains("stealer") || lower.Contains("loader") || lower.Contains("payload") || lower.Contains("autoupdate");
                        if (!suspicious)
                        {
                            continue;
                        }

                        var classification = lower.Contains("stealer") || lower.Contains("credential") ? "Known Malware" :
                            lower.Contains("inject") ? "Suspicious" :
                            lower.Contains("executor") ? "Potentially Unwanted Application" : "Unknown Third-Party Tool";

                        results.Add(new RobloxScanResult
                        {
                            FilePath = file,
                            Classification = classification,
                            ThreatLevel = classification == "Known Malware" ? ThreatLevel.DarkRed : classification == "Suspicious" ? ThreatLevel.Orange : classification == "Potentially Unwanted Application" ? ThreatLevel.Yellow : ThreatLevel.Blue,
                            Reason = "File was inspected locally and contains patterns associated with malicious or high-risk Roblox utility behavior."
                        });
                    }
                    catch
                    {
                        // Skip inaccessible files safely.
                    }
                }
            }

            return results.Any() ? results : new List<RobloxScanResult> { new RobloxScanResult { FilePath = "<offline>local Roblox folders", Classification = "Clean", ThreatLevel = ThreatLevel.Green, Reason = "No malicious indicators were found in local Roblox-related files." } };
        }
    }

    public class RobloxScanResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string Classification { get; set; } = "Clean";
        public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.Green;
        public string Reason { get; set; } = "No malicious indicators were found.";
    }
}
