using Core.Business;
using Pagos.Pasarela.Eventos;
using Pagos.Pasarela.PrismaModel;
using Pagos.Pasarela.TipoPasarela.Prisma.Model.Cierre;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public class Prisma : PagoBase, IPago, IPagoHandler
    {
        private RequestPago _requestPago;
        private ResponseCierre _requestCierre;
        private RequestReversion _requestReversion;
        private ResponseDevolucion _requestDevolucion;

        public Prisma(Configuracion xConfiguracion)
        {
            if (xConfiguracion.Entorno == CommonPago.TipoEntorno.PRODUCCION)
            {
                //PRODUCCION
                xConfiguracion.End_point = "https://api.prismamediosdepago.com";
                xConfiguracion.Key = "OWE0ZmNiYTItNzAzYS00MzM2LWIyZjUtNTRhMjY1OGFjMGU3OmVmMDBlMjhjLWE1MjYtNDg4Yi1hM2ExLWY1OGEwOTVjMDVlYw ==";
            }

            if (xConfiguracion.Entorno == CommonPago.TipoEntorno.HOMOLOGACION)
            {
                //HOMOLOGACION
                xConfiguracion.End_point = "https://api-homo.prismamediosdepago.com";
                xConfiguracion.Key = "YTczNjhmZWYtMTM0ZS00ZGZlLWI0YzgtMDNmMjkxMjBkNWZlOmNhYmZjM2EzLTRhNWEtNGZiNi1iNjhlLTFjMDhiNjczZGY0Mw==";
            }

            xConfiguracion.Sub_end_point = "/v1/paystore_terminals/terminal_payments";
            xConfiguracion.Sub_end_point_authorization = "/v1/oauth/accesstoken";
            
            xConfiguracion.Tiempo_segundos_persistencias = 500;

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
                    if (xNroIntento < 10)
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
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_404, "Error: Registro no encontrado", true));
                }

                if (xList.Any(er => er.status == "409"))
                {
                    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_409, xList.Where(er => er.status == "409").First().title, true));
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
                            EnviarCancelacionPago((Cancelacion)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.SOLICITUD_REVERSION:
                            SolicitudEnProceso = false;
                            EnviarSolicitudReversion(((RequestReversion)e.ParametroOriginal).Parametro_original);
                            break;

                        case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_REVERSION:
                            ConsultaEstadoEnProceso = false;
                            EnviarConsultaEstadoReversion((RequestReversion)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.CANCELACION_REVERSION:
                            EnviarCancelacionReversion((Cancelacion)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.SOLICITUD_DEVOLUCION:
                            SolicitudEnProceso = false;
                            EnviarSolicitudDevolucion(((RequestDevolucion)e.ParametroOriginal).Parametro_original);
                            break;

                        case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_DEVOLUCION:
                            ConsultaEstadoEnProceso = false;
                            EnviarConsultaEstadoDevolucion((ResponseDevolucion)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.CANCELACION_DEVOLUCION:
                            EnviarCancelacionDevolucion((Cancelacion)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.SOLICITUD_CIERRE:
                            SolicitudEnProceso = false;
                            EnviarSolicitudCierre(((RequestCierre)e.ParametroOriginal).Parametro_original);
                            break;

                        case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_CIERRE:
                            ConsultaEstadoEnProceso = false;
                            EnviarConsultaEstadoCierre((ResponseCierre)e.ParametroOriginal);
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

                        sRespuestaSolicitudPago.Parametro_original = sParametroOriginalSolicitudPago.Parametro_original;

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
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud recibida con exito", false, true, true));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.PROCESSING_PAYMENT:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando pago", false, true, false));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.WAITING_CONFIRMATION:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Esperando confirmacion", false, true, false));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.CONFIRM_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud de confirmacion", false, true, false));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.UNDO_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer solicitud", false, false, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.PROCESSING_CONFIRMATION:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando confirmacion", false, true, false));

                                sParametroOriginalConsultaEstadoPago.Nro_persistencia++;

                                EnviarConsultaEstadoPago(sParametroOriginalConsultaEstadoPago);
                                break;

                            case Enums.PaymentStatus.PROCESSING_UNDO:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer procesamiento", false, false, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.CONFIRMED:

                                DatosRespuestaPago sDatosRespuestaPago = new DatosRespuestaPago();
                                sDatosRespuestaPago.Descripcion_tarjeta = "";
                                sDatosRespuestaPago.Tarjeta = "";

                                List<EquivalenciaTarjeta> sList = Pago.ReadEquivalencias();

                                if (sList != null && sList.Any(a => a.Codigo == sPaymentDataReturn.card_brand_product))
                                {
                                    EquivalenciaTarjeta sItem = sList.Where(a => a.Codigo == sPaymentDataReturn.card_brand_product).First();

                                    sDatosRespuestaPago.Descripcion_tarjeta = sItem.Descripcion;
                                    sDatosRespuestaPago.Tarjeta = sItem.Codigo;
                                }
                                else 
                                {
                                    if (sList != null && sList.Count() > 0)
                                    {
                                        EquivalenciaTarjeta sItem = sList.First();

                                        sDatosRespuestaPago.Descripcion_tarjeta = sItem.Descripcion;
                                        sDatosRespuestaPago.Tarjeta = sItem.Codigo;
                                    }
                                }
                                
                                sDatosRespuestaPago.Nro_cupon = sPaymentDataReturn.transaction_receipt.ToString();
                                sDatosRespuestaPago.Nro_lote = sPaymentDataReturn.batch_number.ToString();
                                sDatosRespuestaPago.Nro_terminal = sPaymentDataReturn.terminal_id;

                                switch (sPaymentDataReturn.payment_type)
                                {
                                    case "CREDIT":
                                        sDatosRespuestaPago.Tipo_tarjeta = "CREDITO";
                                        break;

                                    case "DEBIT":
                                        sDatosRespuestaPago.Tipo_tarjeta = "DEBITO";
                                        break;

                                    default:
                                        sDatosRespuestaPago.Tipo_tarjeta = "CREDITO";
                                        break;
                                }
                                
                                //sDatosRespuestaPago.Tipo_tarjeta = sPaymentDataReturn.payment_type;

                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Pago confirmado", sDatosRespuestaPago, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Pago rechazado", true, false, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.UNDONE:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Pago deshecho", false, false, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.PaymentStatus.ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Error durante el procesamiento del pago", true, false, false));
                                ConsultaEstadoEnProceso = false;

                                break;
                        }
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CANCELACION_PAGO:
                    RequestPago sRespuestaCancelacionPago = (RequestPago)e.Respuesta;
                    Cancelacion sParametroOriginalCancelacionPago = (Cancelacion)e.ParametroOriginal;
                    sParametroOriginalCancelacionPago.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaCancelacionPago.errors != null)
                    {
                        Errores<Cancelacion>(sRespuestaCancelacionPago.errors, CommonPago.TipoRespuestaEvento.CANCELACION_PAGO, sParametroOriginalCancelacionPago, sParametroOriginalCancelacionPago.Parametro_original.Nro_intento_generacion_token);
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

                        ConsultaEstadoEnProceso = true;

                        sRespuestaSolicitudReversion.Parametro_original = sParametroOriginalSolicitudReversion.Parametro_original;

                        _requestReversion = sRespuestaSolicitudReversion;

                        EnviarConsultaEstadoReversion(sRespuestaSolicitudReversion);
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
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud recibida con exito", false, true, true));

                                EnviarConsultaEstadoReversion(sParametroOriginalConsultaEstadoReversion);
                                break;

                            case Enums.ReversalStatus.PROCESSING_REVERSAL:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando reversion", false, true, false));

                                EnviarConsultaEstadoReversion(sParametroOriginalConsultaEstadoReversion);
                                break;

                            case Enums.ReversalStatus.REVERSED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Pago devuelto", false, true, false, true));

                                break;

                            case Enums.ReversalStatus.REVERSAL_UNDONE:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Reversion deshecha", false, true, false));

                                break;

                            case Enums.ReversalStatus.UNDO_REVERSAL_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer solicitud", false, true, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.ReversalStatus.PROCESSING_UNDO_REVERSAL:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer procesamiento", false, true, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.ReversalStatus.REVERSAL_DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Devolucion rechazada", false, true, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.ReversalStatus.UNDO_REVERSAL_DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer revolucion rechazada", false, true, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.ReversalStatus.REVERSAL_ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Error durante el procesamiento de la devolucion", false, true, false));
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
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud de reversion recibida con exito"));

                                break;

                            case Enums.ReversalStatus.PROCESSING_REVERSAL:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando reversion"));

                                break;

                            case Enums.ReversalStatus.REVERSED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Reversion cancelada"));

                                break;

                            case Enums.ReversalStatus.REVERSAL_UNDONE:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Reversion deshecha"));

                                break;

                            case Enums.ReversalStatus.UNDO_REVERSAL_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer solicitud"));
                                
                                break;

                            case Enums.ReversalStatus.PROCESSING_UNDO_REVERSAL:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer procesamiento"));
                                
                                break;

                            case Enums.ReversalStatus.REVERSAL_DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Reversion rechazada"));
                                
                                break;

                            case Enums.ReversalStatus.UNDO_REVERSAL_DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer reversion rechazada"));
                                
                                break;

                            case Enums.ReversalStatus.REVERSAL_ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Error durante el procesamiento de la reversion"));
                                
                                break;
                        }
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.SOLICITUD_DEVOLUCION:

                    ResponseDevolucion sRespuestaSolicitudDevolucion = (ResponseDevolucion)e.Respuesta;
                    RequestDevolucion sParametroOriginalSolicitudDevolucion = (RequestDevolucion)e.ParametroOriginal;
                    sParametroOriginalSolicitudDevolucion.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaSolicitudDevolucion.errors != null)
                    {
                        Errores<RequestDevolucion>(sRespuestaSolicitudDevolucion.errors, CommonPago.TipoRespuestaEvento.SOLICITUD_DEVOLUCION, sParametroOriginalSolicitudDevolucion, sParametroOriginalSolicitudDevolucion.Parametro_original.Nro_intento_generacion_token);
                    }
                    else
                    {
                        OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.SOLICITUD_ENVIADA, "Solicitud enviada con exito", sRespuestaSolicitudDevolucion.refund_data.refund_id));

                        SolicitudEnProceso = false;

                        sParametroOriginalSolicitudDevolucion.Nro_persistencia = 1;
                        sParametroOriginalSolicitudDevolucion.Inicio_persistencia = DateTime.UtcNow.AddHours(-3);

                        ConsultaEstadoEnProceso = true;

                        sRespuestaSolicitudDevolucion.Parametro_original = sParametroOriginalSolicitudDevolucion.Parametro_original;
                        sRespuestaSolicitudDevolucion.refund_request = new RequestDevolucion();
                        sRespuestaSolicitudDevolucion.refund_request.subnet_acquirer_id = sParametroOriginalSolicitudDevolucion.subnet_acquirer_id;

                        _requestDevolucion = sRespuestaSolicitudDevolucion;

                        EnviarConsultaEstadoDevolucion(sRespuestaSolicitudDevolucion);
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_DEVOLUCION:
                    ResponseConsultaDevolucion sRespuestaConsultaEstadoDevolucion = (ResponseConsultaDevolucion)e.Respuesta;
                    ResponseDevolucion sParametroOriginalConsultaEstadoDevolucion = (ResponseDevolucion)e.ParametroOriginal;
                    sParametroOriginalConsultaEstadoDevolucion.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaConsultaEstadoDevolucion.errors != null)
                    {
                        Errores<ResponseDevolucion>(sRespuestaConsultaEstadoDevolucion.errors, CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_DEVOLUCION, sParametroOriginalConsultaEstadoDevolucion, sParametroOriginalConsultaEstadoDevolucion.Parametro_original.Nro_intento_generacion_token);

                        ConsultaEstadoEnProceso = false;
                    }
                    else
                    {
                        Refund_data sRefundDataReturn = sRespuestaConsultaEstadoDevolucion.refund_data;

                        switch (sRefundDataReturn.refund_status)
                        {
                            case Enums.RefundStatus.REFUND_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud recibida con exito", false, true, true));

                                EnviarConsultaEstadoDevolucion(sParametroOriginalConsultaEstadoDevolucion);
                                break;

                            case Enums.RefundStatus.PROCESSING_REFUND:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando devolucion", false, true, false));

                                EnviarConsultaEstadoDevolucion(sParametroOriginalConsultaEstadoDevolucion);
                                break;

                            case Enums.RefundStatus.CONFIRMED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Devolucion realizada", false, true, false, true));

                                break;

                            case Enums.RefundStatus.UNDO_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer solicitud", false, true, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.RefundStatus.DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Devolucion rechazada", false, true, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.RefundStatus.UNDONE:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Devolucion Cancelada", false, true, false));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.RefundStatus.ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Error durante el procesamiento de la devolucion", false, true, false));
                                ConsultaEstadoEnProceso = false;

                                break;
                        }
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CANCELACION_DEVOLUCION:
                    ResponseConsultaDevolucion sRespuestaCancelacionDevolucion = (ResponseConsultaDevolucion)e.Respuesta;
                    ResponseDevolucion sParametroOriginalCancelacionDevolucion = (ResponseDevolucion)e.ParametroOriginal;
                    sParametroOriginalCancelacionDevolucion.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaCancelacionDevolucion.errors != null)
                    {
                        Errores<ResponseDevolucion>(sRespuestaCancelacionDevolucion.errors, CommonPago.TipoRespuestaEvento.CANCELACION_DEVOLUCION, sParametroOriginalCancelacionDevolucion, sParametroOriginalCancelacionDevolucion.Parametro_original.Nro_intento_generacion_token);
                    }
                    else
                    {
                        Refund_data sRefundDataReturn = sRespuestaCancelacionDevolucion.refund_data;

                        switch (sRefundDataReturn.refund_status)
                        {
                            case Enums.RefundStatus.REFUND_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud recibida con exito"));

                                break;

                            case Enums.RefundStatus.PROCESSING_REFUND:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando devolucion"));

                                break;

                            case Enums.RefundStatus.CONFIRMED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Devolucion realizada"));

                                break;

                            case Enums.RefundStatus.UNDO_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Deshacer solicitud"));
                                
                                break;

                            case Enums.RefundStatus.DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Devolucion rechazada"));
                                
                                break;

                            case Enums.RefundStatus.UNDONE:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Devolucion Cancelada"));
                                
                                break;

                            case Enums.RefundStatus.ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Error durante el procesamiento de la devolucion"));
                                
                                break;
                        }
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.SOLICITUD_CIERRE:

                    ResponseCierre sRespuestaSolicitudCierre = (ResponseCierre)e.Respuesta;
                    RequestCierre sParametroOriginalSolicitudCierre = (RequestCierre)e.ParametroOriginal;
                    sParametroOriginalSolicitudCierre.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaSolicitudCierre.errors != null)
                    {
                        Errores<RequestCierre>(sRespuestaSolicitudCierre.errors, CommonPago.TipoRespuestaEvento.SOLICITUD_CIERRE, sParametroOriginalSolicitudCierre, sParametroOriginalSolicitudCierre.Parametro_original.Nro_intento_generacion_token);
                    }
                    else
                    {  
                        OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.SOLICITUD_ENVIADA, "Solicitud enviada con exito", sRespuestaSolicitudCierre.settlements_data.settlement_id));

                        SolicitudEnProceso = false;

                        sRespuestaSolicitudCierre.Nro_persistencia = 1;
                        sRespuestaSolicitudCierre.Inicio_persistencia = DateTime.UtcNow.AddHours(-3);

                        _requestCierre = sRespuestaSolicitudCierre;

                        ConsultaEstadoEnProceso = true;

                        sRespuestaSolicitudCierre.Parametro_original = sParametroOriginalSolicitudCierre.Parametro_original;

                        EnviarConsultaEstadoCierre(sRespuestaSolicitudCierre);
                    }

                    break;

                case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_CIERRE:
                    ResponseCierre sRespuestaConsultaEstadoCierre = (ResponseCierre)e.Respuesta;
                    ResponseCierre sParametroOriginalConsultaEstadoCierre = (ResponseCierre)e.ParametroOriginal;
                    sParametroOriginalConsultaEstadoCierre.Parametro_original.Nro_intento_generacion_token++;

                    if (sRespuestaConsultaEstadoCierre.errors != null)
                    {
                        Errores<ResponseCierre>(sRespuestaConsultaEstadoCierre.errors, CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, sParametroOriginalConsultaEstadoCierre, sParametroOriginalConsultaEstadoCierre.Parametro_original.Nro_intento_generacion_token);

                        ConsultaEstadoEnProceso = false;
                    }
                    else
                    {
                        Settlements_data sSettlementsDataReturn = sRespuestaConsultaEstadoCierre.settlements_data;

                        switch (sSettlementsDataReturn.settlement_status)
                        {
                            case Enums.SettlemnetsStatus.SETTLEMENT_REQUEST:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Solicitud recibida con exito", false, false, true));

                                sParametroOriginalConsultaEstadoCierre.Nro_persistencia++;

                                EnviarConsultaEstadoCierre(sParametroOriginalConsultaEstadoCierre);
                                break;

                            case Enums.SettlemnetsStatus.PROCESSING_SETTLEMENT:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Procesando cierre"));

                                sParametroOriginalConsultaEstadoCierre.Nro_persistencia++;

                                EnviarConsultaEstadoCierre(sParametroOriginalConsultaEstadoCierre);
                                break;

                            case Enums.SettlemnetsStatus.CONFIRMED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Cierre confirmado", false, false, false, true));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.SettlemnetsStatus.DECLINED:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Cierre rechazado"));
                                ConsultaEstadoEnProceso = false;

                                break;

                            case Enums.SettlemnetsStatus.ERROR:
                                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ESTADO_PAGO, "Error durante el procesamiento del cierre"));
                                ConsultaEstadoEnProceso = false;

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
            Thread.Sleep(500);

            //PersistirConsultaPago(ConsultaEstadoPagoPersistente, xModel, ref OnRespuestaPago);

            //return true;

            int sSegundosPersistencia = _configuracion.Tiempo_segundos_persistencias == 0 ? _tiempoSegundosPersistenciaDefault : _configuracion.Tiempo_segundos_persistencias;

            //if (DateTime.Compare(xModel.Inicio_persistencia.AddSeconds(sSegundosPersistencia), DateTime.UtcNow.AddHours(-3)) < 0)
            //{
            //    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_401, "Error: Tiempo de persistencia agotado", true));
            //    ConsultaEstadoEnProceso = false;

            //    return;
            //}

            Payment_data sPaymentData = xModel.payment_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            sConsultaEstado.Pago_id = sPaymentData.payment_id;
            sConsultaEstado.Referencia = xModel.payment_request_data.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = xModel.Parametro_original.Cuit_cuil;

            EnviarConsultaEstadoService<RequestPago, RequestPago>(CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO, "/v1/paystore_terminals/terminal_payments", "/payments/" + sConsultaEstado.Pago_id, "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil, xModel);
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

        public void EnviarCancelacionPago(Cancelacion xModel)
        {
            RequestPago sModel = new RequestPago();
            sModel.payment_data = new Payment_data();
            sModel.payment_data.payment_id = xModel.Pago_id;
            sModel.payment_request_data = new Payment_request_data();

            switch (xModel.Entorno)
            {
                case CommonPago.TipoEntorno.PRUEBA:
                    sModel.payment_request_data.subnet_acquirer_id = "1";
                    break;

                case CommonPago.TipoEntorno.HOMOLOGACION:
                    sModel.payment_request_data.subnet_acquirer_id = "9";
                    break;

                case CommonPago.TipoEntorno.PRODUCCION:
                    sModel.payment_request_data.subnet_acquirer_id = "2";
                    break;

                default:
                    sModel.payment_request_data.subnet_acquirer_id = "2";
                    break;
            }

            sModel.Parametro_original = new SolicitudPago();
            sModel.Parametro_original.Cuit_cuil = xModel.Cuit_cuil;

            Payment_data sPaymentData = sModel.payment_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            sConsultaEstado.Pago_id = sPaymentData.payment_id;
            sConsultaEstado.Referencia = sModel.payment_request_data.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = sModel.Parametro_original.Cuit_cuil;

            xModel.Parametro_original = xModel;

            if (sPaymentData.payment_id.ContainValueString())
            {
                EnviarCancelacionPutService<Cancelacion, RequestPago>(CommonPago.TipoRespuestaEvento.CANCELACION_PAGO, "/v1/paystore_terminals/terminal_payments", "/payments/" + sConsultaEstado.Pago_id + "/cancellations", "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil, xModel);
            }
            else 
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "No hay pago para cancelar"));
            }
        }

        public void EnviarCancelacionPago() 
        {
            Cancelacion sCancelacionPago = new Cancelacion();
            sCancelacionPago.Cuit_cuil = _requestPago.Parametro_original.Cuit_cuil;

            switch (_requestPago.payment_request_data.subnet_acquirer_id)
            {
                case "1":
                    sCancelacionPago.Entorno = CommonPago.TipoEntorno.PRUEBA;
                    break;

                case "9":
                    sCancelacionPago.Entorno = CommonPago.TipoEntorno.HOMOLOGACION;
                    break;

                case "2":
                    sCancelacionPago.Entorno = CommonPago.TipoEntorno.PRODUCCION;
                    break;

                default:
                    sCancelacionPago.Entorno = CommonPago.TipoEntorno.PRODUCCION;
                    break;
            }

            sCancelacionPago.Pago_id = _requestPago.payment_data.payment_id;

            EnviarCancelacionPago(sCancelacionPago);
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
                
            sRequestPago.payment_request_data.payment_amount = xModel.Importe.ToString();
            sRequestPago.payment_request_data.terminal_menu_text = xModel.Texto_terminal;
            sRequestPago.payment_request_data.ecr_transaction_id = xModel.Referencia.ContainValueString() ? xModel.Referencia : null;
            sRequestPago.payment_request_data.installments_number = xModel.Cuotas;
            sRequestPago.payment_request_data.ecr_provider = xModel.Nombre_integrador;
            sRequestPago.payment_request_data.ecr_name = xModel.Nombre_sistema_integrador;
            sRequestPago.payment_request_data.ecr_version = xModel.Version_sistema_integrador;
            sRequestPago.payment_request_data.change_amount = xModel.Importe_cambio.ToString();

            int sResultadoParseoInt = 0;
            bool sParseoHabilitado = int.TryParse(xModel.Tipo_cuenta, out sResultadoParseoInt);

            //sRequestPago.payment_request_data.bank_account_type = sParseoHabilitado ? Convert.ToInt32(xModel.Tipo_cuenta) : 0;
            sRequestPago.payment_request_data.bank_account_type = xModel.Tipo_cuenta.ContainValueString() ? xModel.Tipo_cuenta : null;
            sRequestPago.payment_request_data.payment_plan_id = xModel.Identificacion_plan_pago.ContainValueString() ? xModel.Identificacion_plan_pago : null;

            switch (xModel.Metodo_impresion)
            {
                case CommonPago.MetodoImpresion.FISCAL:
                    sRequestPago.payment_request_data.print_method = Enums.PrintMethod.MOBITEF_FISCAL.ToString();
                    break;

                case CommonPago.MetodoImpresion.NO_FISCAL:
                    sRequestPago.payment_request_data.print_method = Enums.PrintMethod.MOBITEF_NON_FISCAL.ToString();
                    break;
            }

            switch (xModel.Copias_comprobante_pago)
            {
                case CommonPago.CopiasComprobantePago.NINGUNO:
                    sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.NONE.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.AMBOS:
                    sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.BOTH.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_CLIENTE:
                    sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.CUSTOMER_ONLY.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_COMERCIANTE:
                    sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.MERCHANT_ONLY.ToString();
                    break;

            }

            sRequestPago.payment_request_data.terminals_list = new List<Terminal>();

            if (xModel.Lista_terminales == null)
                xModel.Lista_terminales = new List<string>();

            foreach (string sTerminalItem in xModel.Lista_terminales)
            {
                Terminal sTerminal = new Terminal();
                sTerminal.terminal_id = sTerminalItem;

                sRequestPago.payment_request_data.terminals_list.Add(sTerminal);
            }

            sRequestPago.payment_request_data.card_brand_product = xModel.Marca_tarjeta.ContainValueString() ? xModel.Marca_tarjeta : null;

            switch (xModel.Metodo_operacion)
            {
                case CommonPago.MetodoOperacion.TARJETA:
                    sRequestPago.payment_request_data.terminal_operation_method = Enums.TerminalOPerationMethod.CARD.ToString();
                    break;

                case CommonPago.MetodoOperacion.QR:
                    sRequestPago.payment_request_data.terminal_operation_method = Enums.TerminalOPerationMethod.QR_CODE.ToString();
                    break;
            }

            sRequestPago.payment_request_data.qr_benefit_code = xModel.Admite_tarjeta_beneficio;
            sRequestPago.payment_request_data.trx_receipt_notes = xModel.Nota_impresion_ticket;
            sRequestPago.payment_request_data.card_holder_id = xModel.Dni_cliente.ContainValueString() ? xModel.Dni_cliente : null; 
            sRequestPago.Parametro_original = xModel;

            RequestPagoMin sRequestPagoMin = new RequestPagoMin();
            sRequestPagoMin.payment_request_data = sRequestPago.payment_request_data;

            EnviarSolicitudService<RequestPagoMin, RequestPago>(CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, "/v1/paystore_terminals/terminal_payments", "/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPagoMin, sRequestPago);

            //RequestPago sReturn = await _client.PostAsync<RequestPago, RequestPago>("/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPago);

            //var sReturn = _client.PostAsync<RequestPago, RequestPago>("/payments/cuit_cuil?" + xModel.Cuit_cuil, sHttpContent).Result;
        }

        public static async Task<string> RenovarTokenSync()
        {
            Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.PRISMA };
            sConfiguracion.End_point = "https://api-homo.prismamediosdepago.com";
            sConfiguracion.Entorno = CommonPago.TipoEntorno.HOMOLOGACION;
            sConfiguracion.Tiempo_segundos_persistencias = 500;
            
            Prisma sPrisma = new Prisma(sConfiguracion);            
            
            //API METODO PARA RENOVAR TOKEN
            TokenResponse sReturn = await sPrisma.GenerarTokenSync<TokenResponse>("", "/v1/oauth/accesstoken", "grant_type=client_credentials");

            return sReturn.access_token;
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

        public static async Task<DatosRespuesta> EnviarSolicitudReversionTest(SolicitudReversion xModel) 
        {
            //IReversiones sReversion;

            Configuracion sConfiguracion = new Configuracion() { Tipo = Pagos.CommonPago.Tipo.PRISMA };
            sConfiguracion.Id_terminales = new List<string>();
            sConfiguracion.Id_terminales.Add("asd123");
            sConfiguracion.End_point = "https://api-homo.prismamediosdepago.com";
            sConfiguracion.Sub_end_point = "/v1/paystore_terminals/terminal_reversals";
            sConfiguracion.Sub_end_point_authorization = "/v1/oauth/accesstoken";
            sConfiguracion.Key = "ZGFmMzVmM2UtYzM0Mi00MmFkLWE4YmUtNzAwODA3YzM3MDdmOjM1NDBjMDViLTQxNzMtNGYyNi05NTBkLTNkZGE5NjM4YjdlNQ==";
            sConfiguracion.Entorno = CommonPago.TipoEntorno.HOMOLOGACION;
            sConfiguracion.Tiempo_segundos_persistencias = 500;
            //sConfiguracion.Key = "";

            Prisma sPrisma = new Prisma(sConfiguracion);

            //Pago sReversion = new Pago(sConfiguracion);

            //sReversion.OnRespuesta += Respuesta;

            SolicitudReversion sSolicitudReversion = new SolicitudReversion()
            {
                Cuit_cuil = "20-34146568-1",
                Importe = 120000,
                Nombre_integrador = "ECR",
                Nombre_sistema_integrador = "Software x",
                Version_sistema_integrador = "3.5",
                Texto_terminal = "Pago reverso x 1",
                Referencia = "12345",
                Cuotas = 1,
                Copias_comprobante_pago = CommonPago.CopiasComprobantePago.SOLO_CLIENTE,
                Nota_impresion_ticket = "nota impresa en el ticket",
                Metodo_operacion = CommonPago.MetodoOperacion.TARJETA,
                Metodo_impresion = CommonPago.MetodoImpresion.NO_FISCAL,
                Admite_tarjeta_beneficio = true,
                Pago_id = "476ca6c9-4117-4be8-a47b-029ffbeafd7f"
            };

            sSolicitudReversion.Lista_terminales = new List<string>();
            sSolicitudReversion.Lista_terminales.Add("38011128");

            return await sPrisma.EnviarSolicitudReversionAsync(sSolicitudReversion);

            //return true;
        }

        public async Task<DatosRespuesta> EnviarSolicitudReversionAsync(SolicitudReversion xModel)
        {
            //if (SolicitudEnProceso)
            //{
            //    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "Ya hay una solicitud en proceso", true));
            //    return;
            //}

            SolicitudEnProceso = true;

            RequestReversion sRequestReversion = new RequestReversion();
            sRequestReversion.reversal_request_data = new Reversal_request_data();

            switch (_configuracion.Entorno)
            {
                case CommonPago.TipoEntorno.PRUEBA:
                    sRequestReversion.reversal_request_data.subnet_acquirer_id = "1";
                    break;

                case CommonPago.TipoEntorno.HOMOLOGACION:
                    sRequestReversion.reversal_request_data.subnet_acquirer_id = "9";
                    break;

                case CommonPago.TipoEntorno.PRODUCCION:
                    sRequestReversion.reversal_request_data.subnet_acquirer_id = "2";
                    break;

                default:
                    sRequestReversion.reversal_request_data.subnet_acquirer_id = "2";
                    break;
            }

            sRequestReversion.reversal_request_data.payment_id = xModel.Pago_id;
            sRequestReversion.reversal_request_data.terminal_menu_text = xModel.Texto_terminal;

            switch (xModel.Copias_comprobante_pago)
            {
                case CommonPago.CopiasComprobantePago.NINGUNO:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.NONE.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.AMBOS:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.BOTH.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_CLIENTE:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.CUSTOMER_ONLY.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_COMERCIANTE:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.MERCHANT_ONLY.ToString();
                    break;
            }

            sRequestReversion.reversal_request_data.terminals_list = new List<Terminal>();

            if (xModel.Lista_terminales == null)
                xModel.Lista_terminales = new List<string>();

            foreach (string sTerminalItem in xModel.Lista_terminales)
            {
                Terminal sTerminal = new Terminal();
                sTerminal.terminal_id = sTerminalItem;

                sRequestReversion.reversal_request_data.terminals_list.Add(sTerminal);
            }

            sRequestReversion.Parametro_original = xModel;

            RequestReversionMin sRequestReversionMin = new RequestReversionMin();
            sRequestReversionMin.reversal_request_data = sRequestReversion.reversal_request_data;

            RequestReversion sReturn = await EnviarSolicitudServiceTest<RequestReversionMin, RequestReversion>(CommonPago.TipoRespuestaEvento.SOLICITUD_REVERSION, "/reversals", "cuit_cuil=" + xModel.Cuit_cuil, sRequestReversionMin, sRequestReversion);

            return new DatosRespuesta() { Id = "djfashfaksfja", Descripcion = "pruebaaaa" };
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

            switch (_configuracion.Entorno)
            {
                case CommonPago.TipoEntorno.PRUEBA:
                    sRequestReversion.reversal_request_data.subnet_acquirer_id = "1";
                    break;

                case CommonPago.TipoEntorno.HOMOLOGACION:
                    sRequestReversion.reversal_request_data.subnet_acquirer_id = "9";
                    break;

                case CommonPago.TipoEntorno.PRODUCCION:
                    sRequestReversion.reversal_request_data.subnet_acquirer_id = "2";
                    break;

                default:
                    sRequestReversion.reversal_request_data.subnet_acquirer_id = "2";
                    break;
            }

            sRequestReversion.reversal_request_data.payment_id = xModel.Pago_id;
            sRequestReversion.reversal_request_data.terminal_menu_text = xModel.Texto_terminal;
            
            switch (xModel.Copias_comprobante_pago)
            {
                case CommonPago.CopiasComprobantePago.NINGUNO:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.NONE.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.AMBOS:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.BOTH.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_CLIENTE:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.CUSTOMER_ONLY.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_COMERCIANTE:
                    sRequestReversion.reversal_request_data.print_copies = Enums.PrintCopies.MERCHANT_ONLY.ToString();
                    break;
            }

            sRequestReversion.reversal_request_data.terminals_list = new List<Terminal>();

            if (xModel.Lista_terminales == null)
                xModel.Lista_terminales = new List<string>();

            foreach (string sTerminalItem in xModel.Lista_terminales)
            {
                Terminal sTerminal = new Terminal();
                sTerminal.terminal_id = sTerminalItem;

                sRequestReversion.reversal_request_data.terminals_list.Add(sTerminal);
            }

            sRequestReversion.Parametro_original = xModel;

            RequestReversionMin sRequestReversionMin = new RequestReversionMin();
            sRequestReversionMin.reversal_request_data = sRequestReversion.reversal_request_data;

            EnviarSolicitudService<RequestReversionMin, RequestReversion>(CommonPago.TipoRespuestaEvento.SOLICITUD_REVERSION, "/v1/paystore_terminals/terminal_reversals", "/reversals", "cuit_cuil=" + xModel.Cuit_cuil, sRequestReversionMin, sRequestReversion);
        }

        private void EnviarConsultaEstadoReversion(RequestReversion xModel)
        {
            Thread.Sleep(500);

            int sSegundosPersistencia = _configuracion.Tiempo_segundos_persistencias == 0 ? _tiempoSegundosPersistenciaDefault : _configuracion.Tiempo_segundos_persistencias;

            //if (DateTime.Compare(xModel.Inicio_persistencia.AddSeconds(sSegundosPersistencia), DateTime.UtcNow.AddHours(-3)) < 0)
            //{
            //    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_401, "Error: Tiempo de persistencia agotado", true));
            //    ConsultaEstadoEnProceso = false;

            //    return;
            //}

            Reversal_data sReversalData = xModel.reversal_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            sConsultaEstado.Pago_id = sReversalData.payment_id;
            sConsultaEstado.Referencia = xModel.reversal_request_data.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = xModel.Parametro_original.Cuit_cuil;

            EnviarConsultaEstadoService<RequestReversion, RequestReversion>(CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_REVERSION, "/v1/paystore_terminals/terminal_reversals", "/reversals/" + sConsultaEstado.Pago_id, "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil, xModel);
        }

        public void EnviarCancelacionReversion(Cancelacion xModel)
        {
            RequestReversion sModel = new RequestReversion();
            sModel.reversal_data = new Reversal_data();
            sModel.reversal_data.payment_id = xModel.Pago_id;
            sModel.reversal_request_data = new Reversal_request_data();

            switch (xModel.Entorno)
            {
                case CommonPago.TipoEntorno.PRUEBA:
                    sModel.reversal_request_data.subnet_acquirer_id = "1";
                    break;

                case CommonPago.TipoEntorno.HOMOLOGACION:
                    sModel.reversal_request_data.subnet_acquirer_id = "9";
                    break;

                case CommonPago.TipoEntorno.PRODUCCION:
                    sModel.reversal_request_data.subnet_acquirer_id = "2";
                    break;

                default:
                    sModel.reversal_request_data.subnet_acquirer_id = "2";
                    break;
            }

            sModel.Parametro_original = new SolicitudReversion();
            sModel.Parametro_original.Cuit_cuil = xModel.Cuit_cuil;

            Reversal_data sReversalData = sModel.reversal_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            sConsultaEstado.Pago_id = sReversalData.payment_id;
            sConsultaEstado.Referencia = sModel.reversal_request_data.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = sModel.Parametro_original.Cuit_cuil;

            xModel.Parametro_original = xModel;

            if (sReversalData.payment_id.ContainValueString())
            {
                EnviarCancelacionPutService<Cancelacion, RequestReversion>(CommonPago.TipoRespuestaEvento.CANCELACION_REVERSION, "/v1/paystore_terminals/terminal_reversals", "/reversals/" + sConsultaEstado.Pago_id + "/cancellations", "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil + "&Nota", xModel);
            }
            else
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.CANCELACION_PAGO, "No hay reversion para cancelar"));
            }
        }

        public void EnviarCancelacionReversion()
        {
            Cancelacion sCancelacion = new Cancelacion();
            sCancelacion.Cuit_cuil = _requestReversion.Parametro_original.Cuit_cuil;

            switch (_requestReversion.reversal_request_data.subnet_acquirer_id)
            {
                case "1":
                    sCancelacion.Entorno = CommonPago.TipoEntorno.PRUEBA;
                    break;

                case "9":
                    sCancelacion.Entorno = CommonPago.TipoEntorno.HOMOLOGACION;
                    break;

                case "2":
                    sCancelacion.Entorno = CommonPago.TipoEntorno.PRODUCCION;
                    break;

                default:
                    sCancelacion.Entorno = CommonPago.TipoEntorno.PRODUCCION;
                    break;
            }

            sCancelacion.Pago_id = _requestReversion.reversal_data.payment_id;

            EnviarCancelacionReversion(sCancelacion);
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

        public void EnviarSolicitudDevolucion(SolicitudDevolucion xModel)
        {
            if (SolicitudEnProceso)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "Ya hay una solicitud en proceso", true));
                return;
            }

            SolicitudEnProceso = true;

            RequestDevolucion sRequestDevolucion = new RequestDevolucion();
            
            switch (_configuracion.Entorno)
            {
                case CommonPago.TipoEntorno.PRUEBA:
                    sRequestDevolucion.subnet_acquirer_id = "1";
                    break;

                case CommonPago.TipoEntorno.HOMOLOGACION:
                    sRequestDevolucion.subnet_acquirer_id = "9";
                    break;

                case CommonPago.TipoEntorno.PRODUCCION:
                    sRequestDevolucion.subnet_acquirer_id = "2";
                    break;

                default:
                    sRequestDevolucion.subnet_acquirer_id = "2";
                    break;
            }

            sRequestDevolucion.terminal_menu_text = xModel.Texto_terminal;
            sRequestDevolucion.refund_amount = xModel.Importe.ToString();
            sRequestDevolucion.card_brand_product = xModel.Marca_tarjeta.ContainValueString() ? xModel.Marca_tarjeta : null;

            switch (xModel.Copias_comprobante_pago)
            {
                case CommonPago.CopiasComprobantePago.NINGUNO:
                    sRequestDevolucion.print_copies = Enums.PrintCopies.NONE.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.AMBOS:
                    sRequestDevolucion.print_copies = Enums.PrintCopies.BOTH.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_CLIENTE:
                    sRequestDevolucion.print_copies = Enums.PrintCopies.CUSTOMER_ONLY.ToString();
                    break;

                case CommonPago.CopiasComprobantePago.SOLO_COMERCIANTE:
                    sRequestDevolucion.print_copies = Enums.PrintCopies.MERCHANT_ONLY.ToString();
                    break;
            }

            sRequestDevolucion.terminals_list = new List<Terminal>();

            if (xModel.Lista_terminales == null)
                xModel.Lista_terminales = new List<string>();

            foreach (string sTerminalItem in xModel.Lista_terminales)
            {
                Terminal sTerminal = new Terminal();
                sTerminal.terminal_id = sTerminalItem;

                sRequestDevolucion.terminals_list.Add(sTerminal);
            }

            sRequestDevolucion.ecr_transaction_id = xModel.Referencia.ContainValueString() ? xModel.Referencia : null;
            sRequestDevolucion.ecr_provider = xModel.Nombre_integrador;
            sRequestDevolucion.ecr_name = xModel.Nombre_sistema_integrador;
            sRequestDevolucion.ecr_version = xModel.Version_sistema_integrador;

            sRequestDevolucion.Parametro_original = xModel;

            RequestDevolucionMin sRequestDevolucionMin = new RequestDevolucionMin();
            sRequestDevolucionMin.subnet_acquirer_id = sRequestDevolucion.subnet_acquirer_id;
            sRequestDevolucionMin.terminal_menu_text = sRequestDevolucion.terminal_menu_text;
            sRequestDevolucionMin.ecr_transaction_id = xModel.Referencia.ContainValueString() ? xModel.Referencia : null;
            sRequestDevolucionMin.ecr_provider = xModel.Nombre_integrador;
            sRequestDevolucionMin.ecr_name = xModel.Nombre_sistema_integrador;
            sRequestDevolucionMin.ecr_version = xModel.Version_sistema_integrador;
            sRequestDevolucionMin.refund_amount = xModel.Importe.ToString();
            sRequestDevolucionMin.card_brand_product = xModel.Marca_tarjeta.ContainValueString() ? xModel.Marca_tarjeta : null;
            sRequestDevolucionMin.print_copies = sRequestDevolucion.print_copies;
            sRequestDevolucionMin.terminals_list = sRequestDevolucion.terminals_list;

            EnviarSolicitudService<RequestDevolucionMin, ResponseDevolucion>(CommonPago.TipoRespuestaEvento.SOLICITUD_DEVOLUCION, "/v1/paystore_terminals/terminal_refunds", "/refunds", "cuit_cuil=" + xModel.Cuit_cuil, sRequestDevolucionMin, sRequestDevolucion);
        }

        private void EnviarConsultaEstadoDevolucion(ResponseDevolucion xModel)
        {
            Thread.Sleep(500);

            int sSegundosPersistencia = _configuracion.Tiempo_segundos_persistencias == 0 ? _tiempoSegundosPersistenciaDefault : _configuracion.Tiempo_segundos_persistencias;

            //if (DateTime.Compare(xModel.Inicio_persistencia.AddSeconds(sSegundosPersistencia), DateTime.UtcNow.AddHours(-3)) < 0)
            //{
            //    OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.ERROR_401, "Error: Tiempo de persistencia agotado", true));
            //    ConsultaEstadoEnProceso = false;

            //    return;
            //}

            //Reversal_data sReversalData = xModel.reversal_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            sConsultaEstado.Referencia = xModel.refund_request.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = xModel.Parametro_original.Cuit_cuil;

            EnviarConsultaEstadoService<ResponseDevolucion, ResponseConsultaDevolucion>(CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_DEVOLUCION, "/v1/paystore_terminals/terminal_refunds", "/refunds/" + xModel.refund_data.refund_id, "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil, xModel);
        }

        public void EnviarCancelacionDevolucion(Cancelacion xModel)
        {
            ResponseDevolucion sModel = new ResponseDevolucion();
            sModel.refund_request = new RequestDevolucion();
            sModel.refund_data = new Refund_data();

            switch (xModel.Entorno)
            {
                case CommonPago.TipoEntorno.PRUEBA:
                    sModel.refund_request.subnet_acquirer_id = "1";
                    break;

                case CommonPago.TipoEntorno.HOMOLOGACION:
                    sModel.refund_request.subnet_acquirer_id = "9";
                    break;

                case CommonPago.TipoEntorno.PRODUCCION:
                    sModel.refund_request.subnet_acquirer_id = "2";
                    break;

                default:
                    sModel.refund_request.subnet_acquirer_id = "2";
                    break;
            }

            sModel.Parametro_original = new SolicitudDevolucion();
            sModel.Parametro_original.Cuit_cuil = xModel.Cuit_cuil;

            //Reversal_data sReversalData = xModel.reversal_data;

            ConsultaEstado sConsultaEstado = new ConsultaEstado();
            //sConsultaEstado.Pago_id = sReversalData.payment_id;
            sConsultaEstado.Referencia = sModel.refund_request.subnet_acquirer_id;
            sConsultaEstado.Cuit_cuil = sModel.Parametro_original.Cuit_cuil;

            //EnviarCancelacionPutService<ResponseDevolucion, ResponseConsultaDevolucion>(CommonPago.TipoRespuestaEvento.CANCELACION_DEVOLUCION, "/refunds/" + sModel.refund_data.refund_id + "/cancellations", "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil + "&Nota", xModel);
            EnviarCancelacionPutService<Cancelacion, ResponseConsultaDevolucion>(CommonPago.TipoRespuestaEvento.CANCELACION_DEVOLUCION, "/v1/paystore_terminals/terminal_refunds", "/refunds/" + xModel.Pago_id + "/cancellations", "subnet_acquirer_id=" + sConsultaEstado.Referencia + "&cuit_cuil=" + sConsultaEstado.Cuit_cuil + "&Nota", xModel);
        }

        public void EnviarCancelacionDevolucion()
        {
            Cancelacion sCancelacion = new Cancelacion();
            sCancelacion.Cuit_cuil = _requestDevolucion.Parametro_original.Cuit_cuil;

            switch (_requestDevolucion.refund_request.subnet_acquirer_id)
            {
                case "1":
                    sCancelacion.Entorno = CommonPago.TipoEntorno.PRUEBA;
                    break;

                case "9":
                    sCancelacion.Entorno = CommonPago.TipoEntorno.HOMOLOGACION;
                    break;

                case "2":
                    sCancelacion.Entorno = CommonPago.TipoEntorno.PRODUCCION;
                    break;

                default:
                    sCancelacion.Entorno = CommonPago.TipoEntorno.PRODUCCION;
                    break;
            }

            sCancelacion.Pago_id = _requestDevolucion.refund_data.refund_id;

            EnviarCancelacionDevolucion(sCancelacion);
        }

        public void ReiniciarConsultaEstadoDevolucion()
        {
            if (ConsultaEstadoEnProceso)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "El proceso de consulta anterior esta en ejecucion", true));
                return;
            }

            ConsultaEstadoEnProceso = true;
            EnviarConsultaEstadoDevolucion(_requestDevolucion);
        }

        public void EnviarSolicitudCierre(SolicitudCierre xModel)
        {
            if (SolicitudEnProceso)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "Ya hay una solicitud en proceso", true));
                return;
            }

            SolicitudEnProceso = true;

            RequestCierre sRequestCierre = new RequestCierre();

            switch (_configuracion.Entorno)
            {
                case CommonPago.TipoEntorno.PRUEBA:
                    sRequestCierre.subnet_acquirer_id = "1";
                    break;

                case CommonPago.TipoEntorno.HOMOLOGACION:
                    sRequestCierre.subnet_acquirer_id = "9";
                    break;

                case CommonPago.TipoEntorno.PRODUCCION:
                    sRequestCierre.subnet_acquirer_id = "2";
                    break;

                default:
                    sRequestCierre.subnet_acquirer_id = "2";
                    break;
            }

            sRequestCierre.Parametro_original = xModel;

            sRequestCierre.print_receipt = "false";

            if (xModel.Imprimir_comprobante)
                sRequestCierre.print_receipt = "true";

            EnviarSolicitudService<object, ResponseCierre>(CommonPago.TipoRespuestaEvento.SOLICITUD_CIERRE, "/v1/paystore_terminals/terminal_settlements", "/settlements", "cuit_cuil=" + xModel.Cuit_cuil + "&subnet_acquirer_id=" + sRequestCierre.subnet_acquirer_id + "&terminal_id=" + xModel.Terminal +"&print_receipt=" + sRequestCierre.print_receipt, new object(), sRequestCierre);
        }

        private void EnviarConsultaEstadoCierre(ResponseCierre xModel)
        {
            Thread.Sleep(500);

            int sSegundosPersistencia = _configuracion.Tiempo_segundos_persistencias == 0 ? _tiempoSegundosPersistenciaDefault : _configuracion.Tiempo_segundos_persistencias;

            EnviarConsultaEstadoService<ResponseCierre, ResponseCierre>(CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_CIERRE, "/v1/paystore_terminals/terminal_settlements", "/settlements/" + xModel.settlements_data.settlement_id, "subnet_acquirer_id=" + xModel.settlements_data.subnet_acquirer_id + "&cuit_cuil=" + xModel.Parametro_original.Cuit_cuil, xModel);
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
