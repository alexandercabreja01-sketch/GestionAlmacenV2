using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GestionAlmacen
{
    public class FormFacturacion : Form
    {
        private class ItemProducto
        {
            public Producto P;
            public override string ToString()
            {
                return P.Id + " - " + P.Nombre + " (" + P.Marca + ")  |  Stock: " + P.Stock +
                       "  |  RD$ " + P.Precio.ToString("N2");
            }
        }

        private readonly ProductoBLL productos = new ProductoBLL();
        private readonly FacturaBLL facturaBll = new FacturaBLL();
        private readonly BindingList<LineaFactura> lineas = new BindingList<LineaFactura>();

        private int idCliente = Constantes.IdClienteContado;
        private ComboBox cboTipo, cboProducto;
        private NumericUpDown nudCantidad;
        private Label lblCliente, lblFecha, lblBruto, lblItbis, lblTotal;
        private DataGridView dgv;
        private Timer reloj;

        public FormFacturacion()
        {
            Text = "Facturación";
            ClientSize = new Size(880, 600);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(896, 639);

            Label cab = UI.Etiqueta(this, "FACTURA  -  " + Config.Negocio, 20, 12);
            cab.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold);
            UI.Etiqueta(this, "Cajero: " + Sesion.NombreUsuario + "   |   Turno: " + Sesion.Turno, 20, 40);
            lblFecha = UI.Etiqueta(this, "", 560, 40);

            UI.Etiqueta(this, "Tipo:", 20, 76);
            cboTipo = new ComboBox();
            cboTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipo.Items.AddRange(new object[] { "Contado", "Crédito" });
            cboTipo.SelectedIndex = 0;
            cboTipo.Left = 60; cboTipo.Top = 72; cboTipo.Width = 110;
            cboTipo.SelectedIndexChanged += cboTipo_SelectedIndexChanged;
            Controls.Add(cboTipo);

            UI.Etiqueta(this, "Cliente:", 200, 76);
            lblCliente = UI.Etiqueta(this, Constantes.NombreClienteContado, 255, 76);
            lblCliente.Font = new Font(lblCliente.Font, FontStyle.Bold);
            UI.Boton(this, "Elegir cliente...", 560, 70, 130, delegate { SeleccionarCliente(); });
            UI.Boton(this, "Cliente de contado", 700, 70, 140, delegate { AsignarCliente(Constantes.IdClienteContado, Constantes.NombreClienteContado); });

            UI.Etiqueta(this, "Producto:", 20, 116);
            cboProducto = new ComboBox();
            cboProducto.DropDownStyle = ComboBoxStyle.DropDownList;
            cboProducto.Left = 85; cboProducto.Top = 112; cboProducto.Width = 480;
            Controls.Add(cboProducto);
            UI.Etiqueta(this, "Cantidad:", 590, 116);
            nudCantidad = new NumericUpDown();
            nudCantidad.Minimum = 1; nudCantidad.Maximum = 100000; nudCantidad.Value = 1;
            nudCantidad.Left = 655; nudCantidad.Top = 112; nudCantidad.Width = 70;
            Controls.Add(nudCantidad);
            UI.Boton(this, "Agregar", 745, 109, 95, btnAgregar_Click);

            dgv = UI.Grilla(this, 20, 150, 840, 280);
            dgv.DataSource = lineas;
            dgv.Columns["IdProducto"].HeaderText = "Id";
            dgv.Columns["Nombre"].HeaderText = "Producto";
            dgv.Columns["Itbis"].HeaderText = "ITBIS";
            dgv.Columns["Total"].HeaderText = "Subtotal";
            UI.FormatearDecimales(dgv);

            Button quitar = UI.Boton(this, "Quitar línea", 20, 445, 110, btnQuitar_Click);
            Button cancelar = UI.Boton(this, "Cancelar factura", 140, 445, 130, btnCancelar_Click);
            quitar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            Label t1 = UI.Etiqueta(this, "Total bruto:", 560, 448);
            Label t2 = UI.Etiqueta(this, "Total ITBIS:", 560, 476);
            Label t3 = UI.Etiqueta(this, "TOTAL A PAGAR:", 560, 510);
            t3.Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Bold);
            lblBruto = UI.Etiqueta(this, "0.00", 720, 448);
            lblItbis = UI.Etiqueta(this, "0.00", 720, 476);
            lblTotal = UI.Etiqueta(this, "0.00", 720, 506);
            lblTotal.Font = new Font("Microsoft Sans Serif", 14f, FontStyle.Bold);
            foreach (Control c in new Control[] { t1, t2, t3, lblBruto, lblItbis, lblTotal })
                c.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            Button terminar = UI.Boton(this, "Terminar factura", 20, 540, 250, btnTerminar_Click);
            terminar.Height = 42;
            terminar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            reloj = new Timer();
            reloj.Interval = 1000;
            reloj.Tick += delegate { lblFecha.Text = "Fecha y hora: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); };
            reloj.Start();
            FormClosed += delegate { reloj.Stop(); reloj.Dispose(); };

            Load += delegate { CargarProductos(); lblFecha.Text = "Fecha y hora: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); };
        }

        private string TipoActual
        {
            get { return cboTipo.SelectedIndex == 1 ? Constantes.Credito : Constantes.Contado; }
        }

        private void CargarProductos()
        {
            try
            {
                DataTable dt = productos.Listar();
                cboProducto.Items.Clear();
                foreach (DataRow r in dt.Rows)
                {
                    Producto p = new Producto();
                    p.Id = Convert.ToInt32(r["Id"]);
                    p.Nombre = Convert.ToString(r["Nombre"]);
                    p.Precio = Convert.ToDecimal(r["Precio"]);
                    p.Stock = Convert.ToInt32(r["Stock"]);
                    p.Marca = Convert.ToString(r["Marca"]);
                    if (p.Stock > 0)
                    {
                        ItemProducto it = new ItemProducto();
                        it.P = p;
                        cboProducto.Items.Add(it);
                    }
                }
            }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void AsignarCliente(int id, string nombre)
        {
            idCliente = id;
            lblCliente.Text = nombre;
        }

        private void SeleccionarCliente()
        {
            using (FormClientes f = new FormClientes(true))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                    AsignarCliente(f.ClienteId, f.ClienteNombre);
            }
        }

        private void cboTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Crédito exige un cliente registrado
            if (cboTipo.SelectedIndex == 1 && idCliente == Constantes.IdClienteContado)
            {
                UI.Aviso("Una venta a crédito requiere un cliente registrado. Elija el cliente.");
                SeleccionarCliente();
                if (idCliente == Constantes.IdClienteContado)
                    cboTipo.SelectedIndex = 0;
            }
        }

        private void Recalcular()
        {
            decimal bruto = lineas.Sum(l => l.Total);
            decimal itbis = lineas.Sum(l => l.Itbis);
            lblBruto.Text = bruto.ToString("N2");
            lblItbis.Text = itbis.ToString("N2");
            lblTotal.Text = (bruto + itbis).ToString("N2");
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            ItemProducto item = cboProducto.SelectedItem as ItemProducto;
            if (item == null) { UI.Aviso("Seleccione un producto."); return; }

            int cantidad = (int)nudCantidad.Value;
            LineaFactura existente = lineas.FirstOrDefault(l => l.IdProducto == item.P.Id);
            int yaEnFactura = existente == null ? 0 : existente.Cantidad;

            if (yaEnFactura + cantidad > item.P.Stock)
            {
                UI.Aviso("Stock insuficiente. Disponible: " + item.P.Stock +
                         (yaEnFactura > 0 ? " (ya tiene " + yaEnFactura + " en esta factura)." : "."));
                return;
            }

            LineaFactura nueva = FacturaBLL.CrearLinea(item.P, yaEnFactura + cantidad);
            if (existente != null)
                lineas[lineas.IndexOf(existente)] = nueva;
            else
                lineas.Add(nueva);

            nudCantidad.Value = 1;
            Recalcular();
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0) { UI.Aviso("Seleccione una línea."); return; }
            lineas.RemoveAt(dgv.CurrentRow.Index);
            Recalcular();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (lineas.Count > 0 &&
                MessageBox.Show("¿Cancelar la factura actual?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            Reiniciar();
        }

        private void Reiniciar()
        {
            lineas.Clear();
            AsignarCliente(Constantes.IdClienteContado, Constantes.NombreClienteContado);
            cboTipo.SelectedIndex = 0;
            nudCantidad.Value = 1;
            Recalcular();
        }

        private void btnTerminar_Click(object sender, EventArgs e)
        {
            if (lineas.Count == 0) { UI.Aviso("Agregue al menos un producto."); return; }

            string tipo = TipoActual;
            decimal total = lineas.Sum(l => l.Total + l.Itbis);

            decimal montoPagado;
            using (FormPago pago = new FormPago(total, tipo, lblCliente.Text))
            {
                if (pago.ShowDialog(this) != DialogResult.OK) return;
                montoPagado = pago.MontoPagado;
            }

            Factura factura;
            try
            {
                factura = facturaBll.Facturar(tipo, idCliente, lineas.ToList(), montoPagado);
            }
            catch (Exception ex)
            {
                UI.Error(ex);
                CargarProductos();
                return;
            }

            UI.Info("Factura " + factura.NumeroTexto + " registrada.", "Éxito");
            try
            {
                Impresion.VistaPrevia(factura, this);
            }
            catch (Exception ex)
            {
                UI.Aviso("La factura se guardó, pero no se pudo imprimir: " + ex.Message);
            }

            Reiniciar();
            CargarProductos();
        }
    }
}
