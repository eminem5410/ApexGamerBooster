using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ApexGamerBooster.Controls;
using ApexGamerBooster.Core;
using ApexGamerBooster.Models;
using ApexGamerBooster.Utils;

namespace ApexGamerBooster.Forms
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private HardwareMonitor hardwareMonitor;
        private BoosterService boosterService;
        private SystemCleaner systemCleaner;
        private GameScanner gameScanner;
        private NetworkTools networkTools;
        private DiscordIPC discordIPC;
        private AppManager appManager;
        private SystemInfo systemInfo;
        private OverlayForm overlayForm;
        private Timer monitorTimer;
        private Timer networkTimer;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private Panel titleBar;
        private Label lblTitle;
        private Panel navPanel;
        private List<RoundedButton> navButtons = new List<RoundedButton>();
        private Panel contentPanel;
        private GamerGraph cpuGraph;
        private GamerGraph ramGraph;
        private GamerGraph gpuGraph;
        private Label lblCpuValue;
        private Label lblRamValue;
        private Label lblGpuValue;
        private Label lblGpuName;
        private Label lblCpuName;
        private RoundedButton btnBoost;
        private RoundedButton btnRestore;
        private Label lblBoostStatus;
        private TextBox txtBoostLog;
        private RoundedButton btnToggleOverlay;
        private RoundedButton btnCleanRam;
        private RoundedButton btnCleanTemp;
        private RoundedButton btnCleanNetwork;
        private TextBox txtCleanLog;
        private Label lblCleanResult;
        
        // REEMPLACÉ EL ComboBox POR ESTO:
        private FlowLayoutPanel flowGames;
        
        private RoundedButton btnScanGames;
        private RoundedButton btnAddGame;
        private RoundedButton btnRemoveGame;
        private RoundedButton btnLaunchGame;
        private RoundedButton btnSetFavorite;
        private Label lblGameInfo;
        private TextBox txtGameLog;
        private TextBox txtSystemInfo;
        private RoundedButton btnRefreshInfo;
        private RoundedButton btnDriverUpdate;
        private Label lblNetTypeValue;
        private Label lblPingValue;
        private Label lblPublicIPValue;
        private Label lblIpv4Value;
        private Label lblIpv6Value;
        private RoundedButton btnTestPing;
        private RoundedButton btnRefreshIP;
        private ListBox lstApps;
        private RoundedButton btnUninstallApp;
        private Label lblAppCount;
        private TextBox txtAppInfo;
        private Label lblDiscordStatus;
        private string currentSection = "monitor";
        private bool isMaximized = false;
        private string discordLastMessage = "Conectando...";
        private Point lastLocation;
        private Size lastSize;
        
        // Variable para el fix de Discord
        private int discordUpdateCounter = 0;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        public Form1()
        {
            InitializeComponent();
            InitializeServices();
            InitializeTray();
            InitializeTimers();
            LoadInitialData();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            FormBorderStyle = FormBorderStyle.None;
            BackColor = UIHelper.BgPrimary;
            ForeColor = UIHelper.TextPrimary;
            Size = new Size(980, 640);
            MinimumSize = new Size(900, 580);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 9f);

            titleBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = UIHelper.BgSecondary, Padding = new Padding(12, 0, 0, 0) };

            lblTitle = new Label
            {
                Text = "APEX BOOSTER",
                Font = new Font("Consolas", 12f, FontStyle.Bold),
                ForeColor = UIHelper.AccentGreen,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 200,
                Padding = new Padding(0, 0, 0, 4)
            };
            lblTitle.Paint += (s, e) =>
            {
                e.Graphics.DrawString("GAMING OPTIMIZER", new Font("Consolas", 6f), new SolidBrush(Color.FromArgb(0, 255, 136, 80)), 0, lblTitle.Height - 12);
            };
            titleBar.Controls.Add(lblTitle);

            var closeBtn = new Label { Text = "X", Size = new Size(40, 42), BackColor = Color.Transparent, ForeColor = UIHelper.AccentRed, Font = new Font("Segoe UI", 10f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand, Location = new Point(80, 0) };
            closeBtn.Click += (s, e) => MinimizeToTray();
            var minBtn = new Label { Text = "—", Size = new Size(40, 42), BackColor = Color.Transparent, ForeColor = UIHelper.TextSecondary, Font = new Font("Segoe UI", 10f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand, Location = new Point(0, 0) };
            minBtn.Click += (s, e) => { WindowState = FormWindowState.Minimized; };
            var maxBtn = new Label { Text = "□", Size = new Size(40, 42), BackColor = Color.Transparent, ForeColor = UIHelper.TextSecondary, Font = new Font("Segoe UI", 10f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand, Location = new Point(40, 0) };
            maxBtn.Click += (s, e) => ToggleMaximize();

            var btnPanel = new Panel { Dock = DockStyle.Right, Width = 120 };
            btnPanel.Controls.AddRange(new System.Windows.Forms.Control[] { minBtn, maxBtn, closeBtn });
            titleBar.Controls.Add(btnPanel);
            titleBar.MouseDown += TitleBar_MouseDown;

            navPanel = new Panel { Dock = DockStyle.Left, Width = 160, BackColor = UIHelper.BgSecondary, Padding = new Padding(8, 8, 8, 8) };

            string[] navTexts = { "  Monitoreo", "  Booster", "  Limpieza", "  Juegos", "  Sistema", "  Redes", "  Aplicaciones" };
            string[] navSections = { "monitor", "booster", "cleaner", "games", "sysinfo", "network", "apps" };
            int btnY = 15;
            for (int i = 0; i < navTexts.Length; i++)
            {
                var btn = new RoundedButton();
                btn.Text = navTexts[i];
                btn.Width = 144;
                btn.Height = 38;
                btn.Location = new Point(8, btnY);
                btn.BorderColor = UIHelper.AccentGreen;
                btn.TextAlignment = ContentAlignment.MiddleLeft;
                btn.Font = new Font("Segoe UI", 9f);
                string section = navSections[i];
                btn.Click += (s, e) => SwitchSection(section);
                navButtons.Add(btn);
                navPanel.Controls.Add(btn);
                btnY += 44;
            }

            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.BgPrimary, Padding = new Padding(15, 15, 15, 15) };
            Controls.Add(contentPanel);
            Controls.Add(navPanel);
            Controls.Add(titleBar);
            ResumeLayout(true);
            UIHelper.FadeIn(this, 400);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0); }
        }

        private void ToggleMaximize()
        {
            if (isMaximized) { Location = lastLocation; Size = lastSize; isMaximized = false; }
            else { lastLocation = Location; lastSize = Size; Location = new Point(0, 0); Size = Screen.PrimaryScreen.WorkingArea.Size; isMaximized = true; }
        }

        private void InitializeServices()
        {
            hardwareMonitor = new HardwareMonitor();
            boosterService = new BoosterService();
            systemCleaner = new SystemCleaner();
            gameScanner = new GameScanner();
            networkTools = new NetworkTools();
            discordIPC = new DiscordIPC();
            appManager = new AppManager();
            systemInfo = new SystemInfo();
            boosterService.OnLog += (msg) => AppendLog(txtBoostLog, msg);
            systemCleaner.OnLog += (msg) => AppendLog(txtCleanLog, msg);
            gameScanner.OnLog += (msg) => AppendLog(txtGameLog, msg);
            appManager.OnLog += (msg) => AppendLog(txtGameLog, msg);
            systemInfo.OnLog += (msg) => AppendLog(txtGameLog, msg);
            boosterService.OnBoostChanged += (boosted) => UpdateBoostStatus();
            networkTools.OnDataChanged += UpdateNetworkUI;
            discordIPC.OnLog += (msg) =>
            {
                discordLastMessage = msg;
                if (lblDiscordStatus == null) return;
                if (lblDiscordStatus.InvokeRequired) { lblDiscordStatus.Invoke(new Action(() => UpdateDiscordUI(msg))); return; }
                UpdateDiscordUI(msg);
            };
        }

        private void UpdateDiscordUI(string msg)
        {
            lblDiscordStatus.Text = msg;
            if (msg.Contains("CONECTADO")) lblDiscordStatus.ForeColor = UIHelper.AccentGreen;
            else if (msg.Contains("No detectado") || msg.Contains("Error")) lblDiscordStatus.ForeColor = UIHelper.AccentRed;
            else lblDiscordStatus.ForeColor = UIHelper.TextSecondary;
        }

        private void InitializeTray()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Abrir", null, (s, e) => RestoreFromTray());
            trayMenu.Items.Add("Salir", null, (s, e) => ExitApplication());
            trayIcon = new NotifyIcon { Icon = SystemIcons.Shield, Text = "ApexBooster", Visible = true, ContextMenuStrip = trayMenu };
            trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        private void InitializeTimers()
        {
            monitorTimer = new Timer { Interval = 1500 };
            monitorTimer.Tick += MonitorTimer_Tick;
            monitorTimer.Start();
            networkTimer = new Timer { Interval = 30000 };
            networkTimer.Tick += (s, e) => { networkTools.DetectNetworkType(); };
            networkTimer.Start();
        }

        private void LoadInitialData()
        {
            SwitchSection("monitor");
            networkTools.DetectNetworkType();
            networkTools.GetPublicIP();
            networkTools.TestPing();
            try { discordIPC.ConnectAsync(); }
            catch { }
        }

        private void MonitorTimer_Tick(object sender, EventArgs e)
        {
            hardwareMonitor.Update();
            float cpu = hardwareMonitor.CpuUsage;
            float ram = hardwareMonitor.RamUsage;
            float gpu = hardwareMonitor.GpuUsage;
            if (currentSection == "monitor")
            {
                if (cpuGraph != null) cpuGraph.PushValue(cpu);
                if (ramGraph != null) ramGraph.PushValue(ram);
                if (gpuGraph != null) gpuGraph.PushValue(gpu);
                if (lblCpuValue != null) { lblCpuValue.Text = cpu.ToString("F1") + "%"; lblCpuValue.ForeColor = UIHelper.GetStatusColor(cpu); }
                if (lblRamValue != null) { lblRamValue.Text = ram.ToString("F1") + "%"; lblRamValue.ForeColor = UIHelper.GetStatusColor(ram); }
                if (lblGpuValue != null) { lblGpuValue.Text = gpu.ToString("F1") + "%"; lblGpuValue.ForeColor = UIHelper.GetStatusColor(gpu); }
            }
            if (overlayForm != null && !overlayForm.IsDisposed) overlayForm.UpdateValues(cpu, ram, gpu);

            discordUpdateCounter++;
            if (discordUpdateCounter >= 10)
            {
                discordUpdateCounter = 0;
                if (discordIPC != null)
                {
                    string estadoDiscord = $"CPU: {cpu:F0}% | RAM: {ram:F0}%";
                    string detalleDiscord = "Optimizando sistema";
                    discordIPC.SetActivity(estadoDiscord, detalleDiscord);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(UIHelper.BorderColor, 1f)) { g.DrawLine(pen, navPanel.Right, titleBar.Bottom, navPanel.Right, Height); }
            using (var glow = new LinearGradientBrush(new Rectangle(0, 0, 1, 6), Color.FromArgb(0, 255, 136, 25), Color.FromArgb(0, 255, 136, 0), LinearGradientMode.Vertical)) { g.FillRectangle(glow, 0, 0, Width, 6); }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; MinimizeToTray(); }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { monitorTimer?.Stop(); networkTimer?.Stop(); discordIPC?.Dispose(); trayIcon?.Dispose(); }
            base.Dispose(disposing);
        }

        private void MinimizeToTray() { Hide(); }
        private void RestoreFromTray() { Show(); WindowState = FormWindowState.Normal; UIHelper.FadeIn(this, 300); }
        private void ExitApplication() { boosterService.Restore(); discordIPC.Disconnect(); if (overlayForm != null && !overlayForm.IsDisposed) overlayForm.Close(); trayIcon.Visible = false; trayIcon.Dispose(); Application.Exit(); }

        private void AppendLog(TextBox textBox, string message)
        {
            if (textBox == null) return;
            if (textBox.InvokeRequired) { textBox.Invoke(new Action(() => AppendLog(textBox, message))); return; }
            string time = DateTime.Now.ToString("HH:mm:ss");
            textBox.AppendText("[" + time + "] " + message + "\r\n");
            textBox.SelectionStart = textBox.Text.Length;
            textBox.ScrollToCaret();
        }

        private Label CreateBigValueLabel(string text) { return new Label { Text = text, Font = new Font("Consolas", 22f, FontStyle.Bold), ForeColor = UIHelper.AccentGreen, AutoSize = true }; }

        private Panel CreateInfoCard(string title, string value, ref int y)
        {
            var panel = new Panel { Location = new Point(0, y), Size = new Size(500, 50), BackColor = UIHelper.BgTertiary };
            panel.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(15, 6), AutoSize = true });
            panel.Controls.Add(new Label { Text = value, Font = new Font("Consolas", 14f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(15, 24), AutoSize = true });
            y += 60;
            return panel;
        }
    }
}