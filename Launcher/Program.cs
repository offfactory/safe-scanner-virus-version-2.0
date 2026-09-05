using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

namespace SafeScanLauncher
{
    internal static class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "SafeScan Launcher";
            Console.WriteLine("SafeScan Launcher starting...");

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var rootDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", ".."));
            var safeScanExe = Path.Combine(rootDirectory, "bin", "Debug", "net8.0-windows", "SafeScan.exe");
            var iconSource = Path.Combine(rootDirectory, "AppLogo.ico");
            var desktopIcon = Path.Combine(desktopPath, "SafeScanLogo.ico");
            var shortcutPath = Path.Combine(desktopPath, "SafeScan Defender.lnk");

            Console.WriteLine("Downloading core files... this may take a moment.");
            for (var i = 1; i <= 100; i++)
            {
                Console.WriteLine($"Downloading part {i}/100...");
                Thread.Sleep(30);
            }

            Console.WriteLine("Download complete.");
            Console.WriteLine("Creating desktop app logo and shortcut...");

            try
            {
                if (File.Exists(iconSource))
                {
                    File.Copy(iconSource, desktopIcon, true);
                    Console.WriteLine($"App logo copied to desktop: {desktopIcon}");
                }

                if (File.Exists(safeScanExe))
                {
                    CreateShortcut(shortcutPath, safeScanExe, iconSource);
                    Console.WriteLine($"Shortcut created on desktop: {shortcutPath}");
                }
                else
                {
                    Console.WriteLine("Warning: SafeScan.exe not found. Shortcut will not be created.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to create desktop items: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("AI Assistant: I only help you, and I explain how your data is linked and how stolen data can be abused.");
            Console.WriteLine("Your data is linked to file paths, scan history, and user documents. If data is stolen, attackers can use it to access your downloads, documents, and personal information.");
            Console.WriteLine();
            Console.WriteLine("Press any key to open the main SafeScan app if it is available.");
            Console.ReadKey(true);

            if (File.Exists(safeScanExe))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(safeScanExe) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not launch SafeScan.exe: {ex.Message}");
                }
            }
        }

        private static void CreateShortcut(string shortcutFile, string targetFile, string iconFile)
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null)
            {
                return;
            }

            var shell = Activator.CreateInstance(shellType);
            if (shell == null)
            {
                return;
            }

            var shortcut = shellType.InvokeMember(
                "CreateShortcut",
                BindingFlags.InvokeMethod,
                null,
                shell,
                new object[] { shortcutFile });

            if (shortcut == null)
            {
                return;
            }

            var shortcutType = shortcut.GetType();
            if (shortcutType == null)
            {
                return;
            }

            shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetFile });
            shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetFile) ?? string.Empty });
            shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { iconFile });
            shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
        }
    }
}
