using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SafeScan.SecurityEngine.Models;

namespace SafeScan.SecurityEngine.Detection
{
    public class SuspiciousBehaviorRules
    {
        private readonly string[] _suspiciousNames =
        {
            "update", "payload", "loader", "dropper", "installer", "crack", "keygen", "exploit", "injector", "stealer"
        };

        private readonly string[] _scriptIndicators =
        {
            "powershell", "wscript", "cscript", "cmd /c", "rundll32", "regsvr32", "mshta", "http://", "https://", "base64", "invoke-webrequest"
        };

        public ThreatInfo? Evaluate(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            var path = filePath.ToLowerInvariant();
            var fileName = Path.GetFileName(filePath) ?? string.Empty;
            var lowerFileName = fileName.ToLowerInvariant();
            var suspicious = false;
            var reasons = new List<string>();

            if (lowerFileName.Contains(".jpg.") || lowerFileName.Contains(".png.") || lowerFileName.Contains(".pdf.") || lowerFileName.Contains(".txt.") || lowerFileName.Contains(".doc.") || lowerFileName.Contains(".exe."))
            {
                suspicious = true;
                reasons.Add("Double file extension used to disguise an executable as a document.");
            }

            if (path.Contains("appdata\\local\\temp") || path.Contains("appdata/local/temp") || path.Contains("windows\\temp") || path.Contains("temp\\") || path.Contains("\\temp\\"))
            {
                suspicious = true;
                reasons.Add("Executable is located in a temporary folder commonly used for dropped payloads.");
            }

            if (path.Contains("startup") || path.Contains("run") || path.Contains("autorun") || path.Contains("startup\\") || path.Contains("startup/"))
            {
                suspicious = true;
                reasons.Add("File is located in an autorun or startup path, which is commonly abused for persistence.");
            }

            var hasKeyword = _suspiciousNames.Any(keyword => lowerFileName.Contains(keyword));
            if (hasKeyword)
            {
                suspicious = true;
                reasons.Add("File name contains suspicious loader or payload keywords.");
            }

            var scriptIndicator = _scriptIndicators.Any(indicator => lowerFileName.Contains(indicator) || path.Contains(indicator));
            if (scriptIndicator)
            {
                suspicious = true;
                reasons.Add("Script or command pattern suggests execution behavior or remote script activity.");
            }

            if (!suspicious)
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
