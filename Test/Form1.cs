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
            //Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.MERCADO_PAGO };
            //sConfiguracion.Id_terminales = new List<string>();
            //sConfiguracion.Id_terminales.Add("asd123");
            //sConfiguracion.End_point = "https://api.mercadopago.com";
            //sConfiguracion.Sub_end_point = "/instore/qr/seller/collectors";
            //sConfiguracion.Sub_end_point_authorization = "/oauth/token";
            ////sConfiguracion.Key = "APP_USR-968ec046-9c39-488e-bf9b-6c68801f1e83";
            ////sConfiguracion.Key = "APP_USR-1410938159947008-010508-82a044e95bca4f02aa7f5578690e4abc__LA_LC__-38492007";
            //sConfiguracion.Key = "APP_USR-6539079267436236-080420-b8d7b71adf9e8fe652cd014690cad1b4-620875575";
            //sConfiguracion.Client_id = "6539079267436236";
            //sConfiguracion.Client_secret = "s0RdcUitJaxrajARixEpxUHiMctYK3V2";
            ////sConfiguracion.Code = "APP_USR-1410938159947008-010508-82a044e95bca4f02aa7f5578690e4abc__LA_LC__-38492007";
            //sConfiguracion.Code = "";
            //sConfiguracion.User_id = "620875575";
            //sConfiguracion.Tipo_integracion = CommonPago.TipoIntegracion.QR;
            //sConfiguracion.Entorno = CommonPago.TipoEntorno.PRUEBA;
            ////sConfiguracion.Token = "APP_USR-1410938159947008-042617-5054d82980c797bc6136d2d388ea35df-38492007";

            Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.MERCADO_PAGO };
            sConfiguracion.Id_terminales = new List<string>();
            sConfiguracion.Id_terminales.Add("asd123");
            sConfiguracion.End_point = "https://api.mercadopago.com";
            sConfiguracion.Sub_end_point = "/point/integration-api";
            sConfiguracion.Sub_end_point_authorization = "/oauth/token";
            sConfiguracion.Key = "APP_USR-6829482887143586-042714-97a5d4d0f26bf8fd3b5d5b19f8bf172b-1360534504";
            sConfiguracion.Client_id = "6829482887143586";
            sConfiguracion.Client_secret = "AGDe3Hx6zUzSyigt4DzfjpKuUok8Y6vh";
            sConfiguracion.Code = "";
            sConfiguracion.User_id = "1360534504";
            sConfiguracion.Tipo_integracion = CommonPago.TipoIntegracion.POINT;
            sConfiguracion.Entorno = CommonPago.TipoEntorno.PRUEBA;
            
            IPagos sPago = new Pago(sConfiguracion);

            sPago.OnRespuesta += Respuesta;

            sPago.EnviarSolicitudPago(
                new SolicitudPago()
                {
                    Cuit_cuil = "20341465681",
                    Sucursal = "54988848",
                    //Sucursal = "32785082",
                    Pos = "987654",
                    Importe = 50,
                    Nombre_integrador = "MS3",
                    Nombre_sistema_integrador = "MS3 POS",
                    Version_sistema_integrador = "1.0.0",
                    Texto_terminal = "",
                    Referencia = "123456",
                    Lista_terminales = sConfiguracion.Id_terminales
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
