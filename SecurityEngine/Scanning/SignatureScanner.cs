using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SafeScan.SecurityEngine.Detection;
using SafeScan.SecurityEngine.Models;

namespace SafeScan.SecurityEngine.Scanning
{
    public class SignatureScanner
    {
        private readonly MalwareDatabase _malwareDatabase = new MalwareDatabase();
        private readonly HashScanner _hashScanner = new HashScanner();

        public async Task<ThreatInfo?> ScanByHashAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            try
            {
                var hash = await _hashScanner.ComputeSha256Async(filePath, cancellationToken).ConfigureAwait(false);
                var match = _malwareDatabase.MatchByHash(hash);
                return match;
            }
            catch
            {
                return null;
            }
        }
    }
}
