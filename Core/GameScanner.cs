using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ApexGamerBooster.Models;

namespace ApexGamerBooster.Core
{
    public class GameScanner
    {
        private string configPath;

        public event Action<string> OnLog;
        public List<Juego> Juegos { get; private set; } = new List<Juego>();
        public Juego JuegoFavorito { get; private set; }

        public GameScanner()
        {
            configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ApexGamerBooster", "games.json");
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
        }

        public void ScanAll()
        {
            Juegos.Clear();
            Log("Escaneando bibliotecas...");
            ScanSteam();
            ScanEpic();
            LoadSavedGames();
            Log($"{Juegos.Count} juegos encontrados");
            LoadFavorite();
        }

        private void ScanSteam()
        {
            try
            {
                string steamPath = GetSteamPath();
                if (string.IsNullOrEmpty(steamPath)) return;

                string libraryFolders = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                List<string> libraryPaths = new List<string> { Path.Combine(steamPath, "steamapps") };

                if (File.Exists(libraryFolders))
                {
                    var lines = File.ReadAllLines(libraryFolders);
                    foreach (var line in lines)
                    {
                        if (line.Contains("\"path\""))
                        {
                            var start = line.IndexOf("\"path\"") + 7;
                            var end = line.IndexOf("\"", start + 1);
                            if (end > start)
                            {
                                var path = line.Substring(start + 1, end - start - 2).Replace("\\\\", "\\");
                                var appsPath = Path.Combine(path, "steamapps");
                                if (!libraryPaths.Contains(appsPath) && Directory.Exists(appsPath))
                                    libraryPaths.Add(appsPath);
                            }
                        }
                    }
                }

                int count = 0;
                foreach (var libPath in libraryPaths)
                {
                    var manifests = Directory.GetFiles(libPath, "appmanifest_*.acf");
                    foreach (var manifest in manifests)
                    {
                        try
                        {
                            var game = ParseSteamManifest(manifest, libPath);
                            if (game != null && !Juegos.Any(j => j.Nombre == game.Nombre))
                            {
                                Juegos.Add(game);
                                count++;
                            }
                        }
                        catch { }
                    }
                }
                Log($"Steam: {count} juegos");
            }
            catch (Exception ex)
            {
                Log($"Steam: Error - {ex.Message}");
            }
        }

        private string GetSteamPath()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
                {
                    if (key != null) return key.GetValue("InstallPath")?.ToString();
                }
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    if (key != null) return key.GetValue("SteamPath")?.ToString();
                }
            }
            catch { }
            return null;
        }

        private Juego ParseSteamManifest(string manifestPath, string libPath)
        {
            var content = File.ReadAllText(manifestPath);
            var nameLine = content.Split('\n').FirstOrDefault(l => l.TrimStart().StartsWith("\"name\""));
            if (nameLine == null) return null;
            var name = ExtractVdfValue(nameLine);
            if (string.IsNullOrWhiteSpace(name)) return null;

            string safeName = new string(name.Where(c => !Path.GetInvalidPathChars().Contains(c)).ToArray());
            string gameFolder = Path.Combine(libPath, "common", safeName);
            string exePath = null;

            if (Directory.Exists(gameFolder))
            {
                var exes = Directory.GetFiles(gameFolder, "*.exe", SearchOption.AllDirectories)
                    .Where(e => !e.ToLower().Contains("uninstall") && !e.ToLower().Contains("installer")
                        && !e.ToLower().Contains("setup") && !e.ToLower().Contains("redist")
                        && !e.ToLower().Contains("directx") && !e.ToLower().Contains("vcredist")
                        && !e.ToLower().Contains("commonredist"))
                    .ToList();
                if (exes.Any())
                    exePath = exes.OrderByDescending(e => new FileInfo(e).Length).First();
            }
            if (exePath == null) return null;
            return new Juego(name, exePath, "Steam");
        }

        private string ExtractVdfValue(string line)
        {
            var parts = line.Split('"').Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            return parts.Count >= 2 ? parts[1] : null;
        }

        private void ScanEpic()
        {
            try
            {
                string[] possiblePaths = {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Epic Games"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Epic Games"),
                    @"C:\Epic Games", @"D:\Epic Games", @"E:\Epic Games"
                };
                string epicPath = null;
                foreach (var p in possiblePaths)
                {
                    if (Directory.Exists(p)) { epicPath = p; break; }
                }
                if (epicPath == null) { Log("Epic: No encontrada"); return; }

                int count = 0;
                foreach (var dir in Directory.GetDirectories(epicPath))
                {
                    try
                    {
                        var exes = Directory.GetFiles(dir, "*.exe", SearchOption.AllDirectories)
                            .Where(e => !e.ToLower().Contains("uninstall") && !e.ToLower().Contains("installer")
                                && !e.ToLower().Contains("setup") && !e.ToLower().Contains("redist"))
                            .ToList();
                        if (exes.Any())
                        {
                            var mainExe = exes.OrderByDescending(e => new FileInfo(e).Length).First();
                            string name = Path.GetFileName(dir);
                            try
                            {
                                var info = FileVersionInfo.GetVersionInfo(mainExe);
                                if (!string.IsNullOrEmpty(info.ProductName) && info.ProductName.Length > 2)
                                    name = info.ProductName;
                            }
                            catch { }
                            if (!Juegos.Any(j => j.RutaExe == mainExe))
                            {
                                Juegos.Add(new Juego(name, mainExe, "Epic"));
                                count++;
                            }
                        }
                    }
                    catch { }
                }
                Log($"Epic: {count} juegos");
            }
            catch (Exception ex)
            {
                Log($"Epic: Error - {ex.Message}");
            }
        }

        public void AddManualGame(string exePath)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(exePath);
                try
                {
                    var info = FileVersionInfo.GetVersionInfo(exePath);
                    if (!string.IsNullOrEmpty(info.FileDescription) && info.FileDescription.Length > 2)
                        name = info.FileDescription;
                }
                catch { }
                Juegos.Add(new Juego(name, exePath, "Manual"));
                SaveGames();
                Log($"Agregado: {name}");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        public void RemoveGame(Juego juego)
        {
            Juegos.Remove(juego);
            if (JuegoFavorito == juego) SetFavorite(null);
            SaveGames();
        }

        public void SetFavorite(Juego juego)
        {
            JuegoFavorito = juego;
            try
            {
                string favPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ApexGamerBooster", "favorite.txt");
                File.WriteAllText(favPath, juego?.RutaExe ?? "");
            }
            catch { }
        }

        private void LoadFavorite()
        {
            try
            {
                string favPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ApexGamerBooster", "favorite.txt");
                if (File.Exists(favPath))
                {
                    var path = File.ReadAllText(favPath).Trim();
                    JuegoFavorito = Juegos.FirstOrDefault(j => j.RutaExe == path);
                }
            }
            catch { }
        }

        private void LoadSavedGames()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    var lines = File.ReadAllLines(configPath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('|');
                        if (parts.Length >= 3 && !Juegos.Any(j => j.RutaExe == parts[1]))
                            Juegos.Add(new Juego(parts[0], parts[1], parts[2]));
                    }
                }
            }
            catch { }
        }

        private void SaveGames()
        {
            try
            {
                var manual = Juegos.Where(j => j.Plataforma == "Manual");
                var lines = manual.Select(j => $"{j.Nombre}|{j.RutaExe}|{j.Plataforma}");
                File.WriteAllLines(configPath, lines);
            }
            catch { }
        }

        public bool LaunchGame(Juego juego)
        {
            try
            {
                if (!File.Exists(juego.RutaExe))
                {
                    Log($"No encontrado: {juego.RutaExe}");
                    return false;
                }
                Log($"Lanzando {juego.Nombre}...");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = juego.RutaExe,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(juego.RutaExe)
                    }
                };
                process.Start();
                Log($"{juego.Nombre} iniciado");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
                return false;
            }
        }

        private void Log(string msg) { OnLog?.Invoke(msg); }
    }
}