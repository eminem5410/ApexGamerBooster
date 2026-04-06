using System;
using System.Diagnostics;
using System.IO;

namespace ApexGamerBooster.Core
{
    public class SystemCleaner
    {
        public event Action<string> OnLog;
        public event Action<long> OnRamFreed;

        public void CleanRAM()
        {
            try
            {
                Log("Limpiando RAM...");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c powershell -Command \"[System.GC]::Collect();[System.GC]::WaitForPendingFinalizers()\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();

                try
                {
                    var p2 = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/c powershell -Command \"Clear-StandbyList\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    p2.Start();
                    p2.WaitForExit();
                }
                catch { }

                Log("RAM limpiada");
                OnRamFreed?.Invoke(0);
            }
            catch (Exception ex)
            {
                Log($"Error RAM: {ex.Message}");
            }
        }

        public long CleanTempFiles()
        {
            long freedBytes = 0;
            Log("Limpiando temporales...");
            try
            {
                freedBytes += CleanFolder(Path.GetTempPath());
                string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
                freedBytes += CleanFolder(winTemp);
                string prefetch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
                freedBytes += CleanFolder(prefetch);
                Log($"Temporales limpiados: {FormatSize(freedBytes)}");
            }
            catch (Exception ex)
            {
                Log($"Error temporales: {ex.Message}");
            }
            return freedBytes;
        }

        private long CleanFolder(string path)
        {
            long freed = 0;
            try
            {
                if (!Directory.Exists(path)) return 0;
                foreach (var file in Directory.GetFiles(path))
                {
                    try
                    {
                        var info = new FileInfo(file);
                        freed += info.Length;
                        info.Delete();
                    }
                    catch { }
                }
                foreach (var dir in Directory.GetDirectories(path))
                {
                    try
                    {
                        freed += CleanFolder(dir);
                        Directory.Delete(dir, true);
                    }
                    catch { }
                }
            }
            catch { }
            return freed;
        }

        public void OptimizeNetwork()
        {
            Log("Optimizando red...");
            try
            {
                RunCmd("ipconfig /flushdns");
                RunCmd("netsh int tcp set global autotuninglevel=normal");
                RunCmd("netsh int tcp set global rss=enabled");
                RunCmd("netsh int tcp set global chimney=disabled");
                RunCmd("netsh int tcp set global congestionprovider=ctcp");
                Log("Red optimizada");
            }
            catch (Exception ex)
            {
                Log($"Error red: {ex.Message}");
            }
        }

        private void RunCmd(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(10000);
        }

        private string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }

        private void Log(string msg) { OnLog?.Invoke(msg); }
    }
}