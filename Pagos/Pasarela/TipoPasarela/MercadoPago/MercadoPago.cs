using Core.Api;
using Core.Business;
using Pagos.Pasarela.Eventos;
using Pagos.Pasarela.MercadoPagoModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pagos.Pasarela
{
    public class MercadoPago : PagoBase, IPago, IPagoHandler, IAutenticacion
    {
        private RequestPagoQr _requestPagoQr;
        private RequestPagoPoint _requestPagoPoint;

        public MercadoPago(Configuracion xConfiguracion)
        {
            _configuracion = xConfiguracion;
            SetClient();

            //SOLO PARA AMBIENTE DE SANDBOX
            if (_configuracion.Entorno == CommonPago.TipoEntorno.PRUEBA && _configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
                _client.AgregarHeader("x-test-scope", "sandbox");

            if (xConfiguracion.Token.ContainValueString())
                Token = xConfiguracion.Token;

            OnRespuestaBase += RespuestaBase;
        }

        public event RespuestaExternaHandler OnRespuesta;

        public void EnviarCancelacionPago()
        {
            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR) 
            {
                EnviarCancelacionPagoQr(_requestPagoQr);
            }

            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
            {
                //EnviarCancelacionPagoPoint(_requestPagoPoint);
            }
        }

        public void EnviarCancelacionReversion()
        {
            throw new NotImplementedException();
        }

        public void BuscarCajas()
        {
            //API METODO PARA RENOVAR TOKEN
            //GenerarToken<object>(ClientApiEntity.TipeAuthorization.Bearer, "", "/pos", "external_id=11963556&external_store_id=undefined&store_id=undefined&category=undefined", new object(), CommonPago.TipoRespuestaEvento.TOKEN);
            GenerarToken<object>(ClientApiEntity.TipeAuthorization.Bearer, "", "/pos", "access_token=APP_USR-1289759863042338-080412-013d9c7a88fd03f47e975307a774dc7d-62251622", new object(), CommonPago.TipoRespuestaEvento.TOKEN);

        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            //Dispositivos();
            //RenovarToken();
            //BuscarCajas();

            if (SolicitudEnProceso)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "Ya hay una solicitud en proceso", true));
                return;
            }

            SolicitudEnProceso = true;

            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR)
            {
                RequestPagoQrMin sRequestPago = new RequestPagoQrMin();
                sRequestPago.external_reference = xModel.Referencia;
                sRequestPago.total_amount = xModel.Importe;
                sRequestPago.title = xModel.Titulo;
                sRequestPago.description = xModel.Texto_terminal;
                sRequestPago.items = new List<Item>();

                if (xModel.Items == null)
                    xModel.Items = new List<Items>();

                foreach (Items sItem in xModel.Items)
                {
                    sRequestPago.items.Add(new Item() { sku_number = sItem.Codigo, category = sItem.Categoria, title = sItem.Titulo, description = sItem.Descripcion, unit_price = sItem.Precio_unitario, quantity = sItem.Cantidad, unit_measure = "unit", total_amount = sItem.Total });
                }

                string sEndPoint = _configuracion.User_id + "/stores/" + _configuracion.Sucursal + "/pos/" + _configuracion.Pos + "/orders";

                //EnviarSolicitudService<RequestPagoQr, ResponsePagoQr>(CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, sEndPoint, "", sRequestPago);
                ActualizarSolicitudService<RequestPagoQrMin, ResponsePagoQr>(CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, sEndPoint, "", sRequestPago, xModel);
            }
            else
            {
                RequestPagoPoint sRequestPago = new RequestPagoPoint();
                sRequestPago.additional_info = new AdicionalInfo();
                sRequestPago.additional_info.ticket_number = xModel.Referencia;

                sRequestPago.deviceId = "";

                if (xModel.Lista_terminales == null)
                    xModel.Lista_terminales = new List<string>();

                foreach (string sTerminalItem in xModel.Lista_terminales)
                {
                    sRequestPago.deviceId = sTerminalItem;

                    break;
                }

                sRequestPago.amount = Convert.ToInt32(xModel.Importe);

                sRequestPago.Parametro_original = xModel;

                string sEndPoint = "/devices/" + sRequestPago.deviceId + "/payment-intents";

                EnviarSolicitudService<RequestPagoPoint, ResponsePagoPoint>(CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, sEndPoint, "", sRequestPago);
            }
        }

        public void EnviarSolicitudReversion(SolicitudReversion xModel)
        {
            throw new NotImplementedException();
        }

        public void ReiniciarConsultaEstadoPago()
        {
            if (ConsultaEstadoEnProceso)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "El proceso de consulta anterior esta en ejecucion", true));
                return;
            }

            ConsultaEstadoEnProceso = true;

            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR)
            {
                //EnviarConsultaEstadoPagoQr(_requestPagoQr);
            }

            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
            {
                EnviarConsultaEstadoPagoPoint(_requestPagoPoint);
            }
        }

        public void ReiniciarConsultaEstadoReversion()
        {
            throw new NotImplementedException();
        }

        public void RenovarToken(object xParametroOriginal, CommonPago.TipoRespuestaEvento xTipoOrigen)
        {
            TokenRequest sTokenRequest = new TokenRequest();
            sTokenRequest.client_id = _configuracion.Client_id;
            sTokenRequest.client_secret = _configuracion.Client_secret;
            sTokenRequest.code = _configuracion.Code;
            sTokenRequest.grant_type = "client_credentials";

            //API METODO PARA RENOVAR TOKEN
            GenerarToken<TokenRequest, TokenResponse>(ClientApiEntity.TipeAuthorization.Bearer, "", _configuracion.Sub_end_point_authorization, sTokenRequest, xParametroOriginal, xTipoOrigen);
        }

        public void RenovarToken()
        {
            TokenRequest sTokenRequest = new TokenRequest();
            sTokenRequest.client_id = _configuracion.Client_id;
            sTokenRequest.client_secret = _configuracion.Client_secret;
            sTokenRequest.code = _configuracion.Code;
            sTokenRequest.grant_type = "client_credentials";

            //API METODO PARA RENOVAR TOKEN
            GenerarToken<TokenRequest, TokenResponse>(ClientApiEntity.TipeAuthorization.Bearer, "", _configuracion.Sub_end_point_authorization, sTokenRequest);
        }

        public void Dispositivos()
        {
            //API METODO PARA RENOVAR TOKEN
            GenerarToken<object>(ClientApiEntity.TipeAuthorization.Bearer, "", "/devices", "", new object(), CommonPago.TipoRespuestaEvento.TOKEN);
             
        }

        private void EnviarConsultaEstadoPagoQr(SolicitudPago xModel)
        {
            if (_solicitudEliminada)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Solicitud cancelada"));
                return;
            }

            int sSegundosPersistencia = _configuracion.Tiempo_segundos_persistencias == 0 ? _tiempoSegundosPersistenciaDefault : _configuracion.Tiempo_segundos_persistencias;

            if (DateTime.Compare(xModel.Inicio_persistencia.AddSeconds(sSegundosPersistencia), DateTime.UtcNow.AddHours(-3)) < 0)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_401, "Error: Tiempo de persistencia agotado", true));
                ConsultaEstadoEnProceso = false;

                return;
            }

            //string sEndPoint = "/" + _configuracion.User_id + "/pos/" + _configuracion.Pos + "/orders";
            string sEndPoint = "/merchant_orders";
            string sParam = "access_token=" + Token + "&external_reference=" + xModel.Referencia;

            EnviarConsultaEstadoService<SolicitudPago, ResponsePagoQr>(CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "", sEndPoint, sParam, xModel);
        }

        private void EnviarConsultaEstadoPagoPoint(RequestPagoPoint xModel)
        {
            int sSegundosPersistencia = _configuracion.Tiempo_segundos_persistencias == 0 ? _tiempoSegundosPersistenciaDefault : _configuracion.Tiempo_segundos_persistencias;

            if (DateTime.Compare(xModel.Inicio_persistencia.AddSeconds(sSegundosPersistencia), DateTime.UtcNow.AddHours(-3)) < 0)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_401, "Error: Tiempo de persistencia agotado", true));
                ConsultaEstadoEnProceso = false;

                return;
            }

            string sEndPoint = "/payment-intents/" + xModel.id;

            EnviarConsultaEstadoService<RequestPagoPoint, ResponsePagoPoint>(CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, sEndPoint, "", xModel);
        }

        private void EnviarCancelacionPagoQr(RequestPagoQr xModel)
        {
            string sEndPoint = "/" + _configuracion.User_id + "/pos/" + _configuracion.Pos + "/orders";

            EnviarCancelacionService<RequestPagoQr, ResponsePagoQr>(CommonPago.TipoRespuestaEvento.CANCELACION_PAGO, sEndPoint, "", xModel);
        }

        private void EnviarCancelacionPagoPoint(RequestPagoPoint xModel)
        {
            string sEndPoint = "/devices/" + xModel.deviceId + "/payment-intents/" + xModel.id;

            EnviarCancelacionService<RequestPagoPoint, ResponsePagoPoint>(CommonPago.TipoRespuestaEvento.CANCELACION_PAGO, sEndPoint, "", xModel);
        }

        private void Errores<T>(string xError, CommonPago.TipoRespuestaEvento xTipo, T xParametroOriginal, int xNroIntento)
        {
            if (xError.ContainValueString())
            {
                if (xError.Equals("401"))
                {
                    if (xNroIntento < 3)
                    {
                        RenovarToken(xParametroOriginal, xTipo);
                    }
                    else
                    {
                        OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_401, "Error de validacion de Token", true));
                        SolicitudEnProceso = false;
                        ConsultaEstadoEnProceso = false;
                    }
                }

                if (xError.Equals("400"))
                {
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_400, "Error de formato", true));
                }

                if (xError.Equals("404"))
                {
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_404, "Error: No encontrado", true));
                }

                if (xError.Equals("500"))
                {
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_500, "Error interno", true));
                }
            }
        }

        public void RespuestaBase(object sender, RespuestaEventArgs e)
        {
            switch (e.TipoRespuesta)
            {
                case CommonPago.TipoRespuestaEvento.TOKEN:

                    TokenResponse sRespuestaToken = (TokenResponse)e.Respuesta;
                    Token = sRespuestaToken.access_token != null ? sRespuestaToken.access_token : "";

                    switch (e.TipoRespuestaOrigen)
                    {
                        case CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO:
                            SolicitudEnProceso = false;

                            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR)
                            {
                                _requestPagoQr = (RequestPagoQr)e.ParametroOriginal;

                                EnviarSolicitudPago((SolicitudPago)e.ParametroOriginal);
                            }
                            
                            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
                            {
                                _requestPagoPoint = (RequestPagoPoint)e.ParametroOriginal;

                                EnviarSolicitudPago(((RequestPagoPoint)e.ParametroOriginal).Parametro_original);
                            }

                            break;

                        case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO:
                            ConsultaEstadoEnProceso = false;

                            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR)
                            {
                                EnviarConsultaEstadoPagoQr((SolicitudPago)e.ParametroOriginal);
                            }

                            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
                            {
                                EnviarConsultaEstadoPagoPoint((RequestPagoPoint)e.ParametroOriginal);
                            }

                            break;

                        case CommonPago.TipoRespuestaEvento.CANCELACION_PAGO:
                            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR)
                            {
                                EnviarCancelacionPagoQr((RequestPagoQr)e.ParametroOriginal);
                            }

                            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
                            {
                                EnviarCancelacionPagoPoint((RequestPagoPoint)e.ParametroOriginal);
                            }

                            break;

                        case CommonPago.TipoRespuestaEvento.SOLICITUD_REVERSION:
                            SolicitudEnProceso = false;
                            //EnviarSolicitudReversion((SolicitudReversion)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_REVERSION:
                            ConsultaEstadoEnProceso = false;
                            //EnviarConsultaEstadoReversion((RequestReversion)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.CANCELACION_REVERSION:
                            //EnviarCancelacionReversion((RequestReversion)e.ParametroOriginal);
                            break;
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO:

                    if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR)
                    {                        
                        ResponsePagoQr sRespuestaSolicitudPago = (ResponsePagoQr)e.Respuesta;
                        SolicitudPago sParametroOriginalSolicitudPago = (SolicitudPago)e.ParametroOriginal;
                        sParametroOriginalSolicitudPago.Nro_intento_generacion_token++;

                        //if (sRespuestaSolicitudPago.status.ContainValueString())
                        //{
                        //    Errores<SolicitudPago>(sRespuestaSolicitudPago.status, CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, sParametroOriginalSolicitudPago, 0);
                        //}
                        //else
                        //{
                            OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.SOLICITUD_ENVIADA, "Solicitud enviada con exito", ""));

                            SolicitudEnProceso = false;

                            //sParametroOriginalSolicitudPago.Nro_persistencia = 1;
                            sParametroOriginalSolicitudPago.Inicio_persistencia = DateTime.UtcNow.AddHours(-3);

                            ConsultaEstadoEnProceso = true;

                            EnviarConsultaEstadoPagoQr(sParametroOriginalSolicitudPago);
                        //}
                    }

                    if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
                    {
                        ResponsePagoPoint sRespuestaSolicitudPago = (ResponsePagoPoint)e.Respuesta;
                        RequestPagoPoint sParametroOriginalSolicitudPago = (RequestPagoPoint)e.ParametroOriginal;
                        sParametroOriginalSolicitudPago.Parametro_original.Nro_intento_generacion_token++;

                        if (sRespuestaSolicitudPago.status.ContainValueString())
                        {
                            Errores<RequestPagoPoint>(sRespuestaSolicitudPago.status, CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, sParametroOriginalSolicitudPago, sParametroOriginalSolicitudPago.Parametro_original.Nro_intento_generacion_token);
                        }
                        else
                        {
                            OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.SOLICITUD_ENVIADA, "Solicitud enviada con exito", ""));

                            SolicitudEnProceso = false;

                            sParametroOriginalSolicitudPago.Nro_persistencia = 1;
                            sParametroOriginalSolicitudPago.Inicio_persistencia = DateTime.UtcNow.AddHours(-3);
                            sParametroOriginalSolicitudPago.id = sRespuestaSolicitudPago.id;

                            ConsultaEstadoEnProceso = true;

                            EnviarConsultaEstadoPagoPoint(sParametroOriginalSolicitudPago);
                        }
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO:

                    if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR)
                    {
                        ResponsePagoQr sRespuestaSolicitudPago = (ResponsePagoQr)e.Respuesta;
                        SolicitudPago sParametroOriginalSolicitudPago = (SolicitudPago)e.ParametroOriginal;
                        sParametroOriginalSolicitudPago.Nro_intento_generacion_token++;

                        //if (sRespuestaSolicitudPago.status.ContainValueString())
                        //{
                        //    Errores<SolicitudPago>(sRespuestaSolicitudPago.status, CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, sParametroOriginalSolicitudPago, 0);
                        //}
                        //else
                        //{

                        string sEstado = "";

                        if (sRespuestaSolicitudPago.elements == null || (sRespuestaSolicitudPago.elements != null && sRespuestaSolicitudPago.elements.Count() == 0))
                        {
                            sEstado = "Pendiente";
                        }
                        else 
                        {
                            sEstado = "Abierto";

                            if (sRespuestaSolicitudPago.elements.First().status.Equals("closed"))
                                sEstado = "Cerrado";
                        }

                        OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, sEstado));

                        if(!sEstado.Equals("Cerrado"))
                            EnviarConsultaEstadoPagoQr(sParametroOriginalSolicitudPago);
                        //}
                    }

                    if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
                    {
                        ResponsePagoPoint sRespuestaConsultaEstadoPago = (ResponsePagoPoint)e.Respuesta;
                        RequestPagoPoint sParametroOriginalConsultaEstadoPago = (RequestPagoPoint)e.ParametroOriginal;
                        sParametroOriginalConsultaEstadoPago.Parametro_original.Nro_intento_generacion_token++;

                        if (sRespuestaConsultaEstadoPago.status.ContainValueString())
                        {
                            Errores<RequestPagoPoint>(sRespuestaConsultaEstadoPago.status, CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, sParametroOriginalConsultaEstadoPago, sParametroOriginalConsultaEstadoPago.Parametro_original.Nro_intento_generacion_token);

                            ConsultaEstadoEnProceso = false;
                        }
                        else
                        {
                            OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, sRespuestaConsultaEstadoPago.state));

                            //VER LOS POSIBLES ESTADOS PARA VER CON CUALES TENGO QUE PERSISTIR

                            //EnviarConsultaEstadoPagoPoint(sParametroOriginalConsultaEstadoPago);                        
                        }
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CANCELACION_PAGO:
                    _solicitudEliminada = true;

                    if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR)
                    {
                        //ResponsePagoQr sRespuestaSolicitudPago = (ResponsePagoQr)e.Respuesta;
                        //SolicitudPago sParametroOriginalSolicitudPago = (SolicitudPago)e.ParametroOriginal;
                        //sParametroOriginalSolicitudPago.Nro_intento_generacion_token++;

                        OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Solicitud cancelada"));
                    }

                    if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
                    {
                        ResponsePagoPoint sRespuestaCancelacionPago = (ResponsePagoPoint)e.Respuesta;
                        RequestPagoPoint sParametroOriginalCancelacionPago = (RequestPagoPoint)e.ParametroOriginal;
                        sParametroOriginalCancelacionPago.Parametro_original.Nro_intento_generacion_token++;

                        if (sRespuestaCancelacionPago.status.ContainValueString())
                        {
                            Errores<RequestPagoPoint>(sRespuestaCancelacionPago.status, CommonPago.TipoRespuestaEvento.CANCELACION_PAGO, sParametroOriginalCancelacionPago, sParametroOriginalCancelacionPago.Parametro_original.Nro_intento_generacion_token);
                        }
                        else
                        {
                            OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, sRespuestaCancelacionPago.state));
                        }
                    }

                    break;
            }
        }

        public async static Task<HttpResponseMessage> CreateSucursal(SucursalPos xModel)
        {
            //ClientApiEntity sClient = new ClientApiEntity();
            //sClient = new ClientApiEntity(xModel.Configuracion.End_point, xModel.Configuracion.Sub_end_point);
            //sClient.ActualizarAuthorization(xModel.Configuracion.Token, ClientApiEntity.TipeAuthorization.Bearer);

            //string sEndPoint = "/user/" + xModel.Configuracion.User_id + "/stores";

            //string sNombre = "SUC" + Convert.ToInt32(xModel.Id).ToString().PadLeft(2, '0');

            //SucursalRequest sSucursalRequest = new SucursalRequest();
            //sSucursalRequest.external_id = sNombre;
            //sSucursalRequest.name = sNombre;
            //sSucursalRequest.location = new Location();
            //sSucursalRequest.location.street_number = "100";
            //sSucursalRequest.location.street_name = "test";
            //sSucursalRequest.location.state_name = "hsdafhas";
            //sSucursalRequest.location.city_name = "sfdgdfsg";
            //sSucursalRequest.location.state_name = "sdfhdfs";
            //sSucursalRequest.location.latitude = -32.8897322;
            //sSucursalRequest.location.longitude = -68.8443275;
            //sSucursalRequest.location.reference = "prueba";

            HttpResponseMessage sReturnh = new HttpResponseMessage();

            try
            {

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
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return response;

                //return await PagoBase.EnviarCreateSucursalService<SucursalRequest, SucursalResponse>(sClient, sEndPoint, "", sSucursalRequest);
            }
            catch
            {
                return sReturnh;
            }
        }

        public void CreatePos(SucursalPos xModel)
        {
            throw new NotImplementedException();
        }

        public void EnviarSolicitudDevolucion(SolicitudDevolucion xModel)
        {
            throw new NotImplementedException();
        }

        public void ReiniciarConsultaEstadoDevolucion()
        {
            throw new NotImplementedException();
        }

        public void EnviarCancelacionDevolucion()
        {
            throw new NotImplementedException();
        }
    }
}
