using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ApexGamerBooster.Core
{
    public class NetworkTools
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public string NetworkType { get; private set; } = "Desconocida";
        public string PublicIP { get; private set; } = "Obteniendo...";
        public long PingMs { get; private set; } = -1;

        public event Action OnDataChanged;

        public string Ipv4Local { get; private set; } = "-";
        public string Ipv6Local { get; private set; } = "-";

        public void DetectNetworkType()
        {
            try
            {
                NetworkType = "No detectada";
                Ipv4Local = "-";
                Ipv6Local = "-";
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up
                        && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                            NetworkType = "Wi-Fi";
                        else if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                            NetworkType = "Cable (Ethernet)";
                        else
                            NetworkType = nic.NetworkInterfaceType.ToString();

                        foreach (var ip in nic.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                Ipv4Local = ip.Address.ToString();
                            else if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && !ip.Address.IsIPv6LinkLocal)
                                Ipv6Local = ip.Address.ToString();
                        }
                        break;
                    }
                }
            }
            catch { NetworkType = "No detectada"; }
            OnDataChanged?.Invoke();
        }

        public async void TestPing()
        {
            try
            {
                var reply = await new Ping().SendPingAsync("8.8.8.8", 2000);
                PingMs = reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;
            }
            catch { PingMs = -1; }
            OnDataChanged?.Invoke();
        }

        public async void GetPublicIP()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ApexGamerBooster/1.0");
                PublicIP = await httpClient.GetStringAsync("https://api.ipify.org");
            }
            catch { PublicIP = "No disponible"; }
            OnDataChanged?.Invoke();
        }
    }
}