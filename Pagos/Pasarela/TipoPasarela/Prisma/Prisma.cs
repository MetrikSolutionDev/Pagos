using Core.Api;
using Core.Api.Authentication;
using Core.Business;
using Pagos.Pasarela.Eventos;
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Pagos.Pasarela
{
    public class Prisma : PagoBase, IPago
    {   public Prisma(Configuracion xConfiguracion)
        {
            _configuracion = xConfiguracion;
            SetClient();

            OnRespuestaInt += Respuesta;
        }

        public event RespuestaPagoHandler OnRespuestaPago;
        public event RespuestaHandler OnRespuesta;

        public void Respuesta(object sender, RespuestaEventArgs e)
        {
            switch (e.TipoRespuesta) 
            {
                case RespuestaEventArgs.Tipo.TOKEN:

                    TokenResponse sRespuestaToken = (TokenResponse)e.Respuesta;
                    Token = sRespuestaToken.access_token != null ? sRespuestaToken.access_token : "";
                    break;

                case RespuestaEventArgs.Tipo.SOLICITUD_PAGO:

                    RequestPago sRespuestaSolicitudPago = (RequestPago)e.Respuesta;

                    if (sRespuestaSolicitudPago.errors != null && sRespuestaSolicitudPago.errors.Any(er => er.code == "401")) 
                    {
                        RenovarToken();

                        //EnviarSolicitudPagoService<RequestPago, RequestPago>("/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPago);
                    }

                    break;
            }
        }

        public RespuestaConsultaEstadoPago ConsultaEstadoPagoPersistente(ConsultaEstadoPago xModel)
        {
            RequestPago sRequest = new RequestPago();
            //sRequest.cuit_cuil = "30123456789";
            //sRequest.payment_request_data.subnet_acquirer_id = "";
            //sRequest.payment_data.payment_id = "";

            ////HAGO LA CONSULTA A LA API
            //HttpContent sHttpContent = new StringContent(JsonConvert.SerializeObject(new { cuit_cuil = "30123456789", subnet_acquirer_id = "1", payment_id = "a12e6d15-959a-4c4a-99ce-76b70c546200" }), Encoding.UTF8);

            //var sReturn = _client.PostAsync(_configuracion.Sub_end_point + "/payments/cuit_cuil?" + xModel.c, sHttpContent).Result;

            return new RespuestaConsultaEstadoPago() { Confirmado = false, Lote = "123456789" };
        }

        public bool EnviarConsultaEstadoPago(ConsultaEstadoPago xModel)
        {
            PersistirConsultaPago(ConsultaEstadoPagoPersistente, xModel, ref OnRespuestaPago);

            return true;
        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            RequestPago sRequestPago = new RequestPago();
            sRequestPago.payment_request_data = new Payment_request_data();
            sRequestPago.payment_request_data.subnet_acquirer_id = "1";
            sRequestPago.payment_request_data.payment_amount = xModel.Importe.ToString("N2");
            sRequestPago.payment_request_data.terminal_menu_text = xModel.Texto_terminal;
            sRequestPago.payment_request_data.ecr_transaction_id = xModel.Referencia.ContainValueString() ? xModel.Referencia : null;
            sRequestPago.payment_request_data.installments_number = xModel.Cuotas;
            sRequestPago.payment_request_data.ecr_provider = xModel.Nombre_integrador;
            sRequestPago.payment_request_data.ecr_name = xModel.Nombre_sistema_integrador;
            sRequestPago.payment_request_data.ecr_version = xModel.Version_sistema_integrador;
            sRequestPago.payment_request_data.change_amount = xModel.Importe_cambio.ToString();

            int sResultadoParseoInt = 0;
            bool sParseoHabilitado = int.TryParse(xModel.Tipo_cuenta, out sResultadoParseoInt);

            sRequestPago.payment_request_data.bank_account_type = sParseoHabilitado ? Convert.ToInt32(xModel.Tipo_cuenta) : 0;
            sRequestPago.payment_request_data.payment_plan_id = xModel.Identificacion_plan_pago.ContainValueString() ? xModel.Identificacion_plan_pago : null;

            switch (xModel.Metodo_impresion)
            {
                case CommonPago.MetodoImpresion.FISCAL:
                    sRequestPago.payment_request_data.print_method = Enums.PrintMethod.MOBITEF_FISCAL;
                    break;

                case CommonPago.MetodoImpresion.NO_FISCAL:
                    sRequestPago.payment_request_data.print_method = Enums.PrintMethod.MOBITEF_NON_FISCAL;
                    break;
            }

            switch (xModel.Copias_comprobante_pago)
            {
                case CommonPago.CopiasComprobantePago.NINGUNO:
                    sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.NONE;
                    break;

                case CommonPago.CopiasComprobantePago.AMBOS:
                    sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.BOTH;
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_CLIENTE:
                    sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.CUSTOMER_ONLY;
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_COMERCIANTE:
                    sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.MERCHANT_ONLY;
                    break;

            }

            sRequestPago.payment_request_data.terminals_list = new List<Terminal>();

            foreach (string sTerminalItem in xModel.Lista_terminales)
            {
                Terminal sTerminal = new Terminal();
                sTerminal.Termina_id = sTerminalItem;

                sRequestPago.payment_request_data.terminals_list.Add(sTerminal);
            }

            sRequestPago.payment_request_data.card_brand_product = xModel.Marca_tarjeta.ContainValueString() ? xModel.Marca_tarjeta : null;

            switch (xModel.Metodo_operacion)
            {
                case CommonPago.MetodoOperacion.TARJETA:
                    sRequestPago.payment_request_data.terminal_operation_method = Enums.TerminalOPerationMethod.CARD;
                    break;

                case CommonPago.MetodoOperacion.QR:
                    sRequestPago.payment_request_data.terminal_operation_method = Enums.TerminalOPerationMethod.QR_CODE;
                    break;
            }

            sRequestPago.payment_request_data.qr_benefit_code = xModel.Admite_tarjeta_beneficio;
            sRequestPago.payment_request_data.trx_receipt_notes = xModel.Nota_impresion_ticket;
            sRequestPago.payment_request_data.card_holder_id = xModel.Dni_cliente.ContainValueString() ? xModel.Dni_cliente : null;

            EnviarSolicitudPagoService<RequestPago, RequestPago>("/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPago);

            //RequestPago sReturn = await _client.PostAsync<RequestPago, RequestPago>("/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPago);

            //var sReturn = _client.PostAsync<RequestPago, RequestPago>("/payments/cuit_cuil?" + xModel.Cuit_cuil, sHttpContent).Result;
        }

        public void RenovarToken()
        {
            //API METODO PARA RENOVAR TOKEN
            GenerarToken<TokenResponse>("", _configuracion.Sub_end_point_authorization, "grant_type=client_credentials");
        }

        //public async Task GenerarToken()
        //{
        //    try
        //    {
        //        _client.ActualizarAuthorization(_configuracion.Key, ClientApiEntity.TipeAuthorization.Basic);
        //        TokenResponse sReturn = await _client.GetAsync<TokenResponse>("", _configuracion.Sub_end_point_authorization, "grant_type=client_credentials");

        //        Token = sReturn.access_token != null ? sReturn.access_token : "";
        //    }
        //    catch { }

        //    //string sAs = "";

        //    //using (HttpClient client = new HttpClient())
        //    //{
        //    //    client.BaseAddress = new Uri("https://api-sandbox.prismamediosdepago.com");
        //    //    client.DefaultRequestHeaders.Accept.Clear();
        //    //    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //    //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "MkpPaHkwbE9SSmRLNUxRaWh6TW5KdVZyQ3dsb0dZWjc6Rnh3NEdRTmx6b3lCUlgzUQ==");
        //    //    client.Timeout = TimeSpan.FromMinutes(10);
        //    //    //Al infinito
        //    //    client.Timeout = new TimeSpan(0, 0, 0, 0, -1);
        //    //    HttpResponseMessage response = await client.GetAsync("https://api-sandbox.prismamediosdepago.com/v1/oauth/accesstoken?grant_type=client_credentials");

        //    //    if (response.IsSuccessStatusCode)
        //    //    {
        //    //        TokenResponse sReturnDirecto = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

        //    //        string algo = "";
        //    //        //// Si se va a recibir un JSON bastante grande
        //    //        //// https://stackoverflow.com/questions/56398881/how-to-read-json-payload-of-a-large-response

        //    //        //var stream = await response.Content.ReadAsStreamAsync();

        //    //        //using (StreamReader sr = new StreamReader(stream))
        //    //        //using (JsonReader reader = new JsonTextReader(sr))
        //    //        //{
        //    //        //    JsonSerializer serializer = new JsonSerializer();
        //    //        //    TokenResponse sReturn = serializer.Deserialize<TokenResponse>(reader);
        //    //        //}

        //    //        //var jsonPuro = await response.Content.ReadAsStringAsync();
        //    //        //var jsonDesarializado = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonPuro);


        //    //        ////lista = JsonConvert
        //    //        ////        .DeserializeObject<List<PeriodoNominaBE>>(jsonPuro.ToString()
        //    //        ////        , new JsonSerializerSettings()
        //    //        ////        {
        //    //        ////            MissingMemberHandling =
        //    //        ////                MissingMemberHandling.Ignore
        //    //        ////        });

        //    //    }
        //    //}
        //}
    }
}
