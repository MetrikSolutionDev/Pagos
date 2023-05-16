using BusinessData.Autorizacion;
using Pagos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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
            sConfiguracion.Id_terminales.Add("2AB7X-CHIPPERBT");
            sConfiguracion.End_point = "https://api.mercadopago.com";
            sConfiguracion.Sub_end_point = "/instore/qr/seller/collectors";
            sConfiguracion.Sub_end_point_authorization = "/oauth/token";
            sConfiguracion.Key = "APP_USR-1289759863042338-080412-013d9c7a88fd03f47e975307a774dc7d-62251622";
            sConfiguracion.Client_id = "1289759863042338";
            sConfiguracion.Client_secret = "9JuptwnCUA9v2VIwfWnJ0LwH5xP7r9co";
            sConfiguracion.Code = "";
            sConfiguracion.User_id = "62251622";
            sConfiguracion.Tipo_integracion = CommonPago.TipoIntegracion.QR;
            sConfiguracion.Entorno = CommonPago.TipoEntorno.PRODUCCION;
            sConfiguracion.Token = "APP_USR-1289759863042338-080412-013d9c7a88fd03f47e975307a774dc7d-62251622";

            IPagos sPago = new Pago(sConfiguracion);

            sPago.OnRespuesta += Respuesta;

            //sRequestPago.items.Add(new Item() { sku_number = "00001-555588", category = "marketplace", title = "Point Mini", description = "test", unit_price = 50, quantity = 1, unit_measure = "unit", total_amount = 50 });
            
            sPago.EnviarSolicitudPago(
                new SolicitudPago()
                { 
                    Cuit_cuil = "20341465681",
                    Sucursal = "SUC001_EI",
                    Pos = "SUC001POS001",
                    Importe = 50,
                    Nombre_integrador = "MS3",
                    Nombre_sistema_integrador = "MS3 POS",
                    Version_sistema_integrador = "1.0.0",
                    Texto_terminal = "",
                    Referencia = "123456"   
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

        public void Respuesta(object sender, RespuestaAutorizacionEventArgs e)
        {
            //lblError.Text = e.Mensaje;
        }

        private AutorizacionEntidad sAutorizacion = new AutorizacionEntidad();

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                sAutorizacion.Tipo_auditoria_id = BusinessData.CommonMT.Data.TipoAuditoria.EGRESO_DINERO;
                sAutorizacion.OnRespuesta += Respuesta;
                sAutorizacion.Descripcion = "1";
                sAutorizacion.Punto_venta_id = 31;
                sAutorizacion.Sucursal_id = 1;
                sAutorizacion.SolicitarAutorizacion();
            }
            catch (Exception ex)
            {
                //lblError.Text = ex.Message;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            lblSuma.Text = (Convert.ToInt32(lblSuma.Text) + 1).ToString();  
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sAutorizacion.AnularAutorizacion();
        }
    }
}
