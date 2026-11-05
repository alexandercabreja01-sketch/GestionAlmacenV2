using System;
using System.Collections.Generic;
using System.Linq;

namespace GestionAlmacen
{
    public class FacturaBLL
    {
        private readonly FacturaDAL dal = new FacturaDAL();

        public static decimal Redondear(decimal v)
        {
            return Math.Round(v, 2, MidpointRounding.AwayFromZero);
        }

        public static LineaFactura CrearLinea(Producto p, int cantidad)
        {
            LineaFactura l = new LineaFactura();
            l.IdProducto = p.Id;
            l.Nombre     = p.Nombre;
            l.Cantidad   = cantidad;
            l.Valor      = p.Precio;
            l.Total      = Redondear(p.Precio * cantidad);
            l.Itbis      = Redondear(p.Precio * cantidad * Config.TasaItbis);
            return l;
        }

        public Factura Facturar(string tipo, int idCliente, IList<LineaFactura> lineas, decimal montoPagado)
        {
            Permisos.ExigirCajero();

            if (tipo != Constantes.Contado && tipo != Constantes.Credito)
                throw new ArgumentException("Tipo de factura no válido.");
            if (lineas == null || lineas.Count == 0)
                throw new ArgumentException("La factura no tiene productos.");
            if (tipo == Constantes.Credito && idCliente == Constantes.IdClienteContado)
                throw new ArgumentException("Una venta a crédito requiere un cliente registrado.");

            decimal total = lineas.Sum(l => l.Total + l.Itbis);
            if (tipo == Constantes.Contado && montoPagado < total)
                throw new ArgumentException("El monto pagado es menor al total a pagar.");

            int numero = dal.Crear(tipo, Sesion.IdUsuario, idCliente, lineas, montoPagado);
            return dal.Obtener(numero);
        }
    }
}
