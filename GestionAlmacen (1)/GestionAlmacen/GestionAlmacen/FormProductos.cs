using System;
using System.Windows.Forms;

namespace GestionAlmacen
{
    public partial class FormProductos : Form
    {
        private readonly ProductoBLL bll = new ProductoBLL();

        public FormProductos()
        {
            InitializeComponent();
        }

        private void FormProductos_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = "Usuario: " + Sesion.NombreUsuario + "  |  Rol: " + Sesion.Rol;

            cboCriterio.Items.AddRange(new object[] { "Código", "Nombre", "Marca" });
            cboCriterio.SelectedIndex = 1;

            AplicarPermisos();
            CargarProductos();
            LimpiarCampos();
        }

        private void AplicarPermisos()
        {
            bool admin = Sesion.EsAdministrador;

            btnNuevo.Enabled      = admin;
            btnGuardar.Enabled    = false;
            btnActualizar.Enabled = admin;
            btnEliminar.Enabled   = admin;

            txtNombre.ReadOnly = !admin;
            txtPrecio.ReadOnly = !admin;
            txtStock.ReadOnly  = !admin;
            txtMarca.ReadOnly  = !admin;
        }

        private void CargarProductos()
        {
            try { dgvProductos.DataSource = bll.Listar(); }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void LimpiarCampos()
        {
            txtCodigo.Clear();
            txtNombre.Clear();
            txtPrecio.Clear();
            txtStock.Clear();
            txtMarca.Clear();
        }

        private void MostrarError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool LeerProducto(out Producto p)
        {
            p = new Producto();
            decimal precio;
            int stock;

            if (!decimal.TryParse(txtPrecio.Text, out precio))
            {
                MessageBox.Show("El precio no es válido.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return false;
            }
            if (!int.TryParse(txtStock.Text, out stock))
            {
                MessageBox.Show("El stock debe ser un número entero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStock.Focus();
                return false;
            }

            int id;
            int.TryParse(txtCodigo.Text, out id);

            p.Id     = id;
            p.Nombre = txtNombre.Text.Trim();
            p.Precio = precio;
            p.Stock  = stock;
            p.Marca  = txtMarca.Text.Trim();
            return true;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
            dgvProductos.ClearSelection();
            btnGuardar.Enabled    = true;
            btnActualizar.Enabled = false;
            btnEliminar.Enabled   = false;
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            Producto p;
            if (!LeerProducto(out p)) return;

            try
            {
                int nuevoId = bll.Insertar(p);
                ArchivoTxt.Agregar(nuevoId, p);
                MessageBox.Show("Producto registrado con código " + nuevoId + ".",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarProductos();
                LimpiarCampos();
                AplicarPermisos();
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCodigo.Text))
            {
                MessageBox.Show("Seleccione un producto de la tabla.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Producto p;
            if (!LeerProducto(out p)) return;

            try
            {
                if (bll.Actualizar(p))
                {
                    MessageBox.Show("Producto actualizado.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarProductos();
                    LimpiarCampos();
                }
                else
                {
                    MessageBox.Show("No se encontró el producto.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(txtCodigo.Text, out id))
            {
                MessageBox.Show("Seleccione un producto de la tabla.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var r = MessageBox.Show("¿Eliminar el producto \"" + txtNombre.Text + "\"?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            try
            {
                if (bll.Eliminar(id))
                {
                    CargarProductos();
                    LimpiarCampos();
                }
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string texto = txtBuscar.Text.Trim();
            if (texto.Length == 0) { CargarProductos(); return; }

            try
            {
                switch (cboCriterio.SelectedItem.ToString())
                {
                    case "Código":
                        int codigo;
                        if (!int.TryParse(texto, out codigo))
                        {
                            MessageBox.Show("El código debe ser numérico.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        dgvProductos.DataSource = bll.Buscar(codigo, null, null);
                        break;
                    case "Nombre":
                        dgvProductos.DataSource = bll.Buscar(null, texto, null);
                        break;
                    case "Marca":
                        dgvProductos.DataSource = bll.Buscar(null, null, texto);
                        break;
                }
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        private void btnMostrarTodo_Click(object sender, EventArgs e)
        {
            txtBuscar.Clear();
            CargarProductos();
        }

        private void dgvProductos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null || dgvProductos.CurrentRow.Index < 0) return;

            var fila = dgvProductos.CurrentRow;
            txtCodigo.Text = Convert.ToString(fila.Cells["Id"].Value);
            txtNombre.Text = Convert.ToString(fila.Cells["Nombre"].Value);
            txtPrecio.Text = Convert.ToString(fila.Cells["Precio"].Value);
            txtStock.Text  = Convert.ToString(fila.Cells["Stock"].Value);
            txtMarca.Text  = Convert.ToString(fila.Cells["Marca"].Value);

            if (Sesion.EsAdministrador)
            {
                btnGuardar.Enabled    = false;
                btnActualizar.Enabled = true;
                btnEliminar.Enabled   = true;
            }
        }

        // ============ EXPORTAR A TXT (botón y tecla F5) ============
        private void btnExportar_Click(object sender, EventArgs e)
        {
            ExportarTxt();
        }

        private void ExportarTxt()
        {
            try
            {
                ArchivoTxt.ExportarTodo(bll.Listar());
                MessageBox.Show("Archivo guardado en:\n" + ArchivoTxt.Ruta, "Exportado",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MostrarError(ex); }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                ExportarTxt();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
