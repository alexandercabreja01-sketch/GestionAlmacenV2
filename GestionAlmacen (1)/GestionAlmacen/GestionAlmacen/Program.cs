using System;
using System.Windows.Forms;

namespace GestionAlmacen
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {
                using (FormLogin login = new FormLogin())
                {
                    if (login.ShowDialog() != DialogResult.OK) break;
                }

                bool otraSesion;
                using (FormMenu menu = new FormMenu())
                {
                    menu.ShowDialog();
                    otraSesion = menu.CerrarSesion;
                }

                Sesion.Cerrar();
                if (!otraSesion) break;
            }
        }
    }
}
