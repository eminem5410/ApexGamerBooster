using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;

namespace ApexGamerBooster.Core
{
    public class BoosterService
    {
        private List<ServiceController> stoppedServices = new List<ServiceController>();
        private string originalPowerPlan = "";
        public bool IsBoosted { get; private set; } = false;

        private static readonly string[] SafeServices = {
            "SysMain", "WSearch", "DiagTrack", "WerSvc", "BITS",
            "wuauserv", "UsoSvc", "TabletInputService", "WMPNetworkSvc",
            "RetailDemo", "MessagingService", "PimIndexMaintenance",
            "MapsBroker", "lfsvc", "WdiSystemHost", "wisvc"
        };

        public event Action<bool> OnBoostChanged;
        public event Action<string> OnLog;

        public void Boost()
        {
            if (IsBoosted) return;
            Log("Iniciando optimizacion...");
            StopServices();
            SetHighPerformance();
            ClearStandbyList();
            IsBoosted = true;
            OnBoostChanged?.Invoke(true);
            Log("Optimizacion completada");
        }

        public void Restore()
        {
            if (!IsBoosted) return;
            Log("Restaurando configuracion...");
            RestoreServices();
            RestorePowerPlan();
            IsBoosted = false;
            OnBoostChanged?.Invoke(false);
            Log("Sistema restaurado");
        }

        private void StopServices()
        {
            int count = 0;
            foreach (var serviceName in SafeServices)
            {
                try
                {
                    var service = new ServiceController(serviceName);
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                        stoppedServices.Add(service);
                        count++;
                    }
                }
                catch { }
            }
            Log($"{count} servicios detenidos");
        }

        private void RestoreServices()
        {
            int count = 0;
            foreach (var service in stoppedServices)
            {
                try
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        count++;
                    }
                }
                catch { }
            }
            stoppedServices.Clear();
            Log($"{count} servicios restaurados");
        }

        private void SetHighPerformance()
        {
            try
            {
                originalPowerPlan = RunCmd("powercfg /getactivescheme").Trim();
                RunCmd("powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
                Log("Plan: Alto Rendimiento");
            }
            catch
            {
                Log("No se pudo cambiar plan de energia");
            }
        }

        private void RestorePowerPlan()
        {
            if (!string.IsNullOrEmpty(originalPowerPlan))
            {
                try
                {
                    int startIdx = originalPowerPlan.IndexOf('{');
                    if (startIdx >= 0)
                    {
                        int endIdx = originalPowerPlan.IndexOf('}', startIdx);
                        if (endIdx > startIdx)
                        {
                            string guid = originalPowerPlan.Substring(startIdx, endIdx - startIdx + 1);
                            RunCmd($"powercfg /setactive {guid}");
                            Log("Plan de energia restaurado");
                        }
                    }
                }
                catch { }
            }
        }

        private void ClearStandbyList()
        {
            try
            {
                RunCmd("powershell -Command \"Clear-StandbyList\"");
                Log("Lista standby limpiada");
            }
            catch { }
        }

        private string RunCmd(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(10000);
            return output;
        }

        private void Log(string msg) { OnLog?.Invoke(msg); }
    }
}