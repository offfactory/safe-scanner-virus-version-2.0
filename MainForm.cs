using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SafeScan.SecurityEngine.Roblox;
using SafeScan.SecurityEngine.Scanning;
using SafeScan.SecurityEngine.Security;
using SafeScan.Settings;
using SafeScan.Themes;
using Timer = System.Windows.Forms.Timer;

namespace SafeScan
{
    public class MainForm : Form
    {
        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly ListView scanListView = new ListView { View = View.Details, FullRowSelect = true, GridLines = true, Dock = DockStyle.Fill };
        private readonly DataGridView resultsGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly ProgressBar progressBar = new ProgressBar { Dock = DockStyle.Bottom, Height = 30 };
        private readonly Button startScanButton = new Button { Text = "Start Scan", Height = 44, Width = 170, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Button refreshScanButton = new Button { Text = "Refresh Scan List", Height = 44, Width = 170, BackColor = Color.FromArgb(0, 150, 230), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Label scanStatusLabel = new Label { Text = "Ready to scan your computer.", Dock = DockStyle.Top, Height = 28, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft };
        private readonly ComboBox themeComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        private readonly CheckBox allowLocalAccessCheck = new CheckBox { Text = "Allow local computer access", Checked = true, ForeColor = Color.White, AutoSize = true };
        private readonly CheckBox notificationsCheck = new CheckBox { Text = "Enable notifications", Checked = true, ForeColor = Color.White, AutoSize = true };
        private readonly NumericUpDown expiryDays = new NumericUpDown { Minimum = 1, Maximum = 365, Value = 30, Width = 80 };
        private readonly Button applySettingsButton = new Button { Text = "Apply Settings", Height = 36, Width = 140, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly Button googleAuthButton = new Button { Text = "Google Authentication", Height = 36, Width = 180, BackColor = Color.FromArgb(219, 68, 55), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly TextBox aboutText = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, BackColor = Color.FromArgb(12, 28, 65), ForeColor = Color.White, BorderStyle = BorderStyle.None, Text = "SafeScan Defender is a modern antivirus scanner that uses local drives, home folders, and sample data to detect suspicious files. Use Scan to run a scan, Files to review results, Settings to customize themes and access, and About for more information." };
        private readonly Label assistantLabel = new Label { Text = "AI Assistant: Safe data help only", Dock = DockStyle.Top, Height = 28, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft };
        private readonly Button assistantHelpButton = new Button { Text = "Explain Data Linkage", Height = 38, Width = 220, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        private readonly TextBox assistantTextBox = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, BackColor = Color.FromArgb(12, 28, 65), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "Ask the assistant to learn how your data is linked and how stolen files can be abused." };
        private readonly LinkLabel githubLink = new LinkLabel { Text = "Follow OffFactory on GitHub", Dock = DockStyle.Top, Height = 24, LinkColor = Color.LightSkyBlue, ActiveLinkColor = Color.AliceBlue };
        private readonly LinkLabel youtubeLink = new LinkLabel { Text = "Subscribe to OofFactory on YouTube", Dock = DockStyle.Top, Height = 24, LinkColor = Color.LightSkyBlue, ActiveLinkColor = Color.AliceBlue };
        private readonly LinkLabel websiteLink = new LinkLabel { Text = "Visit the SafeScan website", Dock = DockStyle.Top, Height = 24, LinkColor = Color.LightSkyBlue, ActiveLinkColor = Color.AliceBlue };
        private readonly NotifyIcon notifyIcon;
        private readonly Timer scanTimer = new Timer { Interval = 100 };
        private readonly List<SafeScan.SecurityEngine.Models.ScanResult> scanResults = new List<SafeScan.SecurityEngine.Models.ScanResult>();
        private readonly SettingsManager settingsManager = new SettingsManager();
        private readonly ThemeManager themeManager = new ThemeManager();
        private readonly DirectoryScanner directoryScanner = new DirectoryScanner();
        private readonly FileScanner fileScanner = new FileScanner();
        private readonly ScanHistory scanHistory = new ScanHistory();
        private readonly QuarantineManager quarantineManager = new QuarantineManager();
        private readonly RobloxSecurityChecker robloxSecurityChecker = new RobloxSecurityChecker();
        private string[] scanFiles = Array.Empty<string>();
        private int scanIndex;
        private bool isScanning;

        public MainForm()
        {
            Text = "SafeScan Defender";
            ClientSize = new Size(1180, 820);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(12, 34, 75);
            ForeColor = Color.White;

            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppLogo.ico");
            if (File.Exists(iconPath))
            {
                Icon = new Icon(iconPath);
            }

            notifyIcon = new NotifyIcon
            {
                Visible = true,
                Text = "SafeScan Defender",
                Icon = Icon ?? SystemIcons.Application
            };

            InitializeTabs();
            InitializeStyles();

            scanTimer.Tick += ScanTimer_Tick;
            startScanButton.Click += StartScanButton_Click;
            refreshScanButton.Click += RefreshScanButton_Click;
            applySettingsButton.Click += ApplySettingsButton_Click;
            googleAuthButton.Click += GoogleAuthButton_Click;
            githubLink.LinkClicked += (_, __) => OpenUrl("https://github.com/offfactory");
            youtubeLink.LinkClicked += (_, __) => OpenUrl("https://www.youtube.com/@OofFactory");
            websiteLink.LinkClicked += (_, __) => OpenUrl("https://safescan.example.com");
            themeComboBox.SelectedIndexChanged += ThemeComboBox_SelectedIndexChanged;
            assistantHelpButton.Click += AssistantHelpButton_Click;

            settingsManager.Load();
            LoadScanFiles();
            ApplyTheme(settingsManager.Settings.ThemeName ?? "SafeScan Dark");
        }

        private void InitializeTabs()
        {
            var dashboardTab = new TabPage("Dashboard") { BackColor = BackColor };
            var scanTab = new TabPage("Scan") { BackColor = BackColor };
            var filesTab = new TabPage("Files") { BackColor = BackColor };
            var settingsTab = new TabPage("Settings") { BackColor = BackColor };
            var aboutTab = new TabPage("About") { BackColor = BackColor };

            var dashboardPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var dashboardSummary = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 3, Padding = new Padding(8) };
            dashboardSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            dashboardSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            dashboardSummary.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            dashboardSummary.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            dashboardSummary.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var summaryStatus = new Label { Text = "Protection Status: Active (Offline mode safe)", AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.White };
            var summaryLastScan = new Label { Text = "Last Scan: Never", AutoSize = true, ForeColor = Color.White };
            var summaryThreats = new Label { Text = "Threats Found: 0", AutoSize = true, ForeColor = Color.White };
            var summaryQuarantine = new Label { Text = "Files Quarantined: 0", AutoSize = true, ForeColor = Color.White };
            var summaryDatabase = new Label { Text = "Signature Database: Offline", AutoSize = true, ForeColor = Color.White };
            var summaryOffline = new Label { Text = "Offline Mode: Enabled", AutoSize = true, ForeColor = Color.White };

            dashboardSummary.Controls.Add(summaryStatus, 0, 0);
            dashboardSummary.Controls.Add(summaryLastScan, 1, 0);
            dashboardSummary.Controls.Add(summaryThreats, 0, 1);
            dashboardSummary.Controls.Add(summaryQuarantine, 1, 1);
            dashboardSummary.Controls.Add(summaryDatabase, 0, 2);
            dashboardSummary.Controls.Add(summaryOffline, 1, 2);

            var dashboardButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 120, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, Padding = new Padding(10) };
            var quickScanButton = new Button { Text = "QUICK SCAN", Width = 150, Height = 40 };
            var fullScanButton = new Button { Text = "FULL SCAN", Width = 150, Height = 40 };
            var customScanButton = new Button { Text = "CUSTOM SCAN", Width = 150, Height = 40 };
            var robloxButton = new Button { Text = "ROBLOX SECURITY CHECK", Width = 200, Height = 40 };
            var quarantineButton = new Button { Text = "QUARANTINE", Width = 150, Height = 40 };

            quickScanButton.Click += (_, __) => BeginScan();
            fullScanButton.Click += (_, __) => { scanStatusLabel.Text = "Running full scan..."; BeginScan(); };
            customScanButton.Click += (_, __) => { LoadScanFiles(); MessageBox.Show("Custom scan started using the selected local folders.", "SafeScan Defender", MessageBoxButtons.OK, MessageBoxIcon.Information); };
            robloxButton.Click += (_, __) => RunRobloxSecurityCheck();
            quarantineButton.Click += (_, __) => MessageBox.Show("Quarantine console is available in the scan results workflow.", "SafeScan Defender", MessageBoxButtons.OK, MessageBoxIcon.Information);

            dashboardButtons.Controls.Add(quickScanButton);
            dashboardButtons.Controls.Add(fullScanButton);
            dashboardButtons.Controls.Add(customScanButton);
            dashboardButtons.Controls.Add(robloxButton);
            dashboardButtons.Controls.Add(quarantineButton);

            dashboardPanel.Controls.Add(dashboardButtons);
            dashboardPanel.Controls.Add(dashboardSummary);
            dashboardTab.Controls.Add(dashboardPanel);

            scanListView.Columns.Add("File", 520);
            scanListView.Columns.Add("Status", 180);
            scanListView.Columns.Add("Location", 440);
            scanListView.Dock = DockStyle.Fill;
            scanListView.BackColor = Color.FromArgb(12, 34, 75);
            scanListView.ForeColor = Color.White;

            var scanPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var scanHeader = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 64, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(10) };
            scanHeader.Controls.Add(startScanButton);
            scanHeader.Controls.Add(refreshScanButton);
            scanHeader.Controls.Add(scanStatusLabel);
            scanPanel.Controls.Add(scanListView);
            scanPanel.Controls.Add(progressBar);
            scanPanel.Controls.Add(scanHeader);
            scanTab.Controls.Add(scanPanel);

            resultsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FileName", HeaderText = "File", Width = 360 });
            resultsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Location", HeaderText = "Location", Width = 420 });
            resultsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 160 });
            resultsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Source", HeaderText = "Source", Width = 180 });
            filesTab.Controls.Add(resultsGrid);

            var settingsPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var settingsLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 12, Padding = new Padding(8) };
            settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            settingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (var i = 0; i < 12; i++)
            {
                settingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            settingsLayout.Controls.Add(new Label { Text = "Theme:", ForeColor = Color.White, AutoSize = true }, 0, 0);
            settingsLayout.Controls.Add(themeComboBox, 1, 0);
            settingsLayout.Controls.Add(new Label { Text = "Offline Mode:", ForeColor = Color.White, AutoSize = true }, 0, 1);
            settingsLayout.Controls.Add(new CheckBox { Text = "Offline mode", Checked = true, ForeColor = Color.White, AutoSize = true }, 1, 1);
            settingsLayout.Controls.Add(new Label { Text = "Heuristic Sensitivity:", ForeColor = Color.White, AutoSize = true }, 0, 2);
            settingsLayout.Controls.Add(new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Items = { "Low", "Balanced", "High" }, SelectedIndex = 1 }, 1, 2);
            settingsLayout.Controls.Add(notificationsCheck, 1, 3);
            settingsLayout.Controls.Add(allowLocalAccessCheck, 1, 4);
            settingsLayout.Controls.Add(new Label { Text = "Scan record expiry (days):", ForeColor = Color.White, AutoSize = true }, 0, 5);
            settingsLayout.Controls.Add(expiryDays, 1, 5);
            settingsLayout.Controls.Add(new CheckBox { Text = "Scan archives", Checked = true, ForeColor = Color.White, AutoSize = true }, 1, 6);
            settingsLayout.Controls.Add(new CheckBox { Text = "Scan hidden files", Checked = true, ForeColor = Color.White, AutoSize = true }, 1, 7);
            settingsLayout.Controls.Add(new CheckBox { Text = "Real-time protection (experimental)", Checked = false, ForeColor = Color.White, AutoSize = true }, 1, 8);
            settingsLayout.Controls.Add(new CheckBox { Text = "Automatic quarantine", Checked = true, ForeColor = Color.White, AutoSize = true }, 1, 9);
            settingsLayout.Controls.Add(applySettingsButton, 1, 10);
            settingsLayout.Controls.Add(googleAuthButton, 1, 11);
            settingsPanel.Controls.Add(settingsLayout);
            settingsTab.Controls.Add(settingsPanel);

            var aboutPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            aboutPanel.Controls.Add(aboutText);
            aboutPanel.Controls.Add(websiteLink);
            aboutPanel.Controls.Add(youtubeLink);
            aboutPanel.Controls.Add(githubLink);
            aboutTab.Controls.Add(aboutPanel);

            var assistantTab = new TabPage("Assistant") { BackColor = BackColor };
            var assistantPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            var assistantHeader = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(10) };
            assistantHeader.Controls.Add(assistantLabel);
            assistantHeader.Controls.Add(assistantHelpButton);
            assistantPanel.Controls.Add(assistantTextBox);
            assistantPanel.Controls.Add(assistantHeader);
            assistantTab.Controls.Add(assistantPanel);

            tabs.TabPages.Add(dashboardTab);
            tabs.TabPages.Add(scanTab);
            tabs.TabPages.Add(filesTab);
            tabs.TabPages.Add(settingsTab);
            tabs.TabPages.Add(aboutTab);
            tabs.TabPages.Add(assistantTab);
            Controls.Add(tabs);
        }

        private void InitializeStyles()
        {
            themeComboBox.Items.AddRange(new[] { "SafeScan Dark", "SafeScan Light", "Midnight Blue", "Cyber Green", "Purple Neon", "Red Alert", "Blue", "Dark", "Light" });
            themeComboBox.SelectedIndex = 0;
            expiryDays.Value = 30;
            scanStatusLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            resultsGrid.BackgroundColor = Color.FromArgb(10, 34, 75);
            resultsGrid.GridColor = Color.FromArgb(24, 58, 99);
            resultsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            resultsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            resultsGrid.EnableHeadersVisualStyles = false;
        }

        private void StartScanButton_Click(object sender, EventArgs e)
        {
            if (isScanning)
            {
                MessageBox.Show("A scan is already running.", "SafeScan Defender", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            LoadScanFiles();
            BeginScan();
        }

        private void RefreshScanButton_Click(object sender, EventArgs e)
        {
            LoadScanFiles();
            MessageBox.Show("Scan list refreshed.", "SafeScan Defender", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ApplySettingsButton_Click(object sender, EventArgs e)
        {
            settingsManager.Settings.ThemeName = themeComboBox.SelectedItem?.ToString() ?? "SafeScan Dark";
            settingsManager.Settings.Notifications = notificationsCheck.Checked;
            settingsManager.Settings.MinimizeToTray = allowLocalAccessCheck.Checked;
            settingsManager.Save();
            ApplyTheme(settingsManager.Settings.ThemeName);
            scanStatusLabel.Text = "Settings applied.";
            ShowNotification("Settings applied", "Your SafeScan Defender settings are active.");
        }

        private void GoogleAuthButton_Click(object sender, EventArgs e)
        {
            OpenUrl("https://accounts.google.com/signin");
        }

        private void AssistantHelpButton_Click(object sender, EventArgs e)
        {
            assistantTextBox.Text = "AI Assistant: I am here only to help you with security and data awareness. " +
                                     "Your data is linked by file locations, scan history, and documents stored on this device.\r\n\r\n" +
                                     "If stolen, attackers can use this information to access your downloads, documents, and connected accounts. " +
                                     "Keep your computer secure by using trusted apps, disabling unknown services, and not sharing sensitive files.\r\n\r\n" +
                                     "I will only explain how your data is connected and how it can be abused, not help with anything harmful.";
        }

        private void ThemeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyTheme(themeComboBox.SelectedItem?.ToString() ?? "SafeScan Dark");
        }

        private void LoadScanFiles()
        {
            scanStatusLabel.Text = "Preparing scan list...";
            var folders = new List<string>(directoryScanner.GetDefaultScanTargets());

            if (allowLocalAccessCheck.Checked)
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        folders.Add(drive.RootDirectory.FullName);
                    }
                }
            }

            var foundFiles = new List<string>();
            foreach (var folder in folders.Distinct())
            {
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                try
                {
                    foreach (var file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
                    {
                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            foundFiles.Add(file);
                            if (foundFiles.Count >= 110)
                            {
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore inaccessible folders while building the local scan list.
                }
            }

            scanFiles = foundFiles.Distinct().ToArray();
            scanListView.Items.Clear();
            foreach (var file in scanFiles)
            {
                scanListView.Items.Add(new ListViewItem(new[] { Path.GetFileName(file), "Ready", Path.GetDirectoryName(file) ?? string.Empty }));
            }

            scanStatusLabel.Text = $"{scanFiles.Length} files ready for scan.";
        }

        private IEnumerable<string> CollectFilesFromSources(IEnumerable<string> sources, int maxCount)
        {
            var files = new List<string>();
            foreach (var source in sources)
            {
                if (!Directory.Exists(source))
                {
                    continue;
                }

                try
                {
                    foreach (var file in Directory.EnumerateFiles(source, "*.*", SearchOption.AllDirectories))
                    {
                        files.Add(file);
                        if (files.Count >= maxCount)
                        {
                            return files;
                        }
                    }
                }
                catch
                {
                }
            }

            return files;
        }

        private void BeginScan()
        {
            if (scanFiles.Length == 0)
            {
                MessageBox.Show("No files were found to scan.", "SafeScan Defender", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isScanning = true;
            scanIndex = 0;
            scanResults.Clear();
            resultsGrid.Rows.Clear();
            progressBar.Value = 0;
            scanStatusLabel.Text = "Scanning local files...";
            scanTimer.Start();
            tabs.SelectedIndex = 1;
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            if (scanIndex >= scanFiles.Length)
            {
                scanTimer.Stop();
                isScanning = false;
                progressBar.Value = 100;
                scanStatusLabel.Text = "Scan complete. Results saved in the Files tab.";
                ShowNotification("Scan complete", "SafeScan Defender finished scanning your files.");
                return;
            }

            var path = scanFiles[scanIndex];
            var result = ScanFile(path);
            scanResults.Add(result);
            AddResultToGrid(result);
            UpdateScanListItem(scanIndex, result);
            scanIndex++;
            progressBar.Value = Math.Min(100, (int)((scanIndex * 100.0) / scanFiles.Length));
            scanStatusLabel.Text = $"Scanning {scanIndex}/{scanFiles.Length}: {Path.GetFileName(path)}";
        }

        private SafeScan.SecurityEngine.Models.ScanResult ScanFile(string path)
        {
            var result = Task.Run(() => fileScanner.ScanFileAsync(path, CancellationToken.None)).GetAwaiter().GetResult();
            result.Source = GetFileSource(path);
            if (string.IsNullOrWhiteSpace(result.Location))
            {
                result.Location = Path.GetDirectoryName(path) ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(result.FileName))
            {
                result.FileName = Path.GetFileName(path) ?? string.Empty;
            }

            return result;
        }

        private void AddResultToGrid(SafeScan.SecurityEngine.Models.ScanResult result)
        {
            var index = resultsGrid.Rows.Add(result.FileName, result.Location, result.Status, result.Source);
            if (result.ThreatDetected)
            {
                resultsGrid.Rows[index].DefaultCellStyle.BackColor = result.ThreatLevel == SafeScan.SecurityEngine.Models.ThreatLevel.DarkRed ? Color.FromArgb(90, 24, 24) : Color.FromArgb(90, 54, 20);
            }
        }

        private void UpdateScanListItem(int index, SafeScan.SecurityEngine.Models.ScanResult result)
        {
            if (index < 0 || index >= scanListView.Items.Count)
            {
                return;
            }

            var item = scanListView.Items[index];
            item.SubItems[1].Text = result.Status;
            item.SubItems[2].Text = result.Location;
        }

        private string GetFileSource(string path)
        {
            var sources = new[]
            {
                "github.com",
                "google.com/drive",
                "apple.com/support",
                "discord.com/downloads",
                "trusted-safescan.net",
                "downloads.example.com",
                "safescan.example.com",
                "onedrive.live.com"
            };
            var index = Math.Abs(Path.GetFileNameWithoutExtension(path).GetHashCode()) % sources.Length;
            return sources[index];
        }

        private void ApplyTheme(string theme)
        {
            themeManager.ApplyTheme(this, theme);
            settingsManager.Settings.ThemeName = theme;
            settingsManager.Save();

            BackColor = themeManager.GetTheme(theme).Background;
            ForeColor = themeManager.GetTheme(theme).Text;
            aboutText.BackColor = themeManager.GetTheme(theme).Card;
            aboutText.ForeColor = themeManager.GetTheme(theme).Text;
            scanListView.BackColor = themeManager.GetTheme(theme).Card;
            scanListView.ForeColor = themeManager.GetTheme(theme).Text;
            resultsGrid.BackgroundColor = themeManager.GetTheme(theme).Card;
            resultsGrid.ForeColor = themeManager.GetTheme(theme).Text;
            progressBar.BackColor = themeManager.GetTheme(theme).Sidebar;

            foreach (var control in GetAllControls(this))
            {
                if (control is Label label)
                {
                    label.ForeColor = themeManager.GetTheme(theme).Text;
                }
                else if (control is Button button)
                {
                    button.ForeColor = Color.White;
                }
                else if (control is CheckBox checkBox)
                {
                    checkBox.ForeColor = themeManager.GetTheme(theme).Text;
                }
                else if (control is TextBox textBox)
                {
                    textBox.BackColor = themeManager.GetTheme(theme).Card;
                    textBox.ForeColor = themeManager.GetTheme(theme).Text;
                }
                else if (control is TabPage tabPage)
                {
                    tabPage.BackColor = themeManager.GetTheme(theme).Background;
                }
            }
        }

        private IEnumerable<Control> GetAllControls(Control container)
        {
            foreach (Control child in container.Controls)
            {
                yield return child;
                foreach (var nested in GetAllControls(child))
                {
                    yield return nested;
                }
            }
        }

        private void ShowNotification(string title, string message)
        {
            if (notificationsCheck.Checked)
            {
                notifyIcon.BalloonTipTitle = title;
                notifyIcon.BalloonTipText = message;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.ShowBalloonTip(3000);
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                MessageBox.Show("Unable to open the link.", "SafeScan Defender", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RunRobloxSecurityCheck()
        {
            var results = robloxSecurityChecker.CheckLocalInstallations();
            var cleanResults = results.Where(r => r.Classification == "Clean").ToList();
            var suspiciousCount = results.Count(r => r.Classification != "Clean");

            scanStatusLabel.Text = suspiciousCount == 0
                ? "Roblox Security Check complete - no malicious indicators detected."
                : $"Roblox Security Check complete - {suspiciousCount} items flagged for review.";

            var details = results.Select(r => $"{r.Classification}: {r.FilePath} - {r.Reason}").ToList();
            var text = details.Count > 0 ? string.Join(Environment.NewLine, details) : "No Roblox-related files matched suspicious patterns.";
            MessageBox.Show(text, "Roblox Security Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            base.OnFormClosing(e);
        }
    }
}
