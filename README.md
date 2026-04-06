
C#.NETWindows FormsStatus
🚀 APEX GAMER BOOSTER
Optimización de rendimiento, monitoreo en tiempo real y gestión de juegos para PC. Todo en una interfaz ligera y sin alcohol.

📖 Descripción
ApexGamerBooster es una herramienta integral diseñada para jugadores que buscan extraer el máximo rendimiento de sus equipos. Desarrollada 100% en C# puro con Windows Forms (sin librerías de terceros para la UI), cuenta con un sistema de monitoreo en tiempo real, limpieza profunda del sistema, gestión centralizada de videojuegos e integración nativa con Discord.

✨ Características Principales
📊 Monitoreo en Tiempo Real
Gráficos dinámicos para CPU, RAM y GPU actualizados en tiempo real.
Overlay In-Game: Muestra un HUD liviano por encima de tus juegos con las estadísticas del hardware.
⚡ Game Booster & Limpieza
Modo Boost: Optimiza el sistema con un solo clic para liberar recursos antes de jugar.
Limpieza de RAM: Fuerza la liberación de memoria en uso.
Limpieza de Temporales: Elimina archivos basura del sistema operativo.
Flush DNS: Resetea y optimiza la caché de red para reducir lag.
🎮 Centro de Juegos (Game Hub)
Escaneo automático de juegos instalados en los discos locales.
Interfaz visual tipo Galería: Visualiza tus juegos con sus portadas e íconos extraídos directamente de los .exe.
Menú contextual integrado: Clic derecho en cualquier juego para:
Lanzar con el Boost activado.
Optimizar el sistema para ese juego en específico.
Abrir la carpeta de archivos locales.
Eliminar de la biblioteca.
Actualización automática de Discord: Al lanzar un juego, tu estado de Discord cambia automáticamente a "Jugando [Nombre del juego]".
🌐 Herramientas de Red
Detección de tipo de red (Wi-Fi / Ethernet).
Test de Ping continuo a servidores Google (8.8.8.8).
Visualización de IP Pública, IPv4 e IPv6 local.
🛠️ Sistema y Aplicaciones
Reporte del Sistema: Genera un informe detallado (OS, CPU, GPU, RAM, Placa Madre).
Escáner de Drivers: Detecta los controladores instalados para facilitar su actualización.
Gestor de Apps: Lista las aplicaciones instaladas con opción de ver detalles y desinstalarlas desde la interfaz.
🔧 Notas Técnicas & Recent Fixes
Este proyecto se destaca por no utilizar WPF ni librerías externas para los controles (como MaterialSkin). Toda la UI (bordes redondeados, colores dark mode, animaciones) está dibujada mediante GDI+ y eventos nativos de Windows.

Fixes importantes en la versión actual:

🛠️ [Discord RPC] Fix "Pipe is Broken": Se reconstruyó la conexión IPC con Discord desde cero usando NamedPipeClientStream. Se implementó un mecanismo de Lock (SemaphoreSlim) para evitar saturar el socket y una auto-reconexión silenciosa si el cliente de Discord se reinicia o se minimiza.
🛠️ [Discord RPC] Rate Limiting: Se limitó el envío de actualizaciones de estado a Discord a 1 vez cada 15 segundos (recomendación oficial de la API) para evitar baneos del Client ID.
🛠️ [UI] Game Hub Overhaul: Migración exitosa de un ComboBox estático a un sistema de FlowLayoutPanel con controles UserControl personalizados, permitiendo interactividad por carátulas.
🛠️ Stack Tecnológico
Lenguaje: C# (.NET)
Interfaz: Windows Forms (Custom Rendering)
Arquitectura: Clean Architecture separada por capas (Core, Controls, Models, Utils, Forms con Partial Classes).
Integraciones: Discord Native IPC.
🚀 Cómo ejecutarlo
Asegurate de tener instalado el .NET SDK 8.0 (o la versión que estés usando).
Clona este repositorio:
git clone https://github.com/TU_USUARIO/ApexGamerBooster.git
Navega a la carpeta del proyecto y compilalo:
bash

cd ApexGamerBooster
dotnet build
Ejecuta el .exe generado en la carpeta bin/Release.
🤝 Contribuciones
Las contribuciones son siempre bienvenidas. Si tenés alguna idea para una nueva función (como soporte para Steam API para sacar banners reales en lugar de íconos de .exe), sentite libre de abrir un Issue o un Pull Request.

📜 Licencia
Este proyecto es de código abierto bajo la licencia MIT.


Hecho con 💚 y mucho café por pablo
