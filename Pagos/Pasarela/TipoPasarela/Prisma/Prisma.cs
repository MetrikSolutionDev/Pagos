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
    {   
        public Prisma(Configuracion xConfiguracion)
        {
            _configuracion = xConfiguracion;
            SetClient();

            OnRespuestaInt += Respuesta;
            //OnRespuestaGenericInt += Respuesta;
        }

        public event RespuestaPagoHandler OnRespuestaPago;
        public event RespuestaHandler OnRespuesta;

        public void Respuesta(object sender, RespuestaEventArgs e)
        {
            switch (e.TipoRespuesta)
            {
                case Enums.TipoRespuestaEvento.TOKEN:

                    TokenResponse sRespuestaToken = (TokenResponse)e.Respuesta;
                    Token = sRespuestaToken.access_token != null ? sRespuestaToken.access_token : "";

                    switch (e.TipoRespuestaOrigen) 
                    {
                        case Enums.TipoRespuestaEvento.SOLICITUD_PAGO:
                            EnviarSolicitudPago((SolicitudPago)e.ParametroOriginal);
                            break;

                        case Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO:
                            EnviarConsultaEstadoPago((RequestPago)e.ParametroOriginal);
                            break;
                    }

                    //if (e.Delegate != null)
                    //    e.Delegate.Invoke(e.ParametroOriginal);

                    break;

                case Enums.TipoRespuestaEvento.SOLICITUD_PAGO:

                    RequestPago sRespuestaSolicitudPago = (RequestPago)e.Respuesta;

                    if (sRespuestaSolicitudPago.errors != null)
                    {
                        if (sRespuestaSolicitudPago.errors.Any(er => er.status == "401"))
                        {
                            RequestPago sParametroOriginal = (RequestPago)e.ParametroOriginal;
                            //EnviarSolicitudPago(sParametroOriginal.Parametro_original);

                            sParametroOriginal.Parametro_original.Nro_intento_generacion_token++;

                            if (sParametroOriginal.Parametro_original.Nro_intento_generacion_token < 3)
                            {
                                RenovarToken(sParametroOriginal.Parametro_original, Enums.TipoRespuestaEvento.SOLICITUD_PAGO);
                            }
                            else
                            {
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.SOLICITUD_PAGO, "Error de validacion de Token"));
                                sParametroOriginal.Parametro_original.Nro_intento_generacion_token = 0;
                                SolicitudPagoEnProceso = false;
                            }
                        }

                        if (sRespuestaSolicitudPago.errors.Any(er => er.status == "400"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.SOLICITUD_PAGO, "Error de formato"));
                        }

                        if (sRespuestaSolicitudPago.errors.Any(er => er.status == "404"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.SOLICITUD_PAGO, "Error: Pago no encontrado"));
                        }

                        if (sRespuestaSolicitudPago.errors.Any(er => er.status == "409"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.SOLICITUD_PAGO, "Error en retorno de formato de mensaje"));
                        }

                        if (sRespuestaSolicitudPago.errors.Any(er => er.status == "500"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.SOLICITUD_PAGO, "Error interno"));
                        }

                        if (sRespuestaSolicitudPago.errors.Any(er => er.status == "503"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.SOLICITUD_PAGO, "Error: Servicio no disponible"));
                        }
                    }
                    else 
                    {
                        OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.SOLICITUD_PAGO, "Solicitud enviada con exito"));

                        SolicitudPagoEnProceso = false;

                        sRespuestaSolicitudPago.Nro_persistencia = 1;
                        sRespuestaSolicitudPago.Inicio_persistencia = DateTime.UtcNow.AddHours(-3);

                        EnviarConsultaEstadoPago(sRespuestaSolicitudPago);
                    }

                    break;

                case Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO:
                    RequestPago sRespuestaConsultaEstadoPago = (RequestPago)e.Respuesta;

                    if (sRespuestaConsultaEstadoPago.errors != null)
                    {
                        if (sRespuestaConsultaEstadoPago.errors.Any(er => er.status == "401"))
                        {
                            RequestPago sParametroOriginal = (RequestPago)e.ParametroOriginal;

                            sParametroOriginal.Parametro_original.Nro_intento_generacion_token++;

                            if (sParametroOriginal.Parametro_original.Nro_intento_generacion_token < 3)
                            {
                                RenovarToken(sParametroOriginal, Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO);
                            }
                            else
                            {
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Error de validacion de Token"));
                                sParametroOriginal.Parametro_original.Nro_intento_generacion_token = 0;
                                SolicitudPagoEnProceso = false;
                            }
                        }

                        if (sRespuestaConsultaEstadoPago.errors.Any(er => er.status == "400"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Error de formato"));
                        }

                        if (sRespuestaConsultaEstadoPago.errors.Any(er => er.status == "404"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Error: Pago no encontrado"));
                        }

                        if (sRespuestaConsultaEstadoPago.errors.Any(er => er.status == "409"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Error en retorno de formato de mensaje"));
                        }

                        if (sRespuestaConsultaEstadoPago.errors.Any(er => er.status == "500"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Error interno"));
                        }

                        if (sRespuestaConsultaEstadoPago.errors.Any(er => er.status == "503"))
                        {
                            OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Error: Servicio no disponible"));
                        }
                    }
                    else
                    {
                        //OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Solicitud enviada con exito"));

                        Payment_data sPaymentDataReturn = sRespuestaConsultaEstadoPago.payment_data;

                        switch (sPaymentDataReturn.payment_status) 
                        {
                            case Enums.PaymentStatus.PAYMENT_REQUEST:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Solicitud recibida con exito"));

                                sRespuestaConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sRespuestaConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.PROCESSING_PAYMENT:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Procesando pago"));

                                sRespuestaConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sRespuestaConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.WAITING_CONFIRMATION:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Esperando confirmacion"));

                                sRespuestaConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sRespuestaConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.CONFIRM_REQUEST:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Solicitud de confirmacion"));

                                sRespuestaConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sRespuestaConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.UNDO_REQUEST:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Deshacer solicitud"));

                                break;

                            case Enums.PaymentStatus.PROCESSING_CONFIRMATION:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Procesando confirmacion"));

                                sRespuestaConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sRespuestaConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.PROCESSING_UNDO:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Deshacer procesamiento"));

                                break;

                            case Enums.PaymentStatus.CONFIRMED:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Pago confirmado"));

                                break;

                            case Enums.PaymentStatus.DECLINED:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Pago rechazado"));

                                break;

                            case Enums.PaymentStatus.UNDONE:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Pago deshecho"));

                                break;

                            case Enums.PaymentStatus.ERROR:
                                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Error durante el procesamiento del pago"));

                                break;
                        }
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

        private void EnviarConsultaEstadoPago(RequestPago xModel)
        {
            //PersistirConsultaPago(ConsultaEstadoPagoPersistente, xModel, ref OnRespuestaPago);

            //return true;

            int sSegundosPersistencia = _configuracion.Tiempo_segundos_persistencias_pago == 0 ? _tiempoSegundosPersistenciaDefault : _configuracion.Tiempo_segundos_persistencias_pago;

            if (DateTime.Compare(xModel.Inicio_persistencia.AddSeconds(sSegundosPersistencia), DateTime.UtcNow.AddHours(-3)) < 0)
            {
                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Error: Tiempo de persistencia agotado"));

                return;
            }

            Payment_data sPaymentData = xModel.payment_data;

            ConsultaEstadoPago sConsultaEstadoPago = new ConsultaEstadoPago();
            sConsultaEstadoPago.Pago_id = sPaymentData.payment_id;
            sConsultaEstadoPago.Referencia = xModel.payment_request_data.subnet_acquirer_id;
            sConsultaEstadoPago.Cuit_cuil = xModel.Parametro_original.Cuit_cuil;

            EnviarConsultaEstadoPagoService<RequestPago, RequestPago>("/payments/" + sConsultaEstadoPago.Pago_id, "subnet_acquirer_id=" + sConsultaEstadoPago.Referencia + "&cuit_cuil=" + sConsultaEstadoPago.Cuit_cuil, xModel);
        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            if (SolicitudPagoEnProceso)
            {
                OnRespuesta(this, new RespuestaEventArgs(Enums.TipoRespuestaEvento.SOLICITUD_PAGO, "Ya hay una solicitud en proceso"));
                return;
            }

            SolicitudPagoEnProceso = true;

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

            if (xModel.Lista_terminales == null)
                xModel.Lista_terminales = new List<string>();

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
            sRequestPago.Parametro_original = xModel;

            EnviarSolicitudPagoService<RequestPago, RequestPago>("/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPago);

            //RequestPago sReturn = await _client.PostAsync<RequestPago, RequestPago>("/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPago);

            //var sReturn = _client.PostAsync<RequestPago, RequestPago>("/payments/cuit_cuil?" + xModel.Cuit_cuil, sHttpContent).Result;
        }

        public void RenovarToken()
        {
            //API METODO PARA RENOVAR TOKEN
            GenerarToken<TokenResponse>("", _configuracion.Sub_end_point_authorization, "grant_type=client_credentials");
        }

        public void RenovarToken(object xParametroOriginal, Enums.TipoRespuestaEvento xTipoOrigen)
        {
            //API METODO PARA RENOVAR TOKEN
            GenerarToken<TokenResponse>("", _configuracion.Sub_end_point_authorization, "grant_type=client_credentials", xParametroOriginal, xTipoOrigen);
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
