using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GestionAlmacen
{
    public class FormClientes : Form
    {
        private readonly ClienteDAL dal = new ClienteDAL();
        private readonly bool seleccion;

        private TextBox txtId, txtNombre, txtTelefono, txtCedula, txtBuscar;
        private Button btnNuevo, btnGuardar, btnActualizar, btnSeleccionar;
        private DataGridView dgv;

        public int ClienteId { get; private set; }
        public string ClienteNombre { get; private set; }

        public FormClientes(bool modoSeleccion)
        {
            seleccion = modoSeleccion;

            Text = seleccion ? "Seleccionar cliente" : "Clientes";
            ClientSize = new Size(760, 500);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(776, 539);

            UI.Etiqueta(this, "Código:", 20, 22);
            txtId = UI.Caja(this, 80, 19, 80);
            txtId.ReadOnly = true;
            UI.Etiqueta(this, "Nombre:", 200, 22);
            txtNombre = UI.Caja(this, 260, 19, 300);
            UI.Etiqueta(this, "Teléfono:", 20, 54);
            txtTelefono = UI.Caja(this, 80, 51, 160);
            UI.Etiqueta(this, "Cédula:", 270, 54);
            txtCedula = UI.Caja(this, 330, 51, 160);

            btnNuevo = UI.Boton(this, "Nuevo", 20, 90, 90, btnNuevo_Click);
            btnGuardar = UI.Boton(this, "Guardar", 120, 90, 90, btnGuardar_Click);
            btnActualizar = UI.Boton(this, "Actualizar", 220, 90, 90, btnActualizar_Click);
            btnSeleccionar = UI.Boton(this, "Seleccionar", 330, 90, 110, btnSeleccionar_Click);
            btnSeleccionar.Visible = seleccion;
            btnGuardar.Enabled = false;
            btnActualizar.Enabled = false;

            UI.Etiqueta(this, "Buscar:", 20, 138);
            txtBuscar = UI.Caja(this, 80, 135, 250);
            UI.Boton(this, "Buscar", 340, 132, 80, delegate { Cargar(txtBuscar.Text); });
            UI.Boton(this, "Mostrar todo", 430, 132, 100, delegate { txtBuscar.Clear(); Cargar(null); });

            dgv = UI.Grilla(this, 20, 175, 720, 305);
            dgv.SelectionChanged += dgv_SelectionChanged;
            dgv.CellDoubleClick += delegate { if (seleccion) btnSeleccionar_Click(null, EventArgs.Empty); };

            Load += delegate { Cargar(null); };
        }

        private void Cargar(string texto)
        {
            try { dgv.DataSource = dal.Buscar(texto); }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void Limpiar()
        {
            txtId.Clear(); txtNombre.Clear(); txtTelefono.Clear(); txtCedula.Clear();
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            Limpiar();
            dgv.ClearSelection();
            btnGuardar.Enabled = true;
            btnActualizar.Enabled = false;
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { UI.Aviso("El nombre es obligatorio."); return; }
            try
            {
                int id = dal.Insertar(txtNombre.Text.Trim(), txtTelefono.Text, txtCedula.Text);
                UI.Info("Cliente registrado con código " + id + ".", "Éxito");
                Limpiar();
                Cargar(null);
            }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(txtId.Text, out id)) { UI.Aviso("Seleccione un cliente de la tabla."); return; }
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { UI.Aviso("El nombre es obligatorio."); return; }
            try
            {
                dal.Actualizar(id, txtNombre.Text.Trim(), txtTelefono.Text, txtCedula.Text);
                UI.Info("Cliente actualizado.", "Éxito");
                Cargar(txtBuscar.Text);
            }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(txtId.Text, out id)) { UI.Aviso("Seleccione un cliente de la tabla."); return; }
            ClienteId = id;
            ClienteNombre = txtNombre.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0 || !dgv.CurrentRow.Selected) return;
            DataGridViewRow fila = dgv.CurrentRow;
            txtId.Text = Convert.ToString(fila.Cells["Id"].Value);
            txtNombre.Text = Convert.ToString(fila.Cells["Nombre"].Value);
            txtTelefono.Text = Convert.ToString(fila.Cells["Telefono"].Value);
            txtCedula.Text = Convert.ToString(fila.Cells["Cedula"].Value);
            btnGuardar.Enabled = false;
            btnActualizar.Enabled = txtId.Text != Constantes.IdClienteContado.ToString();
        }
    }
}
