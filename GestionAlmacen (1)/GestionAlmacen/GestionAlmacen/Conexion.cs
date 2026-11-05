using System.Configuration;
using System.Data.SqlClient;

namespace GestionAlmacen
{
    public static class Conexion
    {
        private static readonly string cadena =
            ConfigurationManager.ConnectionStrings["AlmacenDB"].ConnectionString;

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadena);
        }
    }
}
