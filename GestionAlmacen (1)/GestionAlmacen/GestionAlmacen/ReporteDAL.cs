using System;
using System.Data;

namespace GestionAlmacen
{
    public class ReporteDAL
    {
        public DataTable CuadreDia(DateTime fecha, int? idCajero)
        {
            return DbHelper.Tabla("sp_Rep_CuadreDia",
                DbHelper.P("@Fecha", SqlDbType.Date, fecha.Date),
                DbHelper.P("@IdCajero", SqlDbType.Int, idCajero.HasValue ? (object)idCajero.Value : null));
        }

        public DataTable PorCajero(DateTime desde, DateTime hasta)
        {
            return DbHelper.Tabla("sp_Rep_PorCajero",
                DbHelper.P("@Desde", SqlDbType.Date, desde.Date),
                DbHelper.P("@Hasta", SqlDbType.Date, hasta.Date));
        }

        public DataTable Inventario()
        {
            return DbHelper.Tabla("sp_Rep_Inventario");
        }

        public DataTable Facturas(DateTime desde, DateTime hasta, string tipo)
        {
            return DbHelper.Tabla("sp_Rep_Facturas",
                DbHelper.P("@Desde", SqlDbType.Date, desde.Date),
                DbHelper.P("@Hasta", SqlDbType.Date, hasta.Date),
                DbHelper.P("@Tipo", SqlDbType.VarChar, tipo));
        }

        public DataTable TopClientes(DateTime desde, DateTime hasta)
        {
            return DbHelper.Tabla("sp_Rep_TopClientes",
                DbHelper.P("@Desde", SqlDbType.Date, desde.Date),
                DbHelper.P("@Hasta", SqlDbType.Date, hasta.Date));
        }

        public DataTable CuentasPorCobrar()
        {
            return DbHelper.Tabla("sp_Rep_CuentasPorCobrar");
        }
    }
}
