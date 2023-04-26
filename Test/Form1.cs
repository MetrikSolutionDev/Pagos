using Pagos;
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

        public void Respuesta(object sender, RespuestaExternaEventArgs e)
        {
            //RespuestaConsultaEstadoPago sRespuesta = e.Respuesta;

            //if (sRespuesta.Persistencia_finalizada)
            //{
            //    TextInfo("Finalizado");
            //}
            //else
            //{
            //    TextInfo(sRespuesta.Cantidad_intentos_persistencia.ToString());
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.MERCADO_PAGO };
            sConfiguracion.Id_terminales = new List<string>();
            sConfiguracion.Id_terminales.Add("asd123");
            sConfiguracion.End_point = "https://api.mercadopago.com";
            sConfiguracion.Sub_end_point = "/instore/qr/seller/collectors";
            sConfiguracion.Sub_end_point_authorization = "/oauth/token";
            //sConfiguracion.Key = "APP_USR-968ec046-9c39-488e-bf9b-6c68801f1e83";
            sConfiguracion.Key = "APP_USR-1289759863042338-080412-013d9c7a88fd03f47e975307a774dc7d-62251622";
            sConfiguracion.Client_id = "1410938159947008";
            sConfiguracion.Client_secret = "QpMS8R4iwBHCvaJszhvb4O8iSnOkuuuF";
            //sConfiguracion.Code = "APP_USR-1410938159947008-010508-82a044e95bca4f02aa7f5578690e4abc__LA_LC__-38492007";
            sConfiguracion.Code = "";
            sConfiguracion.User_id = "38492007";
            sConfiguracion.Tipo_integracion = CommonPago.TipoIntegracion.QR;
            sConfiguracion.Token = "APP_USR-1410938159947008-042520-95ae391ea9210556d1f07a31c2a9eb40-38492007";
            //sConfiguracion.Key = "";

            IPagos sPago = new Pago(sConfiguracion);

            sPago.OnRespuesta += Respuesta;

            sPago.EnviarSolicitudPago(
                new SolicitudPago()
                {
                    Cuit_cuil = "20341465681",
                    Sucursal = "CENTRAL",
                    Pos = "79647670",
                    Importe = 100,
                    Nombre_integrador = "MS3",
                    Nombre_sistema_integrador = "MS3 POS",
                    Version_sistema_integrador = "1.0.0",
                    Texto_terminal = ""
                });

            //Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.PRISMA };
            //sConfiguracion.Id_terminales = new List<string>();
            //sConfiguracion.Id_terminales.Add("asd123");
            //sConfiguracion.End_point = "https://api-sandbox.prismamediosdepago.com";
            //sConfiguracion.Sub_end_point = "/v1/paystore_terminals/terminal_payments";
            //sConfiguracion.Sub_end_point_authorization = "/v1/oauth/accesstoken";
            //sConfiguracion.Key = "MkpPaHkwbE9SSmRLNUxRaWh6TW5KdVZyQ3dsb0dZWjc6Rnh3NEdRTmx6b3lCUlgzUQ==";
            ////sConfiguracion.Key = "";

            //IPagos sPago = new Pago(sConfiguracion);

            //sPago.OnRespuesta += Respuesta;

            //sPago.EnviarSolicitudPago(
            //    new SolicitudPago()
            //    {
            //        Cuit_cuil = "20341465681",
            //        Importe = 100,
            //        Nombre_integrador = "MS3",
            //        Nombre_sistema_integrador = "MS3 POS",
            //        Version_sistema_integrador = "1.0.0",
            //        Texto_terminal = ""
            //    });

            //IReversiones sReversion = new Pago(sConfiguracion);

            //sReversion.OnRespuesta += Respuesta;

            //sReversion.EnviarSolicitudPago(
            //    new SolicitudPago()
            //    {
            //        Cuit_cuil = "20341465681",
            //        Importe = 100,
            //        Nombre_integrador = "MS3",
            //        Nombre_sistema_integrador = "MS3 POS",
            //        Version_sistema_integrador = "1.0.0",
            //        Texto_terminal = ""
            //    });

            //sPago.RenovarToken();

            //sPago.EnviarSolicitudPago(new Pagos.Pasarela.Model.SolicitudPago() { Banco = "banco test", Importe = 1000 });

            //sPago.EnviarConsultaEstadoPago(new Pagos.Pasarela.ConsultaEstadoPago() { Pago_id = "asdhfdakfdasjfda" });
        }
    }
}
