using System;
using System.Diagnostics;
using System.Management;

namespace ApexGamerBooster.Core
{
    public class HardwareMonitor
    {
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private PerformanceCounter gpuCounter;
        private string gpuName = "No detectada";
        private bool gpuCounterAvailable = false;

        public float CpuUsage { get; private set; }
        public float RamUsage { get; private set; }
        public float RamTotalGB { get; private set; }
        public float GpuUsage { get; private set; }
        public string GpuName => gpuName;
        public string CpuName { get; private set; } = "No detectado";

        public HardwareMonitor()
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                RamTotalGB = GetTotalRAM();
                CpuName = GetCPUName();
                DetectGPU();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error init HardwareMonitor: {ex.Message}");
            }
        }

        private float GetTotalRAM()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return Convert.ToSingle(Convert.ToUInt64(obj["TotalPhysicalMemory"]) / (1024.0 * 1024.0 * 1024.0));
                    }
                }
            }
            catch { }
            return 8f;
        }

        private string GetCPUName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Name"]?.ToString()?.Trim() ?? "No detectado";
                    }
                }
            }
            catch { }
            return "No detectado";
        }

        private void DetectGPU()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        gpuName = obj["Name"]?.ToString()?.Trim() ?? "No detectada";
                        break;
                    }
                }
                try
                {
                    var cat = new PerformanceCounterCategory("GPU Engine");
                    var instances = cat.GetInstanceNames();
                    if (instances.Length > 0)
                    {
                        gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instances[0]);
                        gpuCounterAvailable = true;
                    }
                }
                catch
                {
                    gpuCounterAvailable = false;
                }
            }
            catch { }
        }

        public void Update()
        {
            try
            {
                CpuUsage = cpuCounter.NextValue();
                float availableMB = ramCounter.NextValue();
                float usedMB = (RamTotalGB * 1024) - availableMB;
                RamUsage = Math.Max(0, (usedMB / (RamTotalGB * 1024)) * 100f);

                if (gpuCounterAvailable)
                {
                    GpuUsage = gpuCounter.NextValue();
                }
            }
            catch { }
        }
    }
}