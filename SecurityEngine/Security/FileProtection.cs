using System;
using System.IO;

namespace SafeScan.SecurityEngine.Security
{
    public class FileProtection
    {
        public static void ProtectFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return;
            }

            try
            {
                var attributes = File.GetAttributes(folderPath);
                File.SetAttributes(folderPath, attributes | FileAttributes.Hidden | FileAttributes.ReadOnly);
            }
            catch
            {
                // Ignore. File protection is best-effort only.
            }
        }
    }
}
