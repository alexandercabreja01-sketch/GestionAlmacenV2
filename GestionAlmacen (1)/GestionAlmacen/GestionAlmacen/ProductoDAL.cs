using System;
using System.Data;
using System.Data.SqlClient;

namespace GestionAlmacen
{
    public class ProductoDAL
    {
        public int Insertar(Producto p)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("sp_Producto_Insertar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = p.Nombre;
                cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = p.Precio;
                cmd.Parameters.Add("@Stock",  SqlDbType.Int).Value = p.Stock;
                cmd.Parameters.Add("@Marca",  SqlDbType.NVarChar, 100).Value = p.Marca;

                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public bool Actualizar(Producto p)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("sp_Producto_Actualizar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id",     SqlDbType.Int).Value = p.Id;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = p.Nombre;
                cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = p.Precio;
                cmd.Parameters.Add("@Stock",  SqlDbType.Int).Value = p.Stock;
                cmd.Parameters.Add("@Marca",  SqlDbType.NVarChar, 100).Value = p.Marca;

                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public bool Eliminar(int id)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("sp_Producto_Eliminar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public DataTable Buscar(int? codigo, string nombre, string marca)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("sp_Producto_Buscar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Codigo", SqlDbType.Int).Value =
                    codigo.HasValue ? (object)codigo.Value : DBNull.Value;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(nombre) ? (object)DBNull.Value : nombre.Trim();
                cmd.Parameters.Add("@Marca", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(marca) ? (object)DBNull.Value : marca.Trim();

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }
}
