using System;
using System.Collections.Generic;

namespace GestionAlmacen
{
    public class LineaFactura
    {
        public int     IdProducto { get; set; }
        public string  Nombre     { get; set; }
        public int     Cantidad   { get; set; }
        public decimal Valor      { get; set; }
        public decimal Itbis      { get; set; }
        public decimal Total      { get; set; }   // Cantidad * Valor (sin ITBIS)
    }

    public class Factura
    {
        public int      Numero      { get; set; }
        public DateTime Fecha       { get; set; }
        public string   Tipo        { get; set; }
        public string   Cajero      { get; set; }
        public string   Turno       { get; set; }
        public int      IdCliente   { get; set; }
        public string   Cliente     { get; set; }
        public decimal  TotalBruto  { get; set; }
        public decimal  TotalItbis  { get; set; }
        public decimal  TotalPagar  { get; set; }
        public decimal  MontoPagado { get; set; }
        public decimal  Devuelta    { get; set; }
        public List<LineaFactura> Lineas = new List<LineaFactura>();

        // Numero de 5 digitos: 00001
        public string NumeroTexto
        {
            get { return Numero.ToString("D5"); }
        }
    }
}
