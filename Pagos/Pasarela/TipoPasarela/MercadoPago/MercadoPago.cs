using Core.Api;
using Core.Business;
using Pagos.Pasarela.Eventos;
using Pagos.Pasarela.MercadoPagoModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public class MercadoPago : PagoBase, IPago, IPagoHandler, IAutenticacion
    {
        public MercadoPago(Configuracion xConfiguracion)
        {
            _configuracion = xConfiguracion;
            SetClient();

            if (xConfiguracion.Token.ContainValueString())
                Token = xConfiguracion.Token;

            OnRespuestaBase += RespuestaBase;
        }

        public event RespuestaExternaHandler OnRespuesta;

        public void EnviarCancelacionPago()
        {
            throw new NotImplementedException();
        }

        public void EnviarCancelacionReversion()
        {
            throw new NotImplementedException();
        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            if (SolicitudEnProceso)
            {
                OnRespuesta(this, new RespuestaExternaEventArgs(CommonPago.TipoRespuestaExternaEvento.PROCESO_EN_EJECUCION, "Ya hay una solicitud en proceso", true));
                return;
            }

            SolicitudEnProceso = true;

            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.QR) 
            {
                RequestPagoQr sRequestPago = new RequestPagoQr();
                sRequestPago.user_id = _configuracion.User_id;
                sRequestPago.external_pos_id = xModel.Pos;
                sRequestPago.external_store_id = xModel.Sucursal;
                sRequestPago.external_reference = xModel.Referencia;
                sRequestPago.total_amount = xModel.Importe;

                string sEndPoint = sRequestPago.user_id + "/stores/" + sRequestPago.external_store_id + "/pos/" + sRequestPago.external_pos_id + "/orders";

                ActualizarSolicitudService<RequestPagoQr, ResponsePagoQr>(CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, sEndPoint, "", sRequestPago);
            }
            else
            {
                //RequestPago sRequestPago = new RequestPago();
                //sRequestPago.payment_request_data = new Payment_request_data();
                //sRequestPago.payment_request_data.subnet_acquirer_id = "1";
                //sRequestPago.payment_request_data.payment_amount = xModel.Importe.ToString("N2");
                //sRequestPago.payment_request_data.terminal_menu_text = xModel.Texto_terminal;
                //sRequestPago.payment_request_data.ecr_transaction_id = xModel.Referencia.ContainValueString() ? xModel.Referencia : null;
                //sRequestPago.payment_request_data.installments_number = xModel.Cuotas;
                //sRequestPago.payment_request_data.ecr_provider = xModel.Nombre_integrador;
                //sRequestPago.payment_request_data.ecr_name = xModel.Nombre_sistema_integrador;
                //sRequestPago.payment_request_data.ecr_version = xModel.Version_sistema_integrador;
                //sRequestPago.payment_request_data.change_amount = xModel.Importe_cambio.ToString();

                //int sResultadoParseoInt = 0;
                //bool sParseoHabilitado = int.TryParse(xModel.Tipo_cuenta, out sResultadoParseoInt);

                //sRequestPago.payment_request_data.bank_account_type = sParseoHabilitado ? Convert.ToInt32(xModel.Tipo_cuenta) : 0;
                //sRequestPago.payment_request_data.payment_plan_id = xModel.Identificacion_plan_pago.ContainValueString() ? xModel.Identificacion_plan_pago : null;

                //switch (xModel.Metodo_impresion)
                //{
                //    case CommonPago.MetodoImpresion.FISCAL:
                //        sRequestPago.payment_request_data.print_method = Enums.PrintMethod.MOBITEF_FISCAL;
                //        break;

                //    case CommonPago.MetodoImpresion.NO_FISCAL:
                //        sRequestPago.payment_request_data.print_method = Enums.PrintMethod.MOBITEF_NON_FISCAL;
                //        break;
                //}

                //switch (xModel.Copias_comprobante_pago)
                //{
                //    case CommonPago.CopiasComprobantePago.NINGUNO:
                //        sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.NONE;
                //        break;

                //    case CommonPago.CopiasComprobantePago.AMBOS:
                //        sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.BOTH;
                //        break;

                //    case CommonPago.CopiasComprobantePago.SOLO_CLIENTE:
                //        sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.CUSTOMER_ONLY;
                //        break;

                //    case CommonPago.CopiasComprobantePago.SOLO_COMERCIANTE:
                //        sRequestPago.payment_request_data.print_copies = Enums.PrintCopies.MERCHANT_ONLY;
                //        break;

                //}

                //sRequestPago.payment_request_data.terminals_list = new List<Terminal>();

                //if (xModel.Lista_terminales == null)
                //    xModel.Lista_terminales = new List<string>();

                //foreach (string sTerminalItem in xModel.Lista_terminales)
                //{
                //    Terminal sTerminal = new Terminal();
                //    sTerminal.Termina_id = sTerminalItem;

                //    sRequestPago.payment_request_data.terminals_list.Add(sTerminal);
                //}

                //sRequestPago.payment_request_data.card_brand_product = xModel.Marca_tarjeta.ContainValueString() ? xModel.Marca_tarjeta : null;

                //switch (xModel.Metodo_operacion)
                //{
                //    case CommonPago.MetodoOperacion.TARJETA:
                //        sRequestPago.payment_request_data.terminal_operation_method = Enums.TerminalOPerationMethod.CARD;
                //        break;

                //    case CommonPago.MetodoOperacion.QR:
                //        sRequestPago.payment_request_data.terminal_operation_method = Enums.TerminalOPerationMethod.QR_CODE;
                //        break;
                //}

                //sRequestPago.payment_request_data.qr_benefit_code = xModel.Admite_tarjeta_beneficio;
                //sRequestPago.payment_request_data.trx_receipt_notes = xModel.Nota_impresion_ticket;
                //sRequestPago.payment_request_data.card_holder_id = xModel.Dni_cliente.ContainValueString() ? xModel.Dni_cliente : null;
                //sRequestPago.Parametro_original = xModel;

                //EnviarSolicitudService<RequestPago, RequestPago>(CommonPago.TipoRespuestaEvento.SOLICITUD_PAGO, "/payments", "cuit_cuil=" + xModel.Cuit_cuil, sRequestPago);
            }
        }

        public void EnviarSolicitudReversion(SolicitudReversion xModel)
        {
            throw new NotImplementedException();
        }

        public void ReiniciarConsultaEstadoPago()
        {
            throw new NotImplementedException();
        }

        public void ReiniciarConsultaEstadoReversion()
        {
            throw new NotImplementedException();
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
                                EnviarSolicitudPago(((RequestPagoQr)e.ParametroOriginal).Parametro_original);
                            }
                            
                            if (_configuracion.Tipo_integracion == CommonPago.TipoIntegracion.POINT)
                            {
                                EnviarSolicitudPago(((RequestPagoPoint)e.ParametroOriginal).Parametro_original);
                            }

                            break;

                        case CommonPago.TipoRespuestaEvento.CONSULTA_ESTADO_PAGO:
                            ConsultaEstadoEnProceso = false;
                            //EnviarConsultaEstadoPago((RequestPago)e.ParametroOriginal);
                            break;

                        case CommonPago.TipoRespuestaEvento.CANCELACION_PAGO:
                            //EnviarCancelacionPago((RequestPago)e.ParametroOriginal);
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
                        RequestPagoQr sRespuestaSolicitudPago = (RequestPagoQr)e.Respuesta;
                        RequestPagoQr sParametroOriginalSolicitudPago = (RequestPagoQr)e.ParametroOriginal;
                        sParametroOriginalSolicitudPago.Parametro_original.Nro_intento_generacion_token++;

                    }

                    break;
            }
        }
    }
}
