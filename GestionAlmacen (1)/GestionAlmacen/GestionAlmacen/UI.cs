using System;
using System.Windows.Forms;

namespace GestionAlmacen
{
    // Ayudas para construir las ventanas por código
    public static class UI
    {
        public static Label Etiqueta(Control padre, string texto, int x, int y)
        {
            Label l = new Label();
            l.Text = texto; l.Left = x; l.Top = y; l.AutoSize = true;
            padre.Controls.Add(l);
            return l;
        }

        public static TextBox Caja(Control padre, int x, int y, int ancho)
        {
            TextBox t = new TextBox();
            t.Left = x; t.Top = y; t.Width = ancho;
            padre.Controls.Add(t);
            return t;
        }

        public static Button Boton(Control padre, string texto, int x, int y, int ancho, EventHandler click)
        {
            Button b = new Button();
            b.Text = texto; b.Left = x; b.Top = y; b.Width = ancho; b.Height = 28;
            b.UseVisualStyleBackColor = true;
            if (click != null) b.Click += click;
            padre.Controls.Add(b);
            return b;
        }

        public static DataGridView Grilla(Control padre, int x, int y, int ancho, int alto)
        {
            DataGridView g = new DataGridView();
            g.Left = x; g.Top = y; g.Width = ancho; g.Height = alto;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.MultiSelect = false;
            g.RowHeadersVisible = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            padre.Controls.Add(g);
            return g;
        }

        public static void FormatearDecimales(DataGridView g)
        {
            foreach (DataGridViewColumn c in g.Columns)
            {
                if (c.ValueType == typeof(decimal))
                {
                    c.DefaultCellStyle.Format = "N2";
                    c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        public static void Aviso(string mensaje)
        {
            MessageBox.Show(mensaje, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Info(string mensaje, string titulo)
        {
            MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Error(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
