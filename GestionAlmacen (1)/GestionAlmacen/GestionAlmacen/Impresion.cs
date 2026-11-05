using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace GestionAlmacen
{
    public static class Impresion
    {
        private const int Ancho = 70;   // caracteres por línea

        // Abre la vista previa; desde ahí se imprime con el botón de la impresora
        public static void VistaPrevia(Factura f, IWin32Window dueno)
        {
            List<string> lineas = Construir(f);
            int indice = 0;

            PrintDocument doc = new PrintDocument();
            doc.DocumentName = "Factura " + f.NumeroTexto;
            doc.BeginPrint += (s, e) => { indice = 0; };
            doc.PrintPage += (s, e) =>
            {
                using (Font normal = new Font("Courier New", 9f))
                using (Font negrita = new Font("Courier New", 9f, FontStyle.Bold))
                {
                    float alto = normal.GetHeight(e.Graphics);
                    float y = e.MarginBounds.Top;
                    while (indice < lineas.Count && y + alto <= e.MarginBounds.Bottom)
                    {
                        Font fuente = indice < 2 ? negrita : normal;
                        e.Graphics.DrawString(lineas[indice], fuente, Brushes.Black, e.MarginBounds.Left, y);
                        y += alto;
                        indice++;
                    }
                    e.HasMorePages = indice < lineas.Count;
                }
            };

            using (PrintPreviewDialog vista = new PrintPreviewDialog())
            {
                vista.Document = doc;
                vista.Width = 900;
                vista.Height = 720;
                vista.StartPosition = FormStartPosition.CenterScreen;
                vista.ShowDialog(dueno);
            }
        }

        private static string Centrar(string t)
        {
            if (t.Length >= Ancho) return t;
            return new string(' ', (Ancho - t.Length) / 2) + t;
        }

        private static string Par(string etiqueta, string valor)
        {
            return etiqueta.PadRight(Ancho - 20) + valor.PadLeft(20);
        }

        private static string Recortar(string t, int max)
        {
            return t.Length <= max ? t : t.Substring(0, max);
        }

        public static List<string> Construir(Factura f)
        {
            List<string> l = new List<string>();
            string linea = new string('-', Ancho);

            l.Add(Centrar(Config.Negocio));
            l.Add(Centrar("FACTURA"));
            l.Add("");
            l.Add("Factura No.: " + f.NumeroTexto);
            l.Add("Fecha: " + f.Fecha.ToString("dd/MM/yyyy") + "     Hora: " + f.Fecha.ToString("HH:mm:ss"));
            l.Add("Tipo: " + (f.Tipo == Constantes.Credito ? "Crédito" : "Contado"));
            l.Add("Cajero: " + f.Cajero);
            l.Add("Cliente: " + f.Cliente);
            l.Add(linea);
            l.Add("ID".PadRight(6) + "PRODUCTO".PadRight(26) + "CANT".PadLeft(5) +
                  "VALOR".PadLeft(11) + "ITBIS".PadLeft(10) + "SUBTOTAL".PadLeft(12));
            l.Add(linea);

            foreach (LineaFactura x in f.Lineas)
            {
                l.Add(x.IdProducto.ToString().PadRight(6) +
                      Recortar(x.Nombre, 25).PadRight(26) +
                      x.Cantidad.ToString().PadLeft(5) +
                      x.Valor.ToString("N2").PadLeft(11) +
                      x.Itbis.ToString("N2").PadLeft(10) +
                      x.Total.ToString("N2").PadLeft(12));
            }

            l.Add(linea);
            l.Add(Par("Total bruto:", f.TotalBruto.ToString("N2")));
            l.Add(Par("Total ITBIS:", f.TotalItbis.ToString("N2")));
            l.Add(Par("TOTAL A PAGAR:", f.TotalPagar.ToString("N2")));

            if (f.Tipo == Constantes.Credito)
            {
                l.Add("");
                l.Add(Centrar("Venta a crédito - registrada en cuentas por cobrar"));
            }
            else
            {
                l.Add(Par("Monto pagado:", f.MontoPagado.ToString("N2")));
                l.Add(Par("Devuelta:", f.Devuelta.ToString("N2")));
            }

            l.Add("");
            l.Add(Centrar("Gracias por su compra"));
            return l;
        }
    }
}
