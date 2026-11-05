using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace GestionAlmacen
{
    // Ventana que se muestra al terminar de facturar:
    // cantidad a pagar, monto pagado y devuelta (contado) o aviso de crédito
    public class FormPago : Form
    {
        private readonly decimal total;
        private readonly bool contado;
        private TextBox txtPagado;
        private Label lblDevuelta;
        private Button btnAceptar;

        public decimal MontoPagado { get; private set; }
        public decimal Devuelta { get; private set; }

        public FormPago(decimal totalAPagar, string tipo, string cliente)
        {
            total = totalAPagar;
            contado = tipo == Constantes.Contado;

            Text = contado ? "Pago al contado" : "Venta a crédito";
            ClientSize = new Size(380, contado ? 230 : 190);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            UI.Etiqueta(this, "Cantidad a pagar:", 20, 22);
            Label lblTotal = UI.Etiqueta(this, total.ToString("N2"), 170, 16);
            lblTotal.Font = new Font("Microsoft Sans Serif", 14f, FontStyle.Bold);

            int yBotones;
            if (contado)
            {
                UI.Etiqueta(this, "Monto pagado:", 20, 70);
                txtPagado = UI.Caja(this, 170, 66, 150);
                txtPagado.Font = new Font("Microsoft Sans Serif", 12f);
                txtPagado.TextChanged += txtPagado_TextChanged;

                UI.Etiqueta(this, "Devuelta:", 20, 115);
                lblDevuelta = UI.Etiqueta(this, "0.00", 170, 110);
                lblDevuelta.Font = new Font("Microsoft Sans Serif", 14f, FontStyle.Bold);
                yBotones = 170;
            }
            else
            {
                Label info = UI.Etiqueta(this,
                    "Cliente: " + cliente + "\nLa factura se registrará en\ncuentas por cobrar.", 20, 70);
                info.AutoSize = true;
                yBotones = 130;
            }

            btnAceptar = UI.Boton(this, "Aceptar", 90, yBotones, 100, btnAceptar_Click);
            Button btnCancelar = UI.Boton(this, "Cancelar", 200, yBotones, 100, delegate { DialogResult = DialogResult.Cancel; Close(); });
            AcceptButton = btnAceptar;
            CancelButton = btnCancelar;

            if (contado)
            {
                btnAceptar.Enabled = false;
                ActiveControl = txtPagado;
            }
        }

        private void txtPagado_TextChanged(object sender, EventArgs e)
        {
            decimal pagado;
            if (decimal.TryParse(txtPagado.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out pagado)
                && pagado >= total)
            {
                lblDevuelta.Text = (pagado - total).ToString("N2");
                btnAceptar.Enabled = true;
            }
            else
            {
                lblDevuelta.Text = "0.00";
                btnAceptar.Enabled = false;
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (contado)
            {
                decimal pagado;
                if (!decimal.TryParse(txtPagado.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out pagado)
                    || pagado < total)
                {
                    UI.Aviso("El monto pagado debe ser igual o mayor al total a pagar.");
                    return;
                }
                MontoPagado = pagado;
                Devuelta = pagado - total;
            }
            else
            {
                MontoPagado = 0;
                Devuelta = 0;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
