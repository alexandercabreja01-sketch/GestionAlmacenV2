using System;
using System.Data;

namespace GestionAlmacen
{
    public class ClienteDAL
    {
        public int Insertar(string nombre, string telefono, string cedula)
        {
            return Convert.ToInt32(DbHelper.Escalar("sp_Cliente_Insertar",
                DbHelper.P("@Nombre", SqlDbType.NVarChar, nombre),
                DbHelper.P("@Telefono", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim()),
                DbHelper.P("@Cedula", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(cedula) ? null : cedula.Trim())));
        }

        public bool Actualizar(int id, string nombre, string telefono, string cedula)
        {
            return Convert.ToInt32(DbHelper.Escalar("sp_Cliente_Actualizar",
                DbHelper.P("@Id", SqlDbType.Int, id),
                DbHelper.P("@Nombre", SqlDbType.NVarChar, nombre),
                DbHelper.P("@Telefono", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim()),
                DbHelper.P("@Cedula", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(cedula) ? null : cedula.Trim()))) > 0;
        }

        public DataTable Buscar(string texto)
        {
            return DbHelper.Tabla("sp_Cliente_Buscar",
                DbHelper.P("@Texto", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(texto) ? null : texto.Trim()));
        }
    }
}
