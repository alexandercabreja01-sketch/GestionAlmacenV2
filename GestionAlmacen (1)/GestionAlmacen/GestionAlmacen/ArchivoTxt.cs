using System;
using System.Data;
using System.IO;
using System.Text;

namespace GestionAlmacen
{
    public static class ArchivoTxt
    {
        // Se guarda en Documentos\productos.txt
        public static readonly string Ruta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "productos.txt");

        // Agrega una línea al final cada vez que se registra un producto
        public static void Agregar(int id, Producto p)
        {
            string linea = string.Join(" | ", id, p.Nombre, p.Precio, p.Stock, p.Marca);
            File.AppendAllText(Ruta, linea + Environment.NewLine, Encoding.UTF8);
        }

        // Reescribe el archivo completo con lo que hay en la tabla
        public static void ExportarTodo(DataTable dt)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id | Nombre | Precio | Stock | Marca");
            foreach (DataRow f in dt.Rows)
                sb.AppendLine(string.Join(" | ", f["Id"], f["Nombre"], f["Precio"], f["Stock"], f["Marca"]));
            File.WriteAllText(Ruta, sb.ToString(), Encoding.UTF8);
        }
    }
}
