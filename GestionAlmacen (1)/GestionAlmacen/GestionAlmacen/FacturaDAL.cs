using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace GestionAlmacen
{
    public class FacturaDAL
    {
        private static SqlCommand Cmd(SqlConnection cn, SqlTransaction tx, string sp)
        {
            SqlCommand cmd = new SqlCommand(sp, cn, tx);
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        // Crea la factura completa en UNA transacción: si algo falla (por ejemplo falta stock) no se guarda nada
        public int Crear(string tipo, int idCajero, int idCliente, IList<LineaFactura> lineas, decimal montoPagado)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    try
                    {
                        int numero;
                        using (SqlCommand cmd = Cmd(cn, tx, "sp_Factura_Crear"))
                        {
                            cmd.Parameters.Add("@Tipo", SqlDbType.VarChar, 7).Value = tipo;
                            cmd.Parameters.Add("@IdCajero", SqlDbType.Int).Value = idCajero;
                            cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;
                            numero = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (LineaFactura l in lineas)
                        {
                            using (SqlCommand cmd = Cmd(cn, tx, "sp_Factura_AgregarLinea"))
                            {
                                cmd.Parameters.Add("@Numero", SqlDbType.Int).Value = numero;
                                cmd.Parameters.Add("@IdProducto", SqlDbType.Int).Value = l.IdProducto;
                                cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = l.Cantidad;
                                SqlParameter tasa = cmd.Parameters.Add("@TasaItbis", SqlDbType.Decimal);
                                tasa.Precision = 5;
                                tasa.Scale = 4;
                                tasa.Value = Config.TasaItbis;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand cmd = Cmd(cn, tx, "sp_Factura_Cerrar"))
                        {
                            cmd.Parameters.Add("@Numero", SqlDbType.Int).Value = numero;
                            SqlParameter pagado = cmd.Parameters.Add("@MontoPagado", SqlDbType.Decimal);
                            pagado.Precision = 18;
                            pagado.Scale = 2;
                            pagado.Value = montoPagado;
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return numero;
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        public Factura Obtener(int numero)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            using (SqlCommand cmd = new SqlCommand("sp_Factura_Obtener", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Numero", SqlDbType.Int).Value = numero;
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;

                    Factura f = new Factura();
                    f.Numero      = dr.GetInt32(0);
                    f.Fecha       = dr.GetDateTime(1);
                    f.Tipo        = dr.GetString(2);
                    f.Cajero      = dr.GetString(3);
                    f.Turno       = dr.IsDBNull(4) ? null : dr.GetString(4);
                    f.IdCliente   = dr.GetInt32(5);
                    f.Cliente     = dr.GetString(6);
                    f.TotalBruto  = dr.GetDecimal(7);
                    f.TotalItbis  = dr.GetDecimal(8);
                    f.TotalPagar  = dr.GetDecimal(9);
                    f.MontoPagado = dr.GetDecimal(10);
                    f.Devuelta    = dr.GetDecimal(11);

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            LineaFactura l = new LineaFactura();
                            l.IdProducto = dr.GetInt32(0);
                            l.Nombre     = dr.GetString(1);
                            l.Cantidad   = dr.GetInt32(2);
                            l.Valor      = dr.GetDecimal(3);
                            l.Itbis      = dr.GetDecimal(4);
                            l.Total      = dr.GetDecimal(5);
                            f.Lineas.Add(l);
                        }
                    }
                    return f;
                }
            }
        }
    }
}
