using System;
using System.Windows.Forms;

namespace GestionAlmacen
{
    public partial class FormLogin : Form
    {
        private readonly UsuarioDAL usuarioDAL = new UsuarioDAL();

        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                string.IsNullOrEmpty(txtContrasena.Text))
            {
                MessageBox.Show("Ingrese usuario y contraseña.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (usuarioDAL.ValidarLogin(txtUsuario.Text.Trim(), txtContrasena.Text))
                {
                    // El cajero solo puede entrar dentro de su turno
                    if (Sesion.EsCajero && Config.ValidarTurno &&
                        Sesion.Turno != Turnos.DeHora(DateTime.Now))
                    {
                        string miTurno = Sesion.Turno;
                        Sesion.Cerrar();
                        MessageBox.Show(
                            "Su turno es " + miTurno + " (" + Turnos.Horario(miTurno) + ").\n" +
                            "Ahora corresponde el turno de " + Turnos.DeHora(DateTime.Now) + ".\n\n" +
                            "No puede iniciar sesión fuera de su turno.",
                            "Fuera de turno", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtContrasena.Clear();
                        txtContrasena.Focus();
                        return;
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.", "Acceso denegado",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtContrasena.Clear();
                    txtContrasena.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
