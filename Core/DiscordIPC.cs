using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApexGamerBooster.Core
{
    public class DiscordIPC : IDisposable
    {
        private NamedPipeClientStream pipe;
        private bool isConnected = false;
        private const int MaxPipeAttempts = 16;
        
        // Candado para evitar que los mensajes se pisen entre sí y rompan el pipe
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        public event Action<string> OnLog;

        public async Task ConnectAsync()
        {
            // Si ya está conectado, no hace nada
            if (isConnected && pipe != null && pipe.IsConnected) return;

            // Limpiamos la conexión anterior si existiera
            Disconnect();

            for (int i = 0; i < MaxPipeAttempts; i++)
            {
                try
                {
                    // Creamos una tubería temporal para probar
                    var tempPipe = new NamedPipeClientStream(".", "discord-ipc-" + i, PipeDirection.InOut);
                    await tempPipe.ConnectAsync(500); // 500ms de espera
                    
                    if (tempPipe.IsConnected)
                    {
                        pipe = tempPipe; // Solo la asignamos si fue exitosa
                        isConnected = true;
                        Log("Discord: CONECTADO (pipe " + i + ")");
                        await SendHandshake();
                        return;
                    }
                    else
                    {
                        tempPipe.Dispose(); // Si no conectó, la destruimos
                    }
                }
                catch 
                {
                    // Si falla el pipe 'i', simplemente prueba con el siguiente
                }
            }
            Log("Discord: No detectado (no esta ejecutandose?)");
        }

        private async Task SendHandshake()
        {
            try
            {
                var handshake = CreatePacket(0, "{\"v\":1,\"client_id\":\"130000000000000000\"}");
                await pipe.WriteAsync(handshake, 0, handshake.Length);
                await pipe.FlushAsync();
            }
            catch (Exception ex)
            {
                Log("Discord: Error en handshake - " + ex.Message);
                Disconnect(); // Si falla el handshake, cortamos la conexión
            }
        }

        public async void SetActivity(string state, string details)
        {
            // 1. Si se cayó la conexión, intentamos reconectar automáticamente
            if (!isConnected)
            {
                Log("Discord: Conexion perdida, intentando reconectar...");
                await ConnectAsync();
                
                if (!isConnected) 
                    return; // Si después de intentar reconectar sigue sin haber, salimos
            }

            // 2. Bloqueamos el hilo para que ningún otro mensaje se envíe al mismo tiempo
            await _writeLock.WaitAsync();
            try
            {
                var payload = "{\"cmd\":\"SET_ACTIVITY\",\"args\":{\"pid\":" + System.Diagnostics.Process.GetCurrentProcess().Id + ",\"activity\":{\"state\":\"" + EscapeJson(state) + "\",\"details\":\"" + EscapeJson(details) + "\",\"timestamps\":{\"start\":" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "}}}}";
                var packet = CreatePacket(1, payload);
                
                await pipe.WriteAsync(packet, 0, packet.Length);
                await pipe.FlushAsync();
                Log("Discord: Estado enviado -> " + state);
            }
            catch (Exception ex)
            {
                // 3. SI EL PIPE SE ROMPE: Cerramos la tubería para que Windows la libere
                Log("Discord: Error al enviar (pipe roto) - " + ex.Message);
                Disconnect(); 
            }
            finally
            {
                // 4. Siempre liberamos el candado, pase lo que pase
                _writeLock.Release();
            }
        }

        private byte[] CreatePacket(int opCode, string payload)
        {
            var jsonBytes = Encoding.UTF8.GetBytes(payload);
            var packet = new byte[8 + jsonBytes.Length];
            packet[0] = (byte)(opCode & 0xFF);
            packet[1] = (byte)((opCode >> 8) & 0xFF);
            packet[2] = (byte)((opCode >> 16) & 0xFF);
            packet[3] = (byte)((opCode >> 24) & 0xFF);
            packet[4] = (byte)(jsonBytes.Length & 0xFF);
            packet[5] = (byte)((jsonBytes.Length >> 8) & 0xFF);
            packet[6] = (byte)((jsonBytes.Length >> 16) & 0xFF);
            packet[7] = (byte)((jsonBytes.Length >> 24) & 0xFF);
            Array.Copy(jsonBytes, 0, packet, 8, jsonBytes.Length);
            return packet;
        }

        private string EscapeJson(string s)
        {
            return s != null ? s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "") : "";
        }

        public void Disconnect()
        {
            if (pipe != null)
            {
                try { pipe.Close(); } catch { }
                try { pipe.Dispose(); } catch { } // Forzamos al SO a destruir el pipe roto
                pipe = null;
                isConnected = false;
                Log("Discord: Desconectado");
            }
            else
            {
                isConnected = false;
            }
        }

        public void Dispose()
        {
            Disconnect();
            _writeLock.Dispose(); // Liberamos el candado de memoria
        }

        private void Log(string msg) { OnLog?.Invoke(msg); }
    }
}