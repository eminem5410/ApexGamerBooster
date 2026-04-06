using System;
using System.Diagnostics;
using System.Windows.Forms;
using ApexGamerBooster.Core;
using ApexGamerBooster.Models;
using ApexGamerBooster.Utils;

namespace ApexGamerBooster.Forms
{
    public partial class Form1
    {
        private void BtnBoost_Click(object sender, EventArgs e)
        {
            boosterService.Boost();
            discordIPC.SetActivity("Sistema Optimizado", "ApexGamerBooster Activo");
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            boosterService.Restore();
            discordIPC.SetActivity("Modo Normal", "ApexGamerBooster");
        }

        private void UpdateBoostStatus()
        {
            if (lblBoostStatus == null) return;
            if (boosterService.IsBoosted) { lblBoostStatus.Text = "Estado: ACTIVO"; lblBoostStatus.ForeColor = UIHelper.AccentGreen; }
            else { lblBoostStatus.Text = "Estado: INACTIVO"; lblBoostStatus.ForeColor = UIHelper.TextSecondary; }
        }

        private void BtnToggleOverlay_Click(object sender, EventArgs e)
        {
            if (overlayForm == null || overlayForm.IsDisposed)
            {
                overlayForm = new OverlayForm(); overlayForm.Show();
                btnToggleOverlay.Text = "OVERLAY OFF"; btnToggleOverlay.BorderColor = UIHelper.AccentRed;
            }
            else { overlayForm.Close(); overlayForm = null; btnToggleOverlay.Text = "OVERLAY ON"; btnToggleOverlay.BorderColor = UIHelper.AccentBlue; }
        }

        private void BtnCleanRam_Click(object sender, EventArgs e) { systemCleaner.CleanRAM(); lblCleanResult.Text = "RAM liberada"; lblCleanResult.ForeColor = UIHelper.AccentGreen; }
        private void BtnCleanTemp_Click(object sender, EventArgs e) { long freed = systemCleaner.CleanTempFiles(); lblCleanResult.Text = $"Temporales: {UIHelper.FormatBytes(freed)} liberados"; lblCleanResult.ForeColor = UIHelper.AccentGreen; }
        private void BtnCleanNetwork_Click(object sender, EventArgs e) { systemCleaner.OptimizeNetwork(); lblCleanResult.Text = "Red optimizada"; lblCleanResult.ForeColor = UIHelper.AccentGreen; }

        private void BtnScanGames_Click(object sender, EventArgs e) 
        { 
            AppendLog(txtGameLog, "Escaneando discos en busca de juegos...");
            gameScanner.ScanAll(); 
            RefreshGameCovers(); 
            AppendLog(txtGameLog, "Escaneo completado.");
        }

        private void BtnAddGame_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog()) 
            { 
                ofd.Filter = "Ejecutables (*.exe)|*.exe"; 
                ofd.Title = "Seleccionar juego"; 
                if (ofd.ShowDialog() == DialogResult.OK) 
                { 
                    gameScanner.AddManualGame(ofd.FileName); 
                    RefreshGameCovers(); 
                    AppendLog(txtGameLog, "Juego agregado manualmente.");
                } 
            }
        }

        private void LoadSystemInfo()
        {
            var report = systemInfo.GetFullReport();
            txtSystemInfo.Text = $"=== APEX GAMER BOOSTER - REPORTE ===\r\n\r\nSISTEMA OPERATIVO\r\n   {report.OS}\r\n\r\nPROCESADOR\r\n   {report.CPU}\r\n\r\nTARJETA GRAFICA\r\n   {report.GPU}\r\n\r\nMEMORIA RAM\r\n   {report.RAM}\r\n\r\nALMACENAMIENTO\r\n   {report.Storage}\r\n\r\nPLACA MADRE\r\n   {report.Motherboard}\r\n\r\nGenerado: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n";
        }

        private void BtnDriverUpdate_Click(object sender, EventArgs e)
        {
            txtSystemInfo.Clear();
            txtSystemInfo.AppendText("Escaneando drivers del sistema...\r\n");
            txtSystemInfo.AppendText("================================\r\n");
            txtSystemInfo.AppendText(systemInfo.GetDriverList());
            txtSystemInfo.AppendText("\r\n================================\r\n");
            string vendor = systemInfo.DetectGPUVendor();
            if (vendor != null)
            {
                txtSystemInfo.AppendText($"\r\nPara actualizar drivers de {vendor} visitá la pagina oficial.\r\n");
                txtSystemInfo.AppendText("El boton WINDOWS UPDATE tambien puede detectar drivers.\r\n");
            }
        }

        private void UpdateNetworkUI()
        {
            if (lblNetTypeValue == null || lblPingValue == null || lblPublicIPValue == null) return;
            if (InvokeRequired) { Invoke(new Action(UpdateNetworkUI)); return; }
            lblNetTypeValue.Text = networkTools.NetworkType;
            lblPingValue.Text = networkTools.PingMs >= 0 ? $"{networkTools.PingMs} ms" : "Sin respuesta";
            lblPublicIPValue.Text = networkTools.PublicIP;
            if (lblIpv4Value != null) lblIpv4Value.Text = networkTools.Ipv4Local;
            if (lblIpv6Value != null) lblIpv6Value.Text = networkTools.Ipv6Local;
        }

        private void LstApps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstApps.SelectedItem is InstalledApp app)
            {
                txtAppInfo.Text = $"Nombre: {app.Name}\r\nVersion: {app.Version}\r\nEditor: {app.Publisher}\r\nTamano: {UIHelper.FormatBytes(app.SizeBytes)}\r\nDesinstalable: {(app.CanUninstall ? "Si" : "No")}\r\nComando: {app.UninstallCommand}";
            }
        }

        private void BtnUninstallApp_Click(object sender, EventArgs e)
        {
            if (lstApps.SelectedItem is InstalledApp app)
            {
                if (MessageBox.Show($"Desinstalar {app.Name}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { appManager.UninstallApp(app); }
            }
        }

        // --- NUEVA LÓGICA DE PORTADAS DE JUEGOS ---
        private void RefreshGameCovers()
        {
            if (flowGames == null || gameScanner.Juegos == null) return;
            flowGames.Controls.Clear();

            foreach (var juego in gameScanner.Juegos)
            {
                var card = new Controls.GameCoverCard(juego.Nombre, juego.RutaExe);
                
                card.OnLaunchGame += (path) => 
                { 
                    boosterService.Boost(); 
                    discordIPC.SetActivity($"Jugando {juego.Nombre}", "ApexGamerBooster Gaming"); 
                    gameScanner.LaunchGame(juego); 
                    AppendLog(txtGameLog, "Juego lanzado con Boost: " + juego.Nombre);
                };

                card.OnOptimizeGame += (path) => 
                { 
                    boosterService.Boost(); 
                    AppendLog(txtGameLog, "Optimización aplicada para: " + juego.Nombre);
                };

                card.OnOpenFiles += (path) => 
                { 
                    try { Process.Start("explorer.exe", "/select, \"" + path + "\""); } 
                    catch (Exception ex) { AppendLog(txtGameLog, "Error al abrir: " + ex.Message); } 
                };

                card.OnUninstallGame += (path) => 
                { 
                    if (MessageBox.Show($"¿Eliminar '{juego.Nombre}' de la lista?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) 
                    { 
                        gameScanner.RemoveGame(juego); 
                        RefreshGameCovers(); 
                        AppendLog(txtGameLog, "Juego eliminado de la lista.");
                    } 
                };
                
                flowGames.Controls.Add(card);
            }
        }
    }
}