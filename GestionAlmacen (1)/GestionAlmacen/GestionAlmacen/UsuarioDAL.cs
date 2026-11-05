using System.Data;
using System.Data.SqlClient;

namespace GestionAlmacen
{
    public class UsuarioDAL
    {
        public bool ValidarLogin(string usuario, string contrasenaPlano)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("sp_Usuario_Login", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 50).Value = usuario;
                cmd.Parameters.Add("@Contrasena", SqlDbType.VarChar, 64).Value =
                    Seguridad.HashSha256(contrasenaPlano);

                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        Sesion.IdUsuario     = dr.GetInt32(0);
                        Sesion.NombreUsuario = dr.GetString(1);
                        Sesion.Rol           = dr.GetString(2);
                        Sesion.Turno         = dr.IsDBNull(3) ? null : dr.GetString(3);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
