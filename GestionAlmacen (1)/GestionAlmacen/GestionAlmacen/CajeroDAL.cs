using System;
using System.Data;

namespace GestionAlmacen
{
    public class CajeroDAL
    {
        public int Insertar(string usuario, string contrasenaPlano, string turno)
        {
            Permisos.ExigirAdministrador();
            return Convert.ToInt32(DbHelper.Escalar("sp_Cajero_Insertar",
                DbHelper.P("@NombreUsuario", SqlDbType.NVarChar, usuario),
                DbHelper.P("@Contrasena", SqlDbType.VarChar, Seguridad.HashSha256(contrasenaPlano)),
                DbHelper.P("@Turno", SqlDbType.NVarChar, turno)));
        }

        // contrasenaPlano vacía = no cambiar la contraseña
        public bool Actualizar(int id, string turno, bool activo, string contrasenaPlano)
        {
            Permisos.ExigirAdministrador();
            object hash = string.IsNullOrEmpty(contrasenaPlano) ? null : Seguridad.HashSha256(contrasenaPlano);
            return Convert.ToInt32(DbHelper.Escalar("sp_Cajero_Actualizar",
                DbHelper.P("@Id", SqlDbType.Int, id),
                DbHelper.P("@Turno", SqlDbType.NVarChar, turno),
                DbHelper.P("@Activo", SqlDbType.Bit, activo),
                DbHelper.P("@Contrasena", SqlDbType.VarChar, hash))) > 0;
        }

        public DataTable Listar()
        {
            return DbHelper.Tabla("sp_Cajero_Listar");
        }
    }
}
