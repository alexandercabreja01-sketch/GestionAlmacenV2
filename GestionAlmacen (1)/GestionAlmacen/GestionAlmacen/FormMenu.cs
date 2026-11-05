using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionAlmacen
{
    public class FormMenu : Form
    {
        public bool CerrarSesion { get; private set; }

        public FormMenu()
        {
            Text = "Menú principal - " + Config.Negocio;
            ClientSize = new Size(380, 440);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            string turno = string.IsNullOrEmpty(Sesion.Turno) ? "" : "  |  Turno: " + Sesion.Turno;
            Label lbl = UI.Etiqueta(this, "Usuario: " + Sesion.NombreUsuario + "  |  Rol: " + Sesion.Rol + turno, 20, 15);
            lbl.Font = new Font(lbl.Font, FontStyle.Bold);

            bool admin = Sesion.EsAdministrador;
            bool cajero = Sesion.EsCajero;

            Boton("Productos" + (admin ? "" : " (consulta)"), 55, true, delegate { Abrir(new FormProductos()); });
            Boton("Facturación", 105, cajero, delegate { Abrir(new FormFacturacion()); });
            Boton("Clientes", 155, admin || cajero, delegate { Abrir(new FormClientes(false)); });
            Boton("Cajeros (crear y administrar)", 205, admin, delegate { Abrir(new FormCajeros()); });
            Boton(cajero ? "Mi cuadre del día" : "Reportes", 255, admin || cajero, delegate { Abrir(new FormReportes()); });
            Boton("Cerrar sesión", 345, true, delegate { CerrarSesion = true; Close(); });
        }

        private void Boton(string texto, int y, bool habilitado, EventHandler click)
        {
            Button b = UI.Boton(this, texto, 40, y, 300, click);
            b.Height = 38;
            b.Enabled = habilitado;
        }

        private void Abrir(Form f)
        {
            using (f)
            {
                f.ShowDialog(this);
            }
        }
    }
}
