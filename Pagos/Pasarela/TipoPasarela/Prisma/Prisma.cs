﻿using Core.Api;
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
    public class Prisma : PagoBase, IPago, IPagoHandler, IAutenticacion
    {
        private RequestPago _requestPago;
        private RequestReversion _requestReversion;

        public Prisma(Configuracion xConfiguracion)
        {
            _configuracion = xConfiguracion;
            SetClient();

            if (xConfiguracion.Token.ContainValueString())
                Token = xConfiguracion.Token;

            OnRespuestaBase += RespuestaBase;
        }

        public event RespuestaExternaHandler OnRespuesta;

        //public void OnRespuestaEvent(object sender, RespuestaExternaEventArgs e)
        //{
        //    RespuestaExternaHandler handler = OnRespuesta;
        //    if (handler != null)
        //    {
        //        handler(sender, e);
        //    }
        //}

        private void Errores<T>(List<Errors> xList, CommonPago.TipoRespuestaEvento xTipo, T xParametroOriginal, int xNroIntento)
        {
            if (xList != null)
            {
                if (xList.Any(er => er.status == "401"))
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

                if (xList.Any(er => er.status == "400"))
                {
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_400, "Error de formato", true));
                }

                if (xList.Any(er => er.status == "404"))
                {
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_404, "Error: Pago no encontrado", true));
                }

                if (xList.Any(er => er.status == "409"))
                {
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_409, "Error en retorno de formato de mensaje", true));
                }

                if (xList.Any(er => er.status == "500"))
                {
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_500, "Error interno", true));
                }

                if (xList.Any(er => er.status == "503"))
                {
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_503, "Error: Servicio no disponible", true));
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

                    if(Token.ContainValueString())
                        OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.TOKEN, "Token", Token));


                    switch (e.TipoRespuestaOrigen) 
                    {
                        case CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO:
                            SolicitudEnProceso = false;
                            EnviarSolicitudPago(((RequestPago)e.ParametroOriginal).Parametro_original);
                            break;

                        case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO:
                            ConsultaEstadoEnProceso = false;
                            EnviarConsultaEstadoPago((RequestPago)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.CANCELACION_PAGO:
                            EnviarCancelacionPago((RequestPago)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.SOLICITUD_REVERSION:
                            SolicitudEnProceso = false;
                            EnviarSolicitudReversion((SolicitudReversion)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_REVERSION:
                            ConsultaEstadoEnProceso = false;
                            EnviarConsultaEstadoReversion((RequestReversion)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.CANCELACION_REVERSION:
                            EnviarCancelacionReversion((RequestReversion)e.ParametroOriginal);
                            break;
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO:

                    RequestPago sRespuestaSolicitudPago = (RequestPago)e.Respuesta;
                    RequestPago sParametroOriginalSolicitudPago = (RequestPago)e.ParametroOriginal;
                    sParametroOriginalSolicitudPago.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaSolicitudPago.errors != null)
                    {
                        Errores<RequestPago>(sRespuestaSolicitudPago.errors, CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, sParametroOriginalSolicitudPago, sParametroOriginalSolicitudPago.Parametro_original.Nro_intento_generacion_token);
                    }
                    else 
                    {
                        ////TEMPORAL PRUEBA
                        //sRespuestaSolicitudPago = new RequestPago();
                        //sRespuestaSolicitudPago.payment_data = new Payment_data();
                        //sRespuestaSolicitudPago.payment_data.payment_id = "jsdfsdakbfjfjd";
                        //sRespuestaSolicitudPago.payment_data.payment_status = Enums.PaymentStatus.PAYMENT_REQUEST;
                        //sRespuestaSolicitudPago.payment_request_data = new Payment_request_data();
                        //sRespuestaSolicitudPago.payment_request_data.subnet_acquirer_id = "1";
                        //sRespuestaSolicitudPago.Parametro_original = new SolicitudPago();
                        //sRespuestaSolicitudPago.Parametro_original.Cuit_cuil = "20341465681";

                        OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.SOLICITUD_ENVIADA, "Solicitud enviada con exito", sRespuestaSolicitudPago.payment_data.payment_id));

                        SolicitudEnProceso = false;

                        sRespuestaSolicitudPago.Nro_persistencia = 1;
                        sRespuestaSolicitudPago.Inicio_persistencia = DateTime.UtcNow.AddHours(-3);

                        _requestPago = sRespuestaSolicitudPago;

                        ConsultaEstadoEnProceso = true;

                        EnviarConsultaEstadoPago(sRespuestaSolicitudPago);
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO:
                    RequestPago sRespuestaConsultaEstadoPago = (RequestPago)e.Respuesta;
                    RequestPago sParametroOriginalConsultaEstadoPago = (RequestPago)e.ParametroOriginal;
                    sParametroOriginalConsultaEstadoPago.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaConsultaEstadoPago.errors != null)
                    {
                        Errores<RequestPago>(sRespuestaConsultaEstadoPago.errors, CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, sParametroOriginalConsultaEstadoPago, sParametroOriginalConsultaEstadoPago.Parametro_original.Nro_intento_generacion_token);

                        ConsultaEstadoEnProceso = false;
                    }
                    else
                    {
                        //OnRespuesta(this, new RespuestaEventArgs(CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "Solicitud enviada con exito"));

                        ////TEMPORAL PRUEBA
                        //sRespuestaConsultaEstadoPago = new RequestPago();
                        //sRespuestaConsultaEstadoPago.payment_data = new Payment_data();
                        //sRespuestaConsultaEstadoPago.payment_data.payment_id = "jsdfsdakbfjfjd";
                        //sRespuestaConsultaEstadoPago.payment_data.payment_status = Enums.PaymentStatus.PAYMENT_REQUEST;
                      
                        Payment_data sPaymentDataReturn = sRespuestaConsultaEstadoPago.payment_data;

                        switch (sPaymentDataReturn.payment_status) 
                        {
                            case Enums.PaymentStatus.PAYMENT_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud recibida con exito"));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.PROCESSING_PAYMENT:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando pago"));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.WAITING_CONFIRMATION:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Esperando confirmacion"));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.CONFIRM_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud de confirmacion"));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.UNDO_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer solicitud"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.PROCESSING_CONFIRMATION:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando confirmacion"));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.PROCESSING_UNDO:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer procesamiento"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.CONFIRMED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Pago confirmado"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Pago rechazado"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.UNDONE:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Pago deshecho"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Error durante el procesamiento del pago"));
                                ConsultaEstadoEnProceso = false;

                                break;
                        }
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CANCELACION_PAGO:
                    RequestPago sRespuestaCancelacionPago = (RequestPago)e.Respuesta;
                    RequestPago sParametroOriginalCancelacionPago = (RequestPago)e.ParametroOriginal;
                    sParametroOriginalCancelacionPago.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaCancelacionPago.errors != null)
                    {
                        Errores<RequestPago>(sRespuestaCancelacionPago.errors, CommonPago.TipoRespuestaEvento.CANCELACION_PAGO, sParametroOriginalCancelacionPago, sParametroOriginalCancelacionPago.Parametro_original.Nro_intento_generacion_token);
                    }
                    else
                    {
                        Payment_data sPaymentDataReturn = sRespuestaCancelacionPago.payment_data;

                        switch (sPaymentDataReturn.payment_status)
                        {
                            case Enums.PaymentStatus.PAYMENT_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Solicitud recibida con exito"));

                                break;

                            case Enums.PaymentStatus.PROCESSING_PAYMENT:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Procesando pago"));

                                break;

                            case Enums.PaymentStatus.WAITING_CONFIRMATION:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Esperando confirmacion"));

                                break;

                            case Enums.PaymentStatus.CONFIRM_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Solicitud de confirmacion"));

                                break;

                            case Enums.PaymentStatus.UNDO_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Deshacer solicitud"));

                                break;

                            case Enums.PaymentStatus.PROCESSING_CONFIRMATION:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Procesando confirmacion"));

                                break;

                            case Enums.PaymentStatus.PROCESSING_UNDO:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Deshacer procesamiento"));

                                break;

                            case Enums.PaymentStatus.CONFIRMED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Pago confirmado"));

                                break;

                            case Enums.PaymentStatus.DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Pago rechazado"));

                                break;

                            case Enums.PaymentStatus.UNDONE:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Pago deshecho"));

                                break;

                            case Enums.PaymentStatus.ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "Error durante el procesamiento del pago"));

                                break;
                        }
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.SOLICITUD_REVERSION:

                    RequestReversion sRespuestaSolicitudReversion = (RequestReversion)e.Respuesta;
                    RequestReversion sParametroOriginalSolicitudReversion = (RequestReversion)e.ParametroOriginal;
                    sParametroOriginalSolicitudReversion.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaSolicitudReversion.errors != null)
                    {
                        Errores<RequestReversion>(sRespuestaSolicitudReversion.errors, CommonPago.TipoRespuestaEvento.SOLICITUD_REVERSION, sParametroOriginalSolicitudReversion, sParametroOriginalSolicitudReversion.Parametro_original.Nro_intento_generacion_token);
                    }
                    else
                    {
                        OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.SOLICITUD_ENVIADA, "Solicitud enviada con exito"));

                        SolicitudEnProceso = false;

                        sParametroOriginalSolicitudReversion.Nro_persistencia = 1;
                        sParametroOriginalSolicitudReversion.Inicio_persistencia = DateTime.UtcNow.AddHours(-3);

                        _requestReversion = sParametroOriginalSolicitudReversion;

                        ConsultaEstadoEnProceso = true;

                        EnviarConsultaEstadoReversion(sParametroOriginalSolicitudReversion);
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_REVERSION:
                    RequestReversion sRespuestaConsultaEstadoReversion = (RequestReversion)e.Respuesta;
                    RequestReversion sParametroOriginalConsultaEstadoReversion = (RequestReversion)e.ParametroOriginal;
                    sParametroOriginalConsultaEstadoReversion.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaConsultaEstadoReversion.errors != null)
                    {
                        Errores<RequestReversion>(sRespuestaConsultaEstadoReversion.errors, CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_REVERSION, sParametroOriginalConsultaEstadoReversion, sParametroOriginalConsultaEstadoReversion.Parametro_original.Nro_intento_generacion_token);

                        ConsultaEstadoEnProceso = false;
                    }
                    else
                    {
                        Reversal_data sReversalDataReturn = sRespuestaConsultaEstadoReversion.reversal_data;

                        switch (sReversalDataReturn.reversal_status)
                        {
                            case Enums.ReversalStatus.REVERSAL_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud recibida con exito"));

                                EnviarConsultaEstadoReversion(sParametroOriginalConsultaEstadoReversion);
                                break;

                            case Enums.ReversalStatus.PROCESSING_REVERSAL:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando devolucion"));

                                EnviarConsultaEstadoReversion(sParametroOriginalConsultaEstadoReversion);
                                break;

                            case Enums.ReversalStatus.REVERSED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Pago cancelado"));

                                break;

                            case Enums.ReversalStatus.UNDO_REVERSAL_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer solicitud"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.ReversalStatus.PROCESSING_UNDO_REVERSAL:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer procesamiento"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.ReversalStatus.REVERSAL_DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Devolucion rechazada"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.ReversalStatus.UNDO_REVERSAL_DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer revolucion rechazada"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.ReversalStatus.REVERSAL_ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Error durante el procesamiento de la devolucion"));
                                ConsultaEstadoEnProceso = false;

                                break;
                        }
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CANCELACION_REVERSION:
                    RequestReversion sRespuestaCancelacionReversion = (RequestReversion)e.Respuesta;
                    RequestReversion sParametroOriginalCancelacionReversion = (RequestReversion)e.ParametroOriginal;
                    sParametroOriginalCancelacionReversion.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaCancelacionReversion.errors != null)
                    {
                        Errores<RequestReversion>(sRespuestaCancelacionReversion.errors, CommonPago.TipoRespuestaEvento.CANCELACION_REVERSION, sParametroOriginalCancelacionReversion, sParametroOriginalCancelacionReversion.Parametro_original.Nro_intento_generacion_token);
                    }
                    else
                    {
                        Reversal_data sReversalDataReturn = sRespuestaCancelacionReversion.reversal_data;

                        switch (sReversalDataReturn.reversal_status)
                        {
                            case Enums.ReversalStatus.REVERSAL_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud recibida con exito"));

                                break;

                            case Enums.ReversalStatus.PROCESSING_REVERSAL:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando devolucion"));

                                break;

                            case Enums.ReversalStatus.REVERSED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Pago cancelado"));

                                break;

                            case Enums.ReversalStatus.UNDO_REVERSAL_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer solicitud"));
                                
                                break;

                            case Enums.ReversalStatus.PROCESSING_UNDO_REVERSAL:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer procesamiento"));
                                
                                break;

                            case Enums.ReversalStatus.REVERSAL_DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Devolucion rechazada"));
                                
                                break;

                            case Enums.ReversalStatus.UNDO_REVERSAL_DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer revolucion rechazada"));
                                
                                break;

                            case Enums.ReversalStatus.REVERSAL_ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Error durante el procesamiento de la devolucion"));
                                
                                break;
                        }
                    }

                    break;
            }
        }

        public RespuestaConsultaEstadoPago ConsultaEstadoPagoPersistente(ConsultaEstado xModel)
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

            int sSegundosPersistencia = _configuracion.Tiempo_segundos_persistencias == 0 ? _tiempoSegundosPersistenciaDefault : _configuracion.Tiempo_segundos_persistencias;

            if (DateTime.Compare(xModel.Inicio_persistencia.AddSeconds(sSegundosPersistencia), DateTime.UtcNow.AddHours(-3)) < 0)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_401, "Error: Tiempo de persistencia agotado", true));
                ConsultaEstadoEnProceso = false;

                return;
            }

            Payment_data sPaymentData = xModel.payment_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            sConsultaEstado.Pago_id = sPaymentData.payment_id;
            sConsultaEstado.Referencia = xModel.payment_request_data.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = xModel.Parametro_original.Cuit_cuil;

            EnviarConsultaEstadoService<RequestPago, RequestPago>(CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "/payments/" + sConsultaEstado.Pago_id, "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil, xModel);
        }

        public void ReiniciarConsultaEstadoPago()
        {
            if (ConsultaEstadoEnProceso)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "El proceso de consulta anterior esta en ejecucion", true));
                return;
            }

            ConsultaEstadoEnProceso = true;
            EnviarConsultaEstadoPago(_requestPago);
        }

        private void EnviarCancelacionPago(RequestPago xModel)
        {
            Payment_data sPaymentData = xModel.payment_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            sConsultaEstado.Pago_id = sPaymentData.payment_id;
            sConsultaEstado.Referencia = xModel.payment_request_data.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = xModel.Parametro_original.Cuit_cuil;

            EnviarCancelacionService<RequestPago, RequestPago>(CommonPago.TipoRespuestaEvento.CANCELACION_PAGO, "/payments/" + sConsultaEstado.Pago_id, "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil, xModel);
        }

        public void EnviarCancelacionPago() 
        {
            EnviarCancelacionPago(_requestPago);
        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            if (SolicitudEnProceso)
            {
                //OnRespuestaEvent(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "Ya hay una solicitud en proceso", true));
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "Ya hay una solicitud en proceso", true));
                return;
            }

            SolicitudEnProceso = true;

            RequestPago sRequestPago = new RequestPago();
            sRequestPago.payment_request_data = new Payment_request_data();

            switch (_configuracion.Entorno) 
            {
                case CommonPago.TipoEntorno.PRUEBA:
                    sRequestPago.payment_request_data.subnet_acquirer_id = "1";
                    break;

                case CommonPago.TipoEntorno.HOMOLOGACION:
                    sRequestPago.payment_request_data.subnet_acquirer_id = "9";
                    break;

                case CommonPago.TipoEntorno.PRODUCCION:
                    sRequestPago.payment_request_data.subnet_acquirer_id = "2";
                    break;

                default:
                    sRequestPago.payment_request_data.subnet_acquirer_id = "2";
                    break;
            }
                
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

            EnviarSolicitudService<RequestPago, RequestPago>(CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, "/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPago);

            //RequestPago sReturn = await _client.PostAsync<RequestPago, RequestPago>("/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPago);

            //var sReturn = _client.PostAsync<RequestPago, RequestPago>("/payments/cuit_cuil?" + xModel.Cuit_cuil, sHttpContent).Result;
        }

        public void RenovarToken()
        {
            //API METODO PARA RENOVAR TOKEN
            GenerarToken<TokenResponse>("", _configuracion.Sub_end_point_authorization, "grant_type=client_credentials");
        }

        public void RenovarToken(object xParametroOriginal, CommonPago.TipoRespuestaEvento xTipoOrigen)
        {
            //API METODO PARA RENOVAR TOKEN
            GenerarToken<TokenResponse>("", _configuracion.Sub_end_point_authorization, "grant_type=client_credentials", xParametroOriginal, xTipoOrigen);
        }

        public void EnviarSolicitudReversion(SolicitudReversion xModel)
        {
            if (SolicitudEnProceso)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "Ya hay una solicitud en proceso", true));
                return;
            }

            SolicitudEnProceso = true;

            RequestReversion sRequestReversion = new RequestReversion();
            sRequestReversion.reversal_request_data = new Reversal_request_data();
            sRequestReversion.reversal_request_data.subnet_acquirer_id = "1";
            sRequestReversion.reversal_request_data.payment_id = xModel.Pago_id;
            sRequestReversion.reversal_request_data.terminal_menu_text = xModel.Texto_terminal;
            
            switch (xModel.Copias_comprobante_pago)
            {
                case CommonPago.CopiasComprobantePago.NINGUNO:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.NONE;
                    break;

                case CommonPago.CopiasComprobantePago.AMBOS:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.BOTH;
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_CLIENTE:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.CUSTOMER_ONLY;
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_COMERCIANTE:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.MERCHANT_ONLY;
                    break;

            }

            sRequestReversion.reversal_request_data.terminals_list = new List<Terminal>();

            if (xModel.Lista_terminales == null)
                xModel.Lista_terminales = new List<string>();

            foreach (string sTerminalItem in xModel.Lista_terminales)
            {
                Terminal sTerminal = new Terminal();
                sTerminal.Termina_id = sTerminalItem;

                sRequestReversion.reversal_request_data.terminals_list.Add(sTerminal);
            }

            EnviarSolicitudService<RequestReversion, RequestReversion>(CommonPago.TipoRespuestaEvento.SOLICITUD_REVERSION, "/reversals", "cuit_cuil=" + xModel.Cuit_cuil, sRequestReversion);
        }

        private void EnviarConsultaEstadoReversion(RequestReversion xModel)
        {
            int sSegundosPersistencia = _configuracion.Tiempo_segundos_persistencias == 0 ? _tiempoSegundosPersistenciaDefault : _configuracion.Tiempo_segundos_persistencias;

            if (DateTime.Compare(xModel.Inicio_persistencia.AddSeconds(sSegundosPersistencia), DateTime.UtcNow.AddHours(-3)) < 0)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_401, "Error: Tiempo de persistencia agotado", true));
                ConsultaEstadoEnProceso = false;

                return;
            }

            Reversal_data sReversalData = xModel.reversal_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            sConsultaEstado.Pago_id = sReversalData.payment_id;
            sConsultaEstado.Referencia = xModel.reversal_request_data.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = xModel.Parametro_original.Cuit_cuil;

            EnviarConsultaEstadoService<RequestReversion, RequestReversion>(CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_REVERSION, "/reversals/" + sConsultaEstado.Pago_id, "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil, xModel);
        }

        private void EnviarCancelacionReversion(RequestReversion xModel)
        {
            Reversal_data sReversalData = xModel.reversal_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            sConsultaEstado.Pago_id = sReversalData.payment_id;
            sConsultaEstado.Referencia = xModel.reversal_request_data.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = xModel.Parametro_original.Cuit_cuil;

            EnviarCancelacionService<RequestReversion, RequestReversion>(CommonPago.TipoRespuestaEvento.CANCELACION_REVERSION, "/reversals/" + sConsultaEstado.Pago_id, "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil + "&Nota", xModel);
        }

        public void EnviarCancelacionReversion()
        {
            EnviarCancelacionReversion(_requestReversion);
        }

        public void ReiniciarConsultaEstadoReversion()
        {
            if (ConsultaEstadoEnProceso)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "El proceso de consulta anterior esta en ejecucion", true));
                return;
            }

            ConsultaEstadoEnProceso = true;
            EnviarConsultaEstadoReversion(_requestReversion);
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
