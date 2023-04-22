using Pagos.Pasarela;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        delegate void SetTextCallback(string xDetalle);
        private void TextInfo(string xDetalle)
        {
            try
            {
                if (this.lblTest.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(TextInfo);
                    this.Invoke(d, new object[] { xDetalle });
                }
                else
                {
                    if (!string.IsNullOrEmpty(xDetalle))
                    {
                        this.lblTest.Text = xDetalle;
                    }
                }
            }
            catch { }
        }

        public void Respuesta(object sender, RespuestaPagoEventArgs e)
        {
            RespuestaConsultaEstadoPago sRespuesta = e.Respuesta;

            if (sRespuesta.Persistencia_finalizada)
            {
                TextInfo("Finalizado");
            }
            else
            {
                TextInfo(sRespuesta.Cantidad_intentos_persistencia.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Configuracion sConfiguracion = new Configuracion() { Cantidad_persistencias_pago = 10, Tipo = Pagos.CommonPago.Tipo.PRISMA };
            sConfiguracion.Id_equipos = new List<string>();
            sConfiguracion.Id_equipos.Add("asd123");
            sConfiguracion.End_point = "https://api-sandbox.prismamediosdepago.com";
            sConfiguracion.Sub_end_point = "/v1/paystore_terminals/terminal_payments";
            sConfiguracion.Sub_end_point_authorization = "/v1/oauth/accesstoken";
            //sConfiguracion.Key = "MkpPaHkwbE9SSmRLNUxRaWh6TW5KdVZyQ3dsb0dZWjc6Rnh3NEdRTmx6b3lCUlgzUQ==";
            sConfiguracion.Key = "";

            IPago sPago = PagoFactory.Instance(sConfiguracion);

            sPago.OnRespuestaPago += Respuesta;

            sPago.EnviarSolicitudPago(
                new SolicitudPago()
                {
                    Cuit_cuil = "20341465681",
                    Importe = 100,
                    Nombre_integrador = "MS3",
                    Nombre_sistema_integrador = "MS3 POS",
                    Version_sistema_integrador = "1.0.0",
                    Texto_terminal = ""
                });

            //sPago.RenovarToken();

            //sPago.EnviarSolicitudPago(new Pagos.Pasarela.Model.SolicitudPago() { Banco = "banco test", Importe = 1000 });

            //sPago.EnviarConsultaEstadoPago(new Pagos.Pasarela.ConsultaEstadoPago() { Pago_id = "asdhfdakfdasjfda" });
        }
    }
}
