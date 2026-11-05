using System;

namespace GestionAlmacen
{
    public static class Turnos
    {
        public const string Manana = "Mañana";
        public const string Tarde = "Tarde";
        public const string Noche = "Noche";

        public static readonly string[] Lista = { Manana, Tarde, Noche };

        // Mañana 6:00-13:59, Tarde 14:00-21:59, Noche 22:00-5:59
        public static string DeHora(DateTime t)
        {
            int h = t.Hour;
            if (h >= 6 && h < 14) return Manana;
            if (h >= 14 && h < 22) return Tarde;
            return Noche;
        }

        public static string Horario(string turno)
        {
            switch (turno)
            {
                case Manana: return "6:00 a.m. a 1:59 p.m.";
                case Tarde: return "2:00 p.m. a 9:59 p.m.";
                case Noche: return "10:00 p.m. a 5:59 a.m.";
                default: return "";
            }
        }
    }
}
