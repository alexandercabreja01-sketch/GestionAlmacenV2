using System;
using System.Data;
using System.Data.SqlClient;

namespace GestionAlmacen
{
    // Ayuda para ejecutar procedimientos almacenados de forma corta
    public static class DbHelper
    {
        public static SqlParameter P(string nombre, SqlDbType tipo, object valor)
        {
            SqlParameter p = new SqlParameter(nombre, tipo);
            p.Value = valor ?? DBNull.Value;
            return p;
        }

        public static DataTable Tabla(string sp, params SqlParameter[] ps)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (ps != null) cmd.Parameters.AddRange(ps);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static object Escalar(string sp, params SqlParameter[] ps)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (ps != null) cmd.Parameters.AddRange(ps);
                cn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}
