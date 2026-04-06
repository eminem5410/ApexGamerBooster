using System;
using System.Drawing;
using System.Windows.Forms;
using ApexGamerBooster.Controls;
using ApexGamerBooster.Utils;

namespace ApexGamerBooster.Forms
{
    public partial class Form1
    {
        private void ClearContent()
        {
            contentPanel.Controls.Clear();
        }

        private void SwitchSection(string section)
        {
            currentSection = section;
            foreach (var btn in navButtons) btn.Selected = false;
            int idx = section switch { "monitor" => 0, "booster" => 1, "cleaner" => 2, "games" => 3, "sysinfo" => 4, "network" => 5, "apps" => 6, _ => 0 };
            navButtons[idx].Selected = true;
            ClearContent();
            switch (section)
            {
                case "monitor": BuildMonitorSection(); break;
                case "booster": BuildBoosterSection(); break;
                case "cleaner": BuildCleanerSection(); break;
                case "games": BuildGamesSection(); break;
                case "sysinfo": BuildSystemInfoSection(); break;
                case "network": BuildNetworkSection(); break;
                case "apps": BuildAppsSection(); break;
            }
        }

        private void BuildMonitorSection()
        {
            int w = contentPanel.Width - 30;
            int cardH = 155;
            int gap = 8;
            int y = 0;

            contentPanel.Controls.Add(new Label { Text = "MONITOREO EN TIEMPO REAL", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(0, 0), AutoSize = true });
            y += 30;

            var cpuCard = UIHelper.CreateCard(0, y, w, cardH, UIHelper.AccentGreen);
            cpuCard.Controls.Add(new Label { Text = "CPU", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = UIHelper.AccentGreen, Location = new Point(12, 8), AutoSize = true });
            lblCpuName = new Label { Text = hardwareMonitor != null ? hardwareMonitor.CpuName : "--", Font = new Font("Consolas", 7f), ForeColor = UIHelper.TextSecondary, Location = new Point(50, 9), AutoSize = true };
            cpuCard.Controls.Add(lblCpuName);
            lblCpuValue = new Label { Text = "0.0%", Font = new Font("Consolas", 24f, FontStyle.Bold), ForeColor = UIHelper.AccentGreen, Location = new Point(12, 26), AutoSize = true };
            cpuCard.Controls.Add(lblCpuValue);
            cpuGraph = new GamerGraph { Label = "", GraphColor = UIHelper.AccentGreen, Location = new Point(12, 65), Size = new Size(w - 24, 80) };
            cpuCard.Controls.Add(cpuGraph);
            contentPanel.Controls.Add(cpuCard);
            y += cardH + gap;

            var ramCard = UIHelper.CreateCard(0, y, w, cardH, UIHelper.AccentBlue);
            ramCard.Controls.Add(new Label { Text = "RAM", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = UIHelper.AccentBlue, Location = new Point(12, 8), AutoSize = true });
            lblRamValue = new Label { Text = "0.0%", Font = new Font("Consolas", 24f, FontStyle.Bold), ForeColor = UIHelper.AccentBlue, Location = new Point(12, 26), AutoSize = true };
            ramCard.Controls.Add(lblRamValue);
            ramGraph = new GamerGraph { Label = "", GraphColor = UIHelper.AccentBlue, Location = new Point(12, 65), Size = new Size(w - 24, 80) };
            ramCard.Controls.Add(ramGraph);
            contentPanel.Controls.Add(ramCard);
            y += cardH + gap;

            var gpuCard = UIHelper.CreateCard(0, y, w, cardH, UIHelper.AccentPurple);
            gpuCard.Controls.Add(new Label { Text = "GPU", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = UIHelper.AccentPurple, Location = new Point(12, 8), AutoSize = true });
            lblGpuName = new Label { Text = hardwareMonitor != null ? hardwareMonitor.GpuName : "--", Font = new Font("Consolas", 7f), ForeColor = UIHelper.TextSecondary, Location = new Point(50, 9), AutoSize = true };
            gpuCard.Controls.Add(lblGpuName);
            lblGpuValue = new Label { Text = "0.0%", Font = new Font("Consolas", 24f, FontStyle.Bold), ForeColor = UIHelper.AccentPurple, Location = new Point(12, 26), AutoSize = true };
            gpuCard.Controls.Add(lblGpuValue);
            gpuGraph = new GamerGraph { Label = "", GraphColor = UIHelper.AccentPurple, Location = new Point(12, 65), Size = new Size(w - 24, 80) };
            gpuCard.Controls.Add(gpuGraph);
            contentPanel.Controls.Add(gpuCard);
        }

        private void BuildBoosterSection()
        {
            contentPanel.Controls.Add(new Label { Text = "BOOSTER DE RENDIMIENTO", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(0, 0), AutoSize = true });

            lblBoostStatus = new Label { Text = "Estado: INACTIVO", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(0, 32), AutoSize = true };
            contentPanel.Controls.Add(lblBoostStatus);

            var lblDiscordStatus = new Label { Text = "Discord: Esperando...", Font = new Font("Consolas", 8f), ForeColor = UIHelper.TextSecondary, Location = new Point(200, 35), AutoSize = true };
            contentPanel.Controls.Add(lblDiscordStatus);
            discordIPC.OnLog += (msg) =>
            {
                if (lblDiscordStatus == null) return;
                if (lblDiscordStatus.InvokeRequired) { lblDiscordStatus.Invoke(new Action(() => lblDiscordStatus.Text = msg)); return; }
                lblDiscordStatus.Text = msg;
                if (msg.Contains("CONECTADO")) lblDiscordStatus.ForeColor = UIHelper.AccentGreen;
                else if (msg.Contains("No detectado") || msg.Contains("Error")) lblDiscordStatus.ForeColor = UIHelper.AccentRed;
                else lblDiscordStatus.ForeColor = UIHelper.TextSecondary;
            };
            UpdateBoostStatus();

            btnBoost = new RoundedButton { Text = "ACTIVAR BOOST", Size = new Size(220, 48), Location = new Point(0, 65), Font = new Font("Segoe UI", 11f, FontStyle.Bold), BorderColor = UIHelper.AccentGreen };
            btnBoost.Click += BtnBoost_Click;
            contentPanel.Controls.Add(btnBoost);

            btnRestore = new RoundedButton { Text = "RESTAURAR SISTEMA", Size = new Size(220, 48), Location = new Point(240, 65), Font = new Font("Segoe UI", 11f, FontStyle.Bold), BorderColor = UIHelper.AccentYellow };
            btnRestore.Click += BtnRestore_Click;
            contentPanel.Controls.Add(btnRestore);

            btnToggleOverlay = new RoundedButton { Text = "OVERLAY ON", Size = new Size(220, 48), Location = new Point(480, 65), Font = new Font("Segoe UI", 11f, FontStyle.Bold), BorderColor = UIHelper.AccentBlue };
            btnToggleOverlay.Click += BtnToggleOverlay_Click;
            contentPanel.Controls.Add(btnToggleOverlay);

            contentPanel.Controls.Add(new Label { Text = "LOG DE OPTIMIZACION", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(0, 130), AutoSize = true });
            txtBoostLog = UIHelper.CreateLogTextBox();
            txtBoostLog.Location = new Point(0, 155);
            txtBoostLog.Size = new Size(contentPanel.Width - 30, contentPanel.Height - 175);
            contentPanel.Controls.Add(txtBoostLog);
        }

        private void BuildCleanerSection()
        {
            contentPanel.Controls.Add(new Label { Text = "LIMPIEZA Y MANTENIMIENTO", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(0, 0), AutoSize = true });
            btnCleanRam = new RoundedButton { Text = "LIMPIAR RAM", Size = new Size(200, 44), Location = new Point(0, 40), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BorderColor = UIHelper.AccentBlue };
            btnCleanRam.Click += BtnCleanRam_Click;
            contentPanel.Controls.Add(btnCleanRam);
            btnCleanTemp = new RoundedButton { Text = "LIMPIAR TEMPORALES", Size = new Size(200, 44), Location = new Point(220, 40), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BorderColor = UIHelper.AccentYellow };
            btnCleanTemp.Click += BtnCleanTemp_Click;
            contentPanel.Controls.Add(btnCleanTemp);
            btnCleanNetwork = new RoundedButton { Text = "OPTIMIZAR RED", Size = new Size(200, 44), Location = new Point(440, 40), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BorderColor = UIHelper.AccentGreen };
            btnCleanNetwork.Click += BtnCleanNetwork_Click;
            contentPanel.Controls.Add(btnCleanNetwork);
            lblCleanResult = new Label { Text = "", Font = new Font("Segoe UI", 9f), ForeColor = UIHelper.AccentGreen, Location = new Point(0, 95), AutoSize = true };
            contentPanel.Controls.Add(lblCleanResult);
            contentPanel.Controls.Add(new Label { Text = "LOG", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(0, 125), AutoSize = true });
            txtCleanLog = UIHelper.CreateLogTextBox();
            txtCleanLog.Location = new Point(0, 150);
            txtCleanLog.Size = new Size(contentPanel.Width - 30, contentPanel.Height - 170);
            contentPanel.Controls.Add(txtCleanLog);
        }

        private void BuildGamesSection()
        {
            contentPanel.Controls.Add(new Label { Text = "CENTRO DE JUEGOS", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(0, 0), AutoSize = true });
            
            btnScanGames = new RoundedButton { Text = "ESCANEAR JUEGOS", Size = new Size(160, 35), Location = new Point(0, 35), BorderColor = UIHelper.AccentGreen };
            btnScanGames.Click += BtnScanGames_Click;
            contentPanel.Controls.Add(btnScanGames);

            btnAddGame = new RoundedButton { Text = "AGREGAR MANUAL", Size = new Size(160, 35), Location = new Point(170, 35), BorderColor = UIHelper.AccentBlue };
            btnAddGame.Click += BtnAddGame_Click;
            contentPanel.Controls.Add(btnAddGame);

            int logHeight = 120;
            flowGames = new FlowLayoutPanel 
            { 
                Location = new Point(0, 80), 
                Size = new Size(contentPanel.Width - 30, contentPanel.Height - 80 - logHeight), 
                AutoScroll = true, 
                FlowDirection = FlowDirection.LeftToRight, 
                WrapContents = true, 
                BackColor = UIHelper.BgPrimary 
            };
            contentPanel.Controls.Add(flowGames);

            contentPanel.Controls.Add(new Label { Text = "LOG", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(0, contentPanel.Height - logHeight - 5), AutoSize = true });
            txtGameLog = UIHelper.CreateLogTextBox();
            txtGameLog.Location = new Point(0, contentPanel.Height - logHeight + 15);
            txtGameLog.Size = new Size(contentPanel.Width - 30, logHeight - 20);
            contentPanel.Controls.Add(txtGameLog);

            RefreshGameCovers();
        }

        private void BuildSystemInfoSection()
        {
            contentPanel.Controls.Add(new Label { Text = "INFORMACION DEL SISTEMA", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(0, 0), AutoSize = true });
            btnRefreshInfo = new RoundedButton { Text = "ACTUALIZAR", Size = new Size(130, 36), Location = new Point(0, 35), BorderColor = UIHelper.AccentGreen };
            btnRefreshInfo.Click += (s, e) => LoadSystemInfo();
            contentPanel.Controls.Add(btnRefreshInfo);
            btnDriverUpdate = new RoundedButton { Text = "ESCANEAR DRIVERS", Size = new Size(170, 36), Location = new Point(145, 35), BorderColor = UIHelper.AccentYellow };
            btnDriverUpdate.Click += BtnDriverUpdate_Click;
            contentPanel.Controls.Add(btnDriverUpdate);
            txtSystemInfo = UIHelper.CreateLogTextBox();
            txtSystemInfo.Font = new Font("Consolas", 9f);
            txtSystemInfo.Location = new Point(0, 90);
            txtSystemInfo.Size = new Size(contentPanel.Width - 30, contentPanel.Height - 100);
            contentPanel.Controls.Add(txtSystemInfo);
            LoadSystemInfo();
        }

        private void BuildNetworkSection()
        {
            contentPanel.Controls.Add(new Label { Text = "RED Y CONECTIVIDAD", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(0, 0), AutoSize = true });

            int y = 40;

            var netTypePanel = new Panel { Location = new Point(0, y), Size = new Size(500, 50), BackColor = UIHelper.BgTertiary };
            netTypePanel.Controls.Add(new Label { Text = "TIPO DE RED", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(15, 6), AutoSize = true });
            lblNetTypeValue = new Label { Text = networkTools.NetworkType, Font = new Font("Consolas", 14f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(15, 24), AutoSize = true };
            netTypePanel.Controls.Add(lblNetTypeValue);
            contentPanel.Controls.Add(netTypePanel);
            y += 60;

            var pingPanel = new Panel { Location = new Point(0, y), Size = new Size(500, 50), BackColor = UIHelper.BgTertiary };
            pingPanel.Controls.Add(new Label { Text = "PING (8.8.8.8)", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(15, 6), AutoSize = true });
            lblPingValue = new Label { Text = networkTools.PingMs >= 0 ? networkTools.PingMs + " ms" : "-- ms", Font = new Font("Consolas", 14f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(15, 24), AutoSize = true };
            pingPanel.Controls.Add(lblPingValue);
            contentPanel.Controls.Add(pingPanel);
            y += 60;

            var ipPanel = new Panel { Location = new Point(0, y), Size = new Size(500, 50), BackColor = UIHelper.BgTertiary };
            ipPanel.Controls.Add(new Label { Text = "IP PUBLICA", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(15, 6), AutoSize = true });
            lblPublicIPValue = new Label { Text = networkTools.PublicIP, Font = new Font("Consolas", 14f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(15, 24), AutoSize = true };
            ipPanel.Controls.Add(lblPublicIPValue);
            contentPanel.Controls.Add(ipPanel);
            y += 60;

            var ipv4Panel = new Panel { Location = new Point(0, y), Size = new Size(500, 50), BackColor = UIHelper.BgTertiary };
            ipv4Panel.Controls.Add(new Label { Text = "IPv4 LOCAL", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(15, 6), AutoSize = true });
            lblIpv4Value = new Label { Text = networkTools.Ipv4Local, Font = new Font("Consolas", 14f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(15, 24), AutoSize = true };
            ipv4Panel.Controls.Add(lblIpv4Value);
            contentPanel.Controls.Add(ipv4Panel);
            y += 60;

            var ipv6Panel = new Panel { Location = new Point(0, y), Size = new Size(500, 50), BackColor = UIHelper.BgTertiary };
            ipv6Panel.Controls.Add(new Label { Text = "IPv6 LOCAL", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(15, 6), AutoSize = true });
            lblIpv6Value = new Label { Text = networkTools.Ipv6Local, Font = new Font("Consolas", 14f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(15, 24), AutoSize = true };
            ipv6Panel.Controls.Add(lblIpv6Value);
            contentPanel.Controls.Add(ipv6Panel);
            y += 60;

            btnTestPing = new RoundedButton { Text = "TEST PING", Size = new Size(180, 44), Location = new Point(0, y + 10), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BorderColor = UIHelper.AccentGreen };
            btnTestPing.Click += (s, e) => networkTools.TestPing();
            contentPanel.Controls.Add(btnTestPing);

            btnRefreshIP = new RoundedButton { Text = "REFRESCAR IP", Size = new Size(180, 44), Location = new Point(200, y + 10), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BorderColor = UIHelper.AccentBlue };
            btnRefreshIP.Click += (s, e) => networkTools.GetPublicIP();
            contentPanel.Controls.Add(btnRefreshIP);
        }

        private void BuildAppsSection()
        {
            contentPanel.Controls.Add(new Label { Text = "GESTOR DE APLICACIONES", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, Location = new Point(0, 0), AutoSize = true });
            lblAppCount = new Label { Text = "Escaneando...", Font = new Font("Segoe UI", 9f), ForeColor = UIHelper.TextSecondary, Location = new Point(0, 35), AutoSize = true };
            contentPanel.Controls.Add(lblAppCount);
            btnUninstallApp = new RoundedButton { Text = "DESINSTALAR", Size = new Size(160, 38), Location = new Point(contentPanel.Width - 190, 30), BorderColor = UIHelper.AccentRed };
            btnUninstallApp.Click += BtnUninstallApp_Click;
            contentPanel.Controls.Add(btnUninstallApp);
            lstApps = new ListBox { Font = new Font("Segoe UI", 9f), BackColor = UIHelper.BgTertiary, ForeColor = UIHelper.TextPrimary, BorderStyle = BorderStyle.None, Location = new Point(0, 65), Size = new Size(contentPanel.Width - 30, 280) };
            lstApps.SelectedIndexChanged += LstApps_SelectedIndexChanged;
            contentPanel.Controls.Add(lstApps);
            contentPanel.Controls.Add(new Label { Text = "DETALLES", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(0, 355), AutoSize = true });
            txtAppInfo = UIHelper.CreateLogTextBox();
            txtAppInfo.Location = new Point(0, 380);
            txtAppInfo.Size = new Size(contentPanel.Width - 30, contentPanel.Height - 395);
            contentPanel.Controls.Add(txtAppInfo);

            appManager.ScanInstalledApps();
            lstApps.Items.Clear();
            foreach (var app in appManager.InstalledApps) lstApps.Items.Add(app);
            lblAppCount.Text = appManager.InstalledApps.Count + " aplicaciones";
        }
    }
}