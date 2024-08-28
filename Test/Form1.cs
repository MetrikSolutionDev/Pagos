using BusinessData.Autorizacion;
using Pagos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
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
            lblTest.Text = e.Mensaje;

            if(e.TipoRespuesta == CommonPago.TipoRespuestaExternaEvento.SOLICITUD_ENVIADA)
                lblTest.Text = "PAGO ID: " + e.Info;

            if (e.PagoConfirmado)
            {
                DatosRespuestaPago sDatosRespuestaPago = e.DatosRespuestaPago;
            }


            if(e.Error)
                lblTest.Text = "ERROR: " + e.Mensaje;

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

        
        IPagos sPago;

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
            sConfiguracion.Id_terminales.Add("SMARTPOS1495000780");
            sConfiguracion.End_point = "https://api.mercadopago.com";
            sConfiguracion.Sub_end_point = "/instore/qr/seller/collectors";
            //sConfiguracion.Sub_end_point = "/point/integration-api";
            sConfiguracion.Sub_end_point_authorization = "/oauth/token";

            //datos de usuario prueba vendedor 1
            //sConfiguracion.Key = "APP_USR-5188075196018449-080710-bee98d20a56bba3893fd625cffd71bd6-1934027353";
            //sConfiguracion.Client_id = "5188075196018449";
            //sConfiguracion.Client_secret = "Q1hAeg5DGAYSeUh3qjR4aOO7WbTh98Dq";
            //sConfiguracion.Code = "";
            //sConfiguracion.User_id = "1360534504";
            //sConfiguracion.Tipo_integracion = CommonPago.TipoIntegracion.POINT;
            //sConfiguracion.Entorno = CommonPago.TipoEntorno.PRODUCCION;
            //sConfiguracion.Token = "APP_USR-5188075196018449-080812-9f84a502fa076a3f2bdfb35151ba428e-1934027353";

            //datos usuario macjey
            sConfiguracion.Key = "APP_USR-4917698465100135-080813-08416c3ee542e95771837aa276eec465-45454237";
            sConfiguracion.Client_id = "4917698465100135";
            sConfiguracion.Client_secret = "WJ7BJQipqt7NrRsTjJnQ7qxHokQP0epf";
            sConfiguracion.Code = "";
            sConfiguracion.User_id = "45454237";
            sConfiguracion.Tipo_integracion = CommonPago.TipoIntegracion.QR;
            sConfiguracion.Entorno = CommonPago.TipoEntorno.PRODUCCION;
            sConfiguracion.Token = "APP_USR-4917698465100135-080814-b0342429f6527e5d407fd6422ca00dcb-45454237";

            //datos usuario ezequiel fortunato
            //sConfiguracion.Key = "APP_USR-7492718820250327-051810-2942febbda71ef7022e5c2c739ad4f97-1360534504";
            //sConfiguracion.Client_id = "7492718820250327";
            //sConfiguracion.Client_secret = "GdTDSZeNcjnQlSpiIZ818HAXSLZrYUeO";
            //sConfiguracion.Code = "";
            //sConfiguracion.User_id = "1360534504";
            //sConfiguracion.Tipo_integracion = CommonPago.TipoIntegracion.QR;
            //sConfiguracion.Entorno = CommonPago.TipoEntorno.PRODUCCION;
            //sConfiguracion.Token = "APP_USR-7492718820250327-051810-2942febbda71ef7022e5c2c739ad4f97-1360534504";

            sConfiguracion.Sucursal = "63740932";
            //sConfiguracion.Pos = "103085572";
            //sConfiguracion.Sucursal = "Local";
            sConfiguracion.Pos = "103089642";
            sConfiguracion.Tiempo_segundos_persistencias = 500;

            sPago = new Pago(sConfiguracion);

            sPago.OnRespuesta += Respuesta;

            //sRequestPago.items.Add(new Item() { sku_number = "00001-555588", category = "marketplace", title = "Point Mini", description = "test", unit_price = 50, quantity = 1, unit_measure = "unit", total_amount = 50 });

            SolicitudPago sSolicitudPago = new SolicitudPago()
            {
                Cuit_cuil = "20341465681",
                //Sucursal = "SUC001_EI",
                //Pos = "SUC001POS001",
                Importe = 8000,
                Nombre_integrador = "MS3",
                Nombre_sistema_integrador = "MS3 POS",
                Version_sistema_integrador = "1.0.0",
                Texto_terminal = "",
                Referencia = "23",
                Titulo = "Titulo"
            };

            sSolicitudPago.Lista_terminales = new List<string>();
            //sSolicitudPago.Lista_terminales.Add("38011127");
            sSolicitudPago.Lista_terminales.Add("PAX_A910__SMARTPOS1495000780");

            sSolicitudPago.Items = new List<Items>();
            sSolicitudPago.Items.Add(new Items() { Codigo = "00001-555588", Categoria = "marketplace", Titulo = "Point Mini", Descripcion = "test", Precio_unitario = 8000, Cantidad = 1, Unidad_medida = "unit", Total = 8000 });

            sPago.EnviarSolicitudPago(sSolicitudPago);







            //IPagos sPago;

            //Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.PRISMA };
            //sConfiguracion.Id_terminales = new List<string>();
            //sConfiguracion.Id_terminales.Add("asd123");
            ////sConfiguracion.End_point = "https://api.prismamediosdepago.com";
            ////sConfiguracion.Sub_end_point = "/v1/paystore_terminals/terminal_payments";
            ////sConfiguracion.Sub_end_point_authorization = "/v1/oauth/accesstoken";
            ////sConfiguracion.Key = "YTczNjhmZWYtMTM0ZS00ZGZlLWI0YzgtMDNmMjkxMjBkNWZlOmNhYmZjM2EzLTRhNWEtNGZiNi1iNjhlLTFjMDhiNjczZGY0Mw==";
            //sConfiguracion.Entorno = CommonPago.TipoEntorno.PRODUCCION;
            ////sConfiguracion.Tiempo_segundos_persistencias = 500;
            ////sConfiguracion.Key = "";

            //sPago = new Pago(sConfiguracion);

            //sPago.OnRespuesta += Respuesta;

            //SolicitudPago sSolicitudPago = new SolicitudPago()
            //{
            //    Cuit_cuil = "30-71267720-8",
            //    Importe = 120000,
            //    Nombre_integrador = "ECR",
            //    Nombre_sistema_integrador = "Software x",
            //    Version_sistema_integrador = "3.5",
            //    Texto_terminal = "Pago x 49",
            //    Referencia = "987654",
            //    Cuotas = 1,
            //    Copias_comprobante_pago = CommonPago.CopiasComprobantePago.NINGUNO,
            //    Nota_impresion_ticket = "nota impresa en el ticket",
            //    Metodo_operacion = CommonPago.MetodoOperacion.TARJETA,
            //    Metodo_impresion = CommonPago.MetodoImpresion.NO_FISCAL,
            //    Admite_tarjeta_beneficio = true
            //};

            //sSolicitudPago.Lista_terminales = new List<string>();
            ////sSolicitudPago.Lista_terminales.Add("38011127");
            //sSolicitudPago.Lista_terminales.Add("16363634");


            //sPago.EnviarSolicitudPago(sSolicitudPago);

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
            //lblSuma.Text = (Convert.ToInt32(lblSuma.Text) + 1).ToString();  


            string jsonString = @"{" +
  "'name': 'SUC04'," +
  "'external_id': 'SUC04'," +
  "'location': {" +
   "                 'street_number': '3039'," +
   " 'street_name': 'Caseros'," +
   " 'city_name': 'Belgrano'," +
   " 'state_name': 'Capital Federal'," +
   " 'latitude': -32.8897322," +
   " 'longitude': -68.8443275," +
   " 'reference': '3er Piso'" +
  "}};";


            HttpClient _httpClient = new HttpClient();
            var uri = "https://api.mercadopago.com/users/1360534504/stores";
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, uri);
            request.Headers.Add("Authorization", "Bearer APP_USR-7492718820250327-051810-2942febbda71ef7022e5c2c739ad4f97-1360534504");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(jsonString, Encoding.UTF8);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = _httpClient.SendAsync(request);
            //response.EnsureSuccessStatusCode();

            //return response;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sAutorizacion.AnularAutorizacion();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            sPago.EnviarCancelacionPago();
        }


        IReversiones sReversion;
        private void button6_Click(object sender, EventArgs e)
        {
            Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.PRISMA };
            sConfiguracion.Id_terminales = new List<string>();
            sConfiguracion.Id_terminales.Add("asd123");
            sConfiguracion.End_point = "https://api-homo.prismamediosdepago.com";
            sConfiguracion.Sub_end_point = "/v1/paystore_terminals/terminal_reversals";
            sConfiguracion.Sub_end_point_authorization = "/v1/oauth/accesstoken";
            sConfiguracion.Key = "YTczNjhmZWYtMTM0ZS00ZGZlLWI0YzgtMDNmMjkxMjBkNWZlOmNhYmZjM2EzLTRhNWEtNGZiNi1iNjhlLTFjMDhiNjczZGY0Mw==";
            sConfiguracion.Entorno = CommonPago.TipoEntorno.HOMOLOGACION;
            sConfiguracion.Tiempo_segundos_persistencias = 500;
            //sConfiguracion.Key = "";

            sReversion = new Pago(sConfiguracion);

            sReversion.OnRespuesta += Respuesta;

            SolicitudReversion sSolicitudReversion = new SolicitudReversion()
            {
                Cuit_cuil = "20-34146568-1",
                Importe = 120000,
                Nombre_integrador = "ECR",
                Nombre_sistema_integrador = "Software x",
                Version_sistema_integrador = "3.5",
                Texto_terminal = "Pago reverso x 49",
                Referencia = "987654",
                Cuotas = 1,
                Copias_comprobante_pago = CommonPago.CopiasComprobantePago.SOLO_CLIENTE,
                Nota_impresion_ticket = "nota impresa en el ticket",
                Metodo_operacion = CommonPago.MetodoOperacion.TARJETA,
                Metodo_impresion = CommonPago.MetodoImpresion.NO_FISCAL,
                Admite_tarjeta_beneficio = true,
                Pago_id = "e5a4488a-5868-4fd2-8c45-b065fa80d536"
            };

            sSolicitudReversion.Lista_terminales = new List<string>();
            sSolicitudReversion.Lista_terminales.Add("38011128");

            sReversion.EnviarSolicitudReversion(sSolicitudReversion);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            sReversion.EnviarCancelacionReversion();
        }

        IDevoluciones sDevolucion;
        private void button8_Click(object sender, EventArgs e)
        {
            Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.PRISMA };
            sConfiguracion.Id_terminales = new List<string>();
            sConfiguracion.Id_terminales.Add("asd123");
            sConfiguracion.End_point = "https://api-homo.prismamediosdepago.com";
            sConfiguracion.Sub_end_point = "/v1/paystore_terminals/terminal_refunds";
            sConfiguracion.Sub_end_point_authorization = "/v1/oauth/accesstoken";
            sConfiguracion.Key = "YTczNjhmZWYtMTM0ZS00ZGZlLWI0YzgtMDNmMjkxMjBkNWZlOmNhYmZjM2EzLTRhNWEtNGZiNi1iNjhlLTFjMDhiNjczZGY0Mw==";
            sConfiguracion.Entorno = CommonPago.TipoEntorno.HOMOLOGACION;
            sConfiguracion.Tiempo_segundos_persistencias = 500;
            //sConfiguracion.Key = "";

            sDevolucion = new Pago(sConfiguracion);

            sDevolucion.OnRespuesta += Respuesta;

            SolicitudDevolucion sSolicitudDevolucion = new SolicitudDevolucion()
            {
                Cuit_cuil = "20-34146568-1",
                Importe = 120000,
                Nombre_integrador = "ECR",
                Nombre_sistema_integrador = "Software x",
                Version_sistema_integrador = "3.5",
                Texto_terminal = "Pago devolucion x 31",
                //Referencia = "987654",
                //Cuotas = 1,
                Copias_comprobante_pago = CommonPago.CopiasComprobantePago.SOLO_CLIENTE,
                //Nota_impresion_ticket = "nota impresa en el ticket",
                //Metodo_operacion = CommonPago.MetodoOperacion.TARJETA,
                //Metodo_impresion = CommonPago.MetodoImpresion.NO_FISCAL,
                //Admite_tarjeta_beneficio = true,
                //Pago_id = "4934ba4e-d375-4848-b73b-9501286cc0d4"
            };

            sSolicitudDevolucion.Lista_terminales = new List<string>();
            sSolicitudDevolucion.Lista_terminales.Add("38011127");

            sDevolucion.EnviarSolicitudDevolucion(sSolicitudDevolucion);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            sDevolucion.EnviarCancelacionDevolucion();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ICierres sCierre;

            Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.PRISMA };
            sConfiguracion.Id_terminales = new List<string>();
            sConfiguracion.Id_terminales.Add("asd123");
            sConfiguracion.End_point = "https://api-homo.prismamediosdepago.com";
            sConfiguracion.Sub_end_point = "/v1/paystore_terminals/terminal_settlements";
            sConfiguracion.Sub_end_point_authorization = "/v1/oauth/accesstoken";
            sConfiguracion.Key = "YTczNjhmZWYtMTM0ZS00ZGZlLWI0YzgtMDNmMjkxMjBkNWZlOmNhYmZjM2EzLTRhNWEtNGZiNi1iNjhlLTFjMDhiNjczZGY0Mw==";
            sConfiguracion.Entorno = CommonPago.TipoEntorno.HOMOLOGACION;
            sConfiguracion.Tiempo_segundos_persistencias = 500;
            //sConfiguracion.Key = "";

            sCierre = new Pago(sConfiguracion);

            sCierre.OnRespuesta += Respuesta;

            SolicitudCierre sSolicitudCierre = new SolicitudCierre()
            {
                Cuit_cuil = "20-34146568-1",
                Terminal = "38011128",
                Imprimir_comprobante = true
            };

            sCierre.EnviarSolicitudCierre(sSolicitudCierre);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            IPagos sPago;

            Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.PRISMA };
            sConfiguracion.Id_terminales = new List<string>();
            sConfiguracion.Id_terminales.Add("asd123");
            sConfiguracion.Entorno = CommonPago.TipoEntorno.HOMOLOGACION;
            
            sPago = new Pago(sConfiguracion);

            sPago.OnRespuesta += Respuesta;

            Cancelacion sCancelacionPago = new Cancelacion()
            {
                Cuit_cuil = "20-34146568-1",
                Entorno = CommonPago.TipoEntorno.HOMOLOGACION,
                Pago_id = "eb418c2a-064d-426e-a133-cbfdaa865e8a"
            };

            sPago.EnviarCancelacionPago(sCancelacionPago);
        }
    }
}
