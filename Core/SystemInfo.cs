using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;

namespace ApexGamerBooster.Core
{
    public class SystemInfoReport
    {
        public string OS { get; set; }
        public string CPU { get; set; }
        public string GPU { get; set; }
        public string RAM { get; set; }
        public string Storage { get; set; }
        public string Motherboard { get; set; }
    }

    public class SystemInfo
    {
        public event Action<string> OnLog;

        public SystemInfoReport GetFullReport()
        {
            var report = new SystemInfoReport();
            report.OS = GetOS();
            report.CPU = GetCPU();
            report.GPU = GetGPU();
            report.RAM = GetRAM();
            report.Storage = GetStorage();
            report.Motherboard = GetMotherboard();
            return report;
        }

        private string GetOS()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                    foreach (var o in s.Get())
                        return o["Caption"] + " " + o["OSArchitecture"];
            }
            catch { }
            return "No detectado";
        }

        private string GetCPU()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                    foreach (var o in s.Get())
                    {
                        int c = Convert.ToInt32(o["NumberOfCores"]);
                        int t = Convert.ToInt32(o["NumberOfLogicalProcessors"]);
                        return o["Name"] + " (" + c + "C/" + t + "T)";
                    }
            }
            catch { }
            return "No detectado";
        }

        private string GetGPU()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    var gpus = new System.Collections.Generic.List<string>();
                    foreach (var o in s.Get())
                    {
                        var vram = Convert.ToUInt64(o["AdapterRAM"]) / (1024 * 1024);
                        gpus.Add(o["Name"] + " (" + vram + " MB)");
                    }
                    return gpus.Count > 0 ? string.Join("\n", gpus) : "No detectada";
                }
            }
            catch { }
            return "No detectada";
        }

        private string GetRAM()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    long total = 0;
                    int sticks = 0;
                    string speed = "";
                    foreach (var o in s.Get())
                    {
                        total += (long)Convert.ToUInt64(o["Capacity"]);
                        sticks++;
                        speed = o["Speed"] != null ? o["Speed"].ToString() : "";
                    }
                    return (total / (1024 * 1024 * 1024)) + " GB (" + sticks + " stick(s) @ " + speed + " MHz)";
                }
            }
            catch { }
            return "No detectado";
        }

        private string GetStorage()
        {
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed);
                return string.Join("\n", drives.Select(d =>
                {
                    long total = d.TotalSize / (1024 * 1024 * 1024);
                    long free = d.TotalFreeSpace / (1024 * 1024 * 1024);
                    long used = total - free;
                    double pct = ((double)used / total) * 100;
                    string st = pct > 90 ? "CRITICO" : pct > 75 ? "LLENO" : "OK";
                    return d.Name + " " + used + "/" + total + " GB (" + pct.ToString("F0") + "%) [" + st + "]";
                }));
            }
            catch { }
            return "No detectado";
        }

        private string GetMotherboard()
        {
            try
            {
                string mfr = "";
                string prod = "";
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                    foreach (var o in s.Get())
                    {
                        mfr = o["Manufacturer"] != null ? o["Manufacturer"].ToString() : "";
                        prod = o["Product"] != null ? o["Product"].ToString() : "";
                    }
                return mfr + " " + prod;
            }
            catch { }
            return "No detectado";
        }

        public string DetectGPUVendor()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                    foreach (var o in s.Get())
                    {
                        string name = o["Name"] != null ? o["Name"].ToString().ToUpper() : "";
                        if (name.Contains("NVIDIA")) return "NVIDIA";
                        if (name.Contains("AMD") || name.Contains("RADEON")) return "AMD";
                        if (name.Contains("INTEL")) return "Intel";
                    }
            }
            catch { }
            return null;
        }

        public void OpenWindowsUpdate()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "ms-settings:windowsupdate";
                psi.UseShellExecute = true;
                System.Diagnostics.Process.Start(psi);
            }
            catch { }
        }

        public string GetDriverList()
        {
            var sb = new StringBuilder();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT DeviceName, DriverVersion, DriverDate, DeviceClass FROM Win32_PnPSignedDriver"))
                {
                    string lastClass = "";
                    foreach (var obj in searcher.Get())
                    {
                        string name = obj["DeviceName"] != null ? obj["DeviceName"].ToString().Trim() : "";
                        string version = obj["DriverVersion"] != null ? obj["DriverVersion"].ToString().Trim() : "";
                        string date = obj["DriverDate"] != null ? obj["DriverDate"].ToString().Trim() : "";
                        string devClass = obj["DeviceClass"] != null ? obj["DeviceClass"].ToString().Trim() : "";
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        if (devClass != "DISPLAY" && devClass != "NET" && devClass != "MEDIA" && devClass != "SCSIADAPTER" && devClass != "HDC" && devClass != "USB" && devClass != "IMAGE" && devClass != "MOUSE" && devClass != "KEYBOARD") continue;

                        string className = devClass;
                        if (devClass == "DISPLAY") className = "TARJETA GRAFICA";
                        else if (devClass == "NET") className = "RED";
                        else if (devClass == "MEDIA") className = "AUDIO";
                        else if (devClass == "SCSIADAPTER") className = "ALMACENAMIENTO";
                        else if (devClass == "HDC") className = "CONTROLADOR HDD";
                        else if (devClass == "USB") className = "USB";
                        else if (devClass == "IMAGE") className = "CAMARA";
                        else if (devClass == "MOUSE") className = "MOUSE";
                        else if (devClass == "KEYBOARD") className = "TECLADO";

                        if (className != lastClass)
                        {
                            sb.AppendLine("\r\n=== " + className + " ===");
                            lastClass = className;
                        }

                        string dateFormatted = "--";
                        if (!string.IsNullOrEmpty(date) && date.Length >= 8)
                            dateFormatted = date.Substring(6, 2) + "/" + date.Substring(4, 2) + "/" + date.Substring(0, 4);

                        sb.AppendLine("  " + name);
                        sb.AppendLine("  Version: " + version + " | Fecha: " + dateFormatted);
                        sb.AppendLine();
                    }
                }
            }
            catch (Exception ex) { sb.AppendLine("Error: " + ex.Message); }
            if (sb.Length == 0) sb.AppendLine("No se encontraron drivers.");
            return sb.ToString();
        }

        public void OpenDriverPage(string vendor)
        {
            try
            {
                string url = "https://www.google.com/search?q=download+gpu+drivers";
                if (vendor != null)
                {
                    if (vendor.ToLower() == "nvidia") url = "https://www.nvidia.com/Download/index.aspx";
                    else if (vendor.ToLower() == "amd") url = "https://www.amd.com/en/support";
                    else if (vendor.ToLower() == "intel") url = "https://www.intel.com/content/www/us/en/download-center/home.html";
                }
                System.Diagnostics.Process.Start(url);
            }
            catch { }
        }

        private void Log(string msg) { OnLog?.Invoke(msg); }
    }
}