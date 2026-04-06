using System;

namespace ApexGamerBooster.Models
{
    public class Juego
    {
        public string Nombre { get; set; }
        public string RutaExe { get; set; }
        public string Plataforma { get; set; }
        public DateTime FechaAgregado { get; set; } = DateTime.Now;

        public Juego() { }

        public Juego(string nombre, string rutaExe, string plataforma)
        {
            Nombre = nombre;
            RutaExe = rutaExe;
            Plataforma = plataforma;
        }

        public override string ToString() => Nombre;
    }
}