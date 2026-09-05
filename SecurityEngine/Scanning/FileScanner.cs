using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SafeScan.SecurityEngine.Detection;
using SafeScan.SecurityEngine.Models;

namespace SafeScan.SecurityEngine.Scanning
{
    public class FileScanner
    {
        private readonly HashScanner _hashScanner = new HashScanner();
        private readonly HeuristicScanner _heuristicScanner = new HeuristicScanner();
        private readonly SignatureScanner _signatureScanner = new SignatureScanner();

        public async Task<ScanResult> ScanFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var result = new ScanResult
            {
                FileName = Path.GetFileName(filePath) ?? string.Empty,
                FilePath = filePath,
                Location = Path.GetDirectoryName(filePath) ?? string.Empty,
                Status = "Clean",
                ThreatLevel = ThreatLevel.Green,
                DetectionName = "Unknown File",
                Reason = "No suspicious indicators were identified."
            };

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Status = "Skipped";
                result.DetectionName = "Unknown File";
                result.Reason = "File could not be accessed or does not exist.";
                return result;
            }

            try
            {
                result.Sha256 = await _hashScanner.ComputeSha256Async(filePath, cancellationToken).ConfigureAwait(false);
                var knownThreat = await _signatureScanner.ScanByHashAsync(filePath, cancellationToken).ConfigureAwait(false);
                var heuristic = _heuristicScanner.Evaluate(filePath);

                if (knownThreat != null)
                {
                    result.Status = "Known Malware Detected";
                    result.ThreatLevel = ThreatLevel.DarkRed;
                    result.DetectionName = knownThreat.Name;
                    result.Reason = knownThreat.Reason;
                    result.ThreatDetected = true;
                    return result;
                }

                if (heuristic != null)
                {
                    result.Status = "Suspicious File Detected";
                    result.ThreatLevel = heuristic.ThreatLevel;
                    result.DetectionName = heuristic.Name;
                    result.Reason = heuristic.Reason;
                    result.ThreatDetected = true;
                    return result;
                }

                result.Status = "Clean";
                result.DetectionName = "Clean";
                result.Reason = "No suspicious indicators were identified.";
            }
            catch (OperationCanceledException)
            {
                result.Status = "Cancelled";
                result.Reason = "Scan was cancelled by the user.";
            }
            catch (UnauthorizedAccessException)
            {
                result.Status = "Access Denied";
                result.DetectionName = "Unknown File";
                result.Reason = "The file could not be accessed safely.";
            }
            catch (IOException)
            {
                result.Status = "Skipped";
                result.DetectionName = "Unknown File";
                result.Reason = "The file could not be opened or was locked by another process.";
            }
            catch
            {
                result.Status = "Skipped";
                result.DetectionName = "Unknown File";
                result.Reason = "The file could not be scanned safely.";
            }

            return result;
        }
    }
}
