using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GestionAlmacen
{
    public class FormReportes : Form
    {
        private class ItemCajero
        {
            public int Id;
            public string Texto;
            public override string ToString() { return Texto; }
        }

        private readonly ReporteDAL dal = new ReporteDAL();
        private readonly CajeroDAL cajeros = new CajeroDAL();

        private ComboBox cboReporte, cboCajero, cboTipo;
        private DateTimePicker dtpDesde, dtpHasta;
        private DataGridView dgv;
        private Label lblResumen;

        private const string RCuadre = "Cuadre del día";
        private const string RPorCajero = "Ventas por cajero";
        private const string RInventario = "Inventario en stock";
        private const string RFacturas = "Facturas a crédito / contado";
        private const string RTop = "5 clientes con más compras";
        private const string RCxC = "Cuentas por cobrar";

        public FormReportes()
        {
            Text = Sesion.EsCajero ? "Mi cuadre del día" : "Reportes";
            ClientSize = new Size(960, 600);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(976, 639);

            UI.Etiqueta(this, "Reporte:", 20, 22);
            cboReporte = new ComboBox();
            cboReporte.DropDownStyle = ComboBoxStyle.DropDownList;
            if (Sesion.EsCajero)
                cboReporte.Items.Add(RCuadre);
            else
                cboReporte.Items.AddRange(new object[] { RCuadre, RPorCajero, RInventario, RFacturas, RTop, RCxC });
            cboReporte.Left = 80; cboReporte.Top = 18; cboReporte.Width = 250;
            cboReporte.SelectedIndexChanged += cboReporte_SelectedIndexChanged;
            Controls.Add(cboReporte);

            UI.Etiqueta(this, "Desde / Fecha:", 350, 22);
            dtpDesde = Fecha(445, 18);
            UI.Etiqueta(this, "Hasta:", 570, 22);
            dtpHasta = Fecha(615, 18);

            UI.Etiqueta(this, "Cajero:", 20, 58);
            cboCajero = new ComboBox();
            cboCajero.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCajero.Left = 80; cboCajero.Top = 54; cboCajero.Width = 200;
            Controls.Add(cboCajero);

            UI.Etiqueta(this, "Tipo:", 350, 58);
            cboTipo = new ComboBox();
            cboTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTipo.Items.AddRange(new object[] { "Todas", "Contado", "Crédito" });
            cboTipo.SelectedIndex = 0;
            cboTipo.Left = 445; cboTipo.Top = 54; cboTipo.Width = 110;
            Controls.Add(cboTipo);

            UI.Boton(this, "Generar", 800, 40, 130, btnGenerar_Click);

            dgv = UI.Grilla(this, 20, 95, 920, 450);
            lblResumen = UI.Etiqueta(this, "", 20, 560);
            lblResumen.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblResumen.Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold);

            Load += FormReportes_Load;
        }

        private DateTimePicker Fecha(int x, int y)
        {
            DateTimePicker d = new DateTimePicker();
            d.Format = DateTimePickerFormat.Short;
            d.Left = x; d.Top = y; d.Width = 110;
            Controls.Add(d);
            return d;
        }

        private void FormReportes_Load(object sender, EventArgs e)
        {
            try
            {
                cboCajero.Items.Clear();
                if (Sesion.EsCajero)
                {
                    ItemCajero yo = new ItemCajero();
                    yo.Id = Sesion.IdUsuario;
                    yo.Texto = Sesion.NombreUsuario;
                    cboCajero.Items.Add(yo);
                    cboCajero.SelectedIndex = 0;
                    cboCajero.Enabled = false;
                }
                else
                {
                    ItemCajero todos = new ItemCajero();
                    todos.Id = 0;
                    todos.Texto = "Todos";
                    cboCajero.Items.Add(todos);
                    foreach (DataRow r in cajeros.Listar().Rows)
                    {
                        ItemCajero c = new ItemCajero();
                        c.Id = Convert.ToInt32(r["Id"]);
                        c.Texto = Convert.ToString(r["NombreUsuario"]) + " (" + Convert.ToString(r["Turno"]) + ")";
                        cboCajero.Items.Add(c);
                    }
                    cboCajero.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { UI.Error(ex); }

            cboReporte.SelectedIndex = 0;
        }

        private void cboReporte_SelectedIndexChanged(object sender, EventArgs e)
        {
            string r = Convert.ToString(cboReporte.SelectedItem);
            bool rango = r == RPorCajero || r == RFacturas || r == RTop;

            dtpDesde.Enabled = r == RCuadre || rango;
            dtpHasta.Enabled = rango;
            cboCajero.Enabled = r == RCuadre && !Sesion.EsCajero;
            cboTipo.Enabled = r == RFacturas;

            dtpDesde.Value = r == RTop ? new DateTime(DateTime.Today.Year, 1, 1) : DateTime.Today;
            dtpHasta.Value = DateTime.Today;
            dgv.DataSource = null;
            lblResumen.Text = "";
        }

        private static decimal Sumar(DataTable dt, string columna)
        {
            object o = dt.Compute("SUM(" + columna + ")", "");
            return o == DBNull.Value ? 0m : Convert.ToDecimal(o);
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            string r = Convert.ToString(cboReporte.SelectedItem);
            DateTime desde = dtpDesde.Value.Date;
            DateTime hasta = dtpHasta.Value.Date;

            if (dtpHasta.Enabled && hasta < desde) { UI.Aviso("La fecha 'Hasta' no puede ser menor que 'Desde'."); return; }

            try
            {
                DataTable dt;
                string resumen = "";

                if (r == RCuadre)
                {
                    ItemCajero c = cboCajero.SelectedItem as ItemCajero;
                    int? id = (c == null || c.Id == 0) ? (int?)null : c.Id;
                    dt = dal.CuadreDia(desde, id);
                    resumen = "Cuadre del " + desde.ToString("dd/MM/yyyy") + " - " + (c == null ? "Todos" : c.Texto);
                }
                else if (r == RPorCajero)
                {
                    dt = dal.PorCajero(desde, hasta);
                }
                else if (r == RInventario)
                {
                    dt = dal.Inventario();
                    resumen = "Valor total del inventario: " + Sumar(dt, "Valor").ToString("N2");
                }
                else if (r == RFacturas)
                {
                    string tipo = cboTipo.SelectedIndex == 1 ? Constantes.Contado :
                                  cboTipo.SelectedIndex == 2 ? Constantes.Credito : null;
                    dt = dal.Facturas(desde, hasta, tipo);
                    resumen = dt.Rows.Count + " factura(s) - Total: " + Sumar(dt, "Total").ToString("N2");
                }
                else if (r == RTop)
                {
                    dt = dal.TopClientes(desde, hasta);
                }
                else
                {
                    dt = dal.CuentasPorCobrar();
                    resumen = "Saldo total por cobrar: " + Sumar(dt, "Saldo").ToString("N2");
                }

                dgv.DataSource = dt;
                UI.FormatearDecimales(dgv);
                lblResumen.Text = resumen;
                if (dt.Rows.Count == 0) lblResumen.Text = "Sin resultados para los criterios seleccionados.";
            }
            catch (Exception ex) { UI.Error(ex); }
        }
    }
}
