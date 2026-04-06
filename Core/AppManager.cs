using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;

namespace ApexGamerBooster.Core
{
    public class InstalledApp
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string UninstallCommand { get; set; }
        public long SizeBytes { get; set; }
        public bool CanUninstall { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class AppManager
    {
        public List<InstalledApp> InstalledApps { get; private set; } = new List<InstalledApp>();
        public event Action<string> OnLog;

        public void ScanInstalledApps()
        {
            InstalledApps.Clear();
            Log("Escaneando aplicaciones...");
            ScanRegistryKey(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            ScanRegistryKey(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            ScanRegistryKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            InstalledApps = InstalledApps.OrderBy(a => a.Name).ToList();
            Log($"{InstalledApps.Count} aplicaciones");
        }

        private void ScanRegistryKey(RegistryKey root, string subKey)
        {
            try
            {
                using (var key = root.OpenSubKey(subKey))
                {
                    if (key == null) return;
                    foreach (var appName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using (var appKey = key.OpenSubKey(appName))
                            {
                                if (appKey == null) continue;
                                var name = appKey.GetValue("DisplayName")?.ToString();
                                if (string.IsNullOrWhiteSpace(name)) continue;
                                var uninstallCmd = appKey.GetValue("QuietUninstallString")?.ToString()
                                    ?? appKey.GetValue("UninstallString")?.ToString();
                                var app = new InstalledApp
                                {
                                    Name = name,
                                    Version = appKey.GetValue("DisplayVersion")?.ToString() ?? "",
                                    Publisher = appKey.GetValue("Publisher")?.ToString() ?? "",
                                    UninstallCommand = uninstallCmd ?? "",
                                    CanUninstall = !string.IsNullOrEmpty(uninstallCmd)
                                };
                                var size = appKey.GetValue("EstimatedSize");
                                if (size != null) app.SizeBytes = Convert.ToInt64(size) * 1024;
                                InstalledApps.Add(app);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        public void UninstallApp(InstalledApp app)
        {
            if (string.IsNullOrEmpty(app.UninstallCommand)) return;
            try
            {
                Log($"Desinstalando {app.Name}...");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {app.UninstallCommand}",
                        UseShellExecute = true,
                        Verb = "runas"
                    }
                };
                process.Start();
                Log("Proceso de desinstalacion iniciado");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private void Log(string msg) { OnLog?.Invoke(msg); }
    }
}