using System;
using System.Configuration;
using System.Globalization;

namespace GestionAlmacen
{
    public static class Config
    {
        public static decimal TasaItbis
        {
            get
            {
                decimal t;
                string s = ConfigurationManager.AppSettings["TasaItbis"];
                if (!decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out t))
                    t = 0.18m;
                return t;
            }
        }

        public static bool ValidarTurno
        {
            get
            {
                string s = ConfigurationManager.AppSettings["ValidarTurno"];
                return s == null || !s.Trim().Equals("false", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string Negocio
        {
            get
            {
                string s = ConfigurationManager.AppSettings["Negocio"];
                return string.IsNullOrWhiteSpace(s) ? "ALMACÉN" : s;
            }
        }
    }
}
