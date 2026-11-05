using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GestionAlmacen
{
    public class FormCajeros : Form
    {
        private readonly CajeroDAL dal = new CajeroDAL();

        private TextBox txtId, txtUsuario, txtClave;
        private ComboBox cboTurno;
        private CheckBox chkActivo;
        private Label lblHorario;
        private Button btnNuevo, btnGuardar, btnActualizar;
        private DataGridView dgv;

        public FormCajeros()
        {
            Text = "Cajeros por turno";
            ClientSize = new Size(760, 500);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(776, 539);

            UI.Etiqueta(this, "Código:", 20, 22);
            txtId = UI.Caja(this, 90, 19, 80);
            txtId.ReadOnly = true;
            UI.Etiqueta(this, "Usuario:", 200, 22);
            txtUsuario = UI.Caja(this, 260, 19, 200);
            UI.Etiqueta(this, "Contraseña:", 20, 54);
            txtClave = UI.Caja(this, 90, 51, 170);
            txtClave.UseSystemPasswordChar = true;
            UI.Etiqueta(this, "Turno:", 290, 54);
            cboTurno = new ComboBox();
            cboTurno.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTurno.Items.AddRange(Turnos.Lista);
            cboTurno.SelectedIndex = 0;
            cboTurno.Left = 340; cboTurno.Top = 51; cboTurno.Width = 120;
            cboTurno.SelectedIndexChanged += delegate { lblHorario.Text = Turnos.Horario(Convert.ToString(cboTurno.SelectedItem)); };
            Controls.Add(cboTurno);
            lblHorario = UI.Etiqueta(this, Turnos.Horario(Turnos.Manana), 470, 54);
            chkActivo = new CheckBox();
            chkActivo.Text = "Activo";
            chkActivo.Checked = true;
            chkActivo.Left = 480; chkActivo.Top = 20; chkActivo.AutoSize = true;
            Controls.Add(chkActivo);

            btnNuevo = UI.Boton(this, "Nuevo", 20, 90, 90, btnNuevo_Click);
            btnGuardar = UI.Boton(this, "Guardar", 120, 90, 90, btnGuardar_Click);
            btnActualizar = UI.Boton(this, "Actualizar", 220, 90, 90, btnActualizar_Click);
            btnGuardar.Enabled = false;
            btnActualizar.Enabled = false;

            Label nota = UI.Etiqueta(this,
                "Al actualizar: deje la contraseña vacía para no cambiarla. Desmarque \"Activo\" para impedir el acceso.", 20, 128);
            nota.ForeColor = Color.DimGray;

            dgv = UI.Grilla(this, 20, 155, 720, 325);
            dgv.SelectionChanged += dgv_SelectionChanged;

            Load += delegate { Cargar(); };
        }

        private void Cargar()
        {
            try
            {
                dgv.DataSource = dal.Listar();
                if (dgv.Columns["NombreUsuario"] != null) dgv.Columns["NombreUsuario"].HeaderText = "Usuario";
            }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void Limpiar()
        {
            txtId.Clear(); txtUsuario.Clear(); txtClave.Clear();
            cboTurno.SelectedIndex = 0;
            chkActivo.Checked = true;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            Limpiar();
            dgv.ClearSelection();
            txtUsuario.ReadOnly = false;
            btnGuardar.Enabled = true;
            btnActualizar.Enabled = false;
            txtUsuario.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            if (usuario.Length == 0) { UI.Aviso("El usuario es obligatorio."); return; }
            if (txtClave.Text.Length < 4) { UI.Aviso("La contraseña debe tener al menos 4 caracteres."); return; }
            if (cboTurno.SelectedItem == null) { UI.Aviso("Seleccione el turno."); return; }

            try
            {
                dal.Insertar(usuario, txtClave.Text, Convert.ToString(cboTurno.SelectedItem));
                UI.Info("Cajero creado para el turno " + cboTurno.SelectedItem + ".", "Éxito");
                Limpiar();
                Cargar();
            }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(txtId.Text, out id)) { UI.Aviso("Seleccione un cajero de la tabla."); return; }
            if (txtClave.Text.Length > 0 && txtClave.Text.Length < 4)
            { UI.Aviso("La contraseña debe tener al menos 4 caracteres."); return; }

            try
            {
                dal.Actualizar(id, Convert.ToString(cboTurno.SelectedItem), chkActivo.Checked, txtClave.Text);
                UI.Info("Cajero actualizado.", "Éxito");
                txtClave.Clear();
                Cargar();
            }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0 || !dgv.CurrentRow.Selected) return;
            DataGridViewRow fila = dgv.CurrentRow;
            txtId.Text = Convert.ToString(fila.Cells["Id"].Value);
            txtUsuario.Text = Convert.ToString(fila.Cells["NombreUsuario"].Value);
            txtUsuario.ReadOnly = true;
            txtClave.Clear();
            cboTurno.SelectedItem = Convert.ToString(fila.Cells["Turno"].Value);
            chkActivo.Checked = Convert.ToBoolean(fila.Cells["Activo"].Value);
            btnGuardar.Enabled = false;
            btnActualizar.Enabled = true;
        }
    }
}
