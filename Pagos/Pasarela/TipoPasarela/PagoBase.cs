using Pagos.Pasarela.Eventos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Api;
using System.Xml.Linq;
using Pagos.Pasarela.PrismaModel;

namespace Pagos.Pasarela
{
    public abstract class PagoBase
    {
        public event EventHandler OnEventHandler;
        public delegate void RespuestaPagoHandler(object sender, RespuestaPagoEventArgs e);
        public event RespuestaPagoHandler OnRespuestaPagoInt;
        public delegate void RespuestaHandler(object sender, RespuestaEventArgs e);
        public event RespuestaHandler OnRespuestaBase;

        //public delegate void RespuestaGenericHandler<T>(object sender, RespuestaGenericEventArgs<T> e);
        //public event RespuestaGenericHandler<object> OnRespuestaSolicitudPagoInt;

        //public event EventHandler<RespuestaGenericEventArgs<object>> OnRespuestaGenericInt;

        public delegate void DelegateParamVoid<T>(T xParam);

        private ConsultaEstado _param;
        private Func<ConsultaEstado, RespuestaConsultaEstadoPago> _func;
        internal Configuracion _configuracion;
        internal string _token;
        internal bool _solicitudEnProceso;
        internal bool _consultaEstadoEnProceso;
        internal bool _solicitudEliminada;
        public ClientApiEntity _client = new ClientApiEntity();
        public string _subEndPointToken = "";
        public static readonly int _tiempoSegundosPersistenciaDefault = 60;

        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public bool SolicitudEnProceso
        {
            get { return _solicitudEnProceso; }
            set { _solicitudEnProceso = value; }
        }

        public bool ConsultaEstadoEnProceso
        {
            get { return _consultaEstadoEnProceso; }
            set { _consultaEstadoEnProceso = value; }
        }

        public void SetClient()
        {
            _client = new ClientApiEntity(_configuracion.End_point, _configuracion.Sub_end_point);
        }

        public virtual void PersistirConsultaPago(Func<ConsultaEstado, RespuestaConsultaEstadoPago> xFunc, ConsultaEstado xParam, ref RespuestaPagoHandler xOnRespuestaPagoHandler)
        {
            _param = xParam;
            //_func = xFunc;
            OnRespuestaPagoInt = xOnRespuestaPagoHandler;

            Task sTask = new Task(PersistirPago);
            sTask.Start();
        }

        private void PersistirPago()
        {
            int sCiclo = 0;

            while (sCiclo < _configuracion.Cantidad_persistencias)
            {
                RespuestaConsultaEstadoPago sRespuesta = _func.Invoke(_param);
                sRespuesta.Cantidad_intentos_persistencia = sCiclo + 1;
                sRespuesta.Persistencia_finalizada = false;

                if ((sCiclo + 1) == _configuracion.Cantidad_persistencias)
                    sRespuesta.Persistencia_finalizada = true;

                OnRespuestaPagoInt(this, (new RespuestaPagoEventArgs(sRespuesta)));

                if (sRespuesta.Confirmado)
                {
                    sRespuesta.Persistencia_finalizada = true;
                    break;
                }

                Thread.Sleep(5000);

                sCiclo++;
            }
        }

        public virtual async Task GenerarToken<T>(string xAddressSuffix, string xEndPoint, string xParam, object xParametroOriginal, CommonPago.TipoRespuestaEvento xTipoOrigen)
        {
            try
            {
                _client.ActualizarAuthorization(_configuracion.Key, ClientApiEntity.TipeAuthorization.Basic);
                T sReturn = await _client.GetAsync<T>(xAddressSuffix, xEndPoint, xParam);

                //OnRespuestaSolicitudPagoInt(this, new RespuestaGenericEventArgs<object>(CommonPago.TipoRespuestaEvento.TOKEN, sReturn, new object(), xDelegate));

                OnRespuestaBase(this, new RespuestaEventArgs(CommonPago.TipoRespuestaEvento.TOKEN, sReturn, xParametroOriginal, xTipoOrigen));
            }
            catch (Exception e)
            {
                string asasad = e.Message;
            }
        }

        public virtual async Task GenerarToken<T>(ClientApiEntity.TipeAuthorization xTipo, string xAddressSuffix, string xEndPoint, string xParam, object xParametroOriginal, CommonPago.TipoRespuestaEvento xTipoOrigen)
        {
            try
            {
                _client.ActualizarAuthorization(_configuracion.Key, xTipo);
                T sReturn = await _client.GetAsync<T>(xAddressSuffix, xEndPoint, xParam);

                OnRespuestaBase(this, new RespuestaEventArgs(CommonPago.TipoRespuestaEvento.TOKEN, sReturn, xParametroOriginal, xTipoOrigen));
            }
            catch (Exception e)
            {
                string asasad = e.Message;
            }
        }

        public virtual async Task GenerarToken<T>(string xAddressSuffix, string xEndPoint, string xParam)
        {
            try
            {
                _client.ActualizarAuthorization(_configuracion.Key, ClientApiEntity.TipeAuthorization.Basic);
                T sReturn = await _client.GetAsync<T>(xAddressSuffix, xEndPoint, xParam);

                OnRespuestaBase(this, new RespuestaEventArgs(CommonPago.TipoRespuestaEvento.TOKEN, sReturn, new object()));
            }
            catch{}

            //string sAs = "";

            //using (HttpClient client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri("https://api-sandbox.prismamediosdepago.com");
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "MkpPaHkwbE9SSmRLNUxRaWh6TW5KdVZyQ3dsb0dZWjc6Rnh3NEdRTmx6b3lCUlgzUQ==");
            //    client.Timeout = TimeSpan.FromMinutes(10);
            //    //Al infinito
            //    client.Timeout = new TimeSpan(0, 0, 0, 0, -1);
            //    HttpResponseMessage response = await client.GetAsync("https://api-sandbox.prismamediosdepago.com/v1/oauth/accesstoken?grant_type=client_credentials");

            //    if (response.IsSuccessStatusCode)
            //    {
            //        TokenResponse sReturnDirecto = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

            //        string algo = "";
            //        //// Si se va a recibir un JSON bastante grande
            //        //// https://stackoverflow.com/questions/56398881/how-to-read-json-payload-of-a-large-response

            //        //var stream = await response.Content.ReadAsStreamAsync();

            //        //using (StreamReader sr = new StreamReader(stream))
            //        //using (JsonReader reader = new JsonTextReader(sr))
            //        //{
            //        //    JsonSerializer serializer = new JsonSerializer();
            //        //    TokenResponse sReturn = serializer.Deserialize<TokenResponse>(reader);
            //        //}

            //        //var jsonPuro = await response.Content.ReadAsStringAsync();
            //        //var jsonDesarializado = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonPuro);


            //        ////lista = JsonConvert
            //        ////        .DeserializeObject<List<PeriodoNominaBE>>(jsonPuro.ToString()
            //        ////        , new JsonSerializerSettings()
            //        ////        {
            //        ////            MissingMemberHandling =
            //        ////                MissingMemberHandling.Ignore
            //        ////        });

            //    }
            //}
        }

        public virtual async Task GenerarToken<P, R>(ClientApiEntity.TipeAuthorization xTipoAutorizacion, string xAddressSuffix, string xEndPoint, string xParam, P xRequestToken)
        {
            try
            {
                _client.ActualizarAuthorization(_configuracion.Key, xTipoAutorizacion);
                R sReturn = await _client.PostAsync<P, R>(xEndPoint, xParam, xRequestToken);

                OnRespuestaBase(this, new RespuestaEventArgs(CommonPago.TipoRespuestaEvento.TOKEN, sReturn, new object()));
            }
            catch { }
        }

        public virtual async Task GenerarToken<P, R>(ClientApiEntity.TipeAuthorization xTipoAutorizacion, string xAddressSuffix, string xEndPoint, P xRequestToken)
        {
            try
            {
                _client.ActualizarAuthorization(_configuracion.Key, xTipoAutorizacion);
                R sReturn = await _client.PostAsync<P, R>("", xEndPoint, "", xRequestToken);

                OnRespuestaBase(this, new RespuestaEventArgs(CommonPago.TipoRespuestaEvento.TOKEN, sReturn, new object()));
            }
            catch{}
        }

        public virtual async Task GenerarToken<P, R>(ClientApiEntity.TipeAuthorization xTipoAutorizacion, string xAddressSuffix, string xEndPoint, P xRequestToken, object xParametroOriginal, CommonPago.TipoRespuestaEvento xTipoOrigen)
        {
            try
            {
                _client.ActualizarAuthorization(_configuracion.Key, xTipoAutorizacion);
                R sReturn = await _client.PostAsync<P, R>("", xEndPoint, "", xRequestToken);

                OnRespuestaBase(this, new RespuestaEventArgs(CommonPago.TipoRespuestaEvento.TOKEN, sReturn, xParametroOriginal, xTipoOrigen));
            }
            catch { }
        }


        public virtual async Task EnviarSolicitudService<P, R>(CommonPago.TipoRespuestaEvento xTipo, string xEndPoint, string xParam, P xRequestPago)
        {
            try
            {
                _client.ActualizarAuthorization(Token, ClientApiEntity.TipeAuthorization.Bearer);
                R sReturn = await _client.PostAsync<P, R>(xEndPoint, xParam, xRequestPago);

                OnRespuestaBase(this, new RespuestaEventArgs(xTipo, sReturn, xRequestPago));
            }
            catch { }
        }

        public virtual async Task EnviarSolicitudService<P, R>(CommonPago.TipoRespuestaEvento xTipo, string xEndPoint, string xParam, P xRequestPago, object xParametroOriginal)
        {
            try
            {
                _client.ActualizarAuthorization(Token, ClientApiEntity.TipeAuthorization.Bearer);
                R sReturn = await _client.PostAsync<P, R>(xEndPoint, xParam, xRequestPago);

                OnRespuestaBase(this, new RespuestaEventArgs(xTipo, sReturn, xParametroOriginal));
            }
            catch { }
        }

        public virtual async Task<R> EnviarSolicitudServiceTest<P, R>(CommonPago.TipoRespuestaEvento xTipo, string xEndPoint, string xParam, P xRequestPago, object xParametroOriginal)
        {
            //try
            //{
                _client.ActualizarAuthorization(Token, ClientApiEntity.TipeAuthorization.Bearer);
                R sReturn = await _client.PostAsync<P, R>(xEndPoint, xParam, xRequestPago);

                OnRespuestaBase(this, new RespuestaEventArgs(xTipo, sReturn, xParametroOriginal));

                return sReturn;
            //}
            //catch { return new object(); }
        }

        public virtual async Task ActualizarSolicitudService<P, R>(CommonPago.TipoRespuestaEvento xTipo, string xEndPoint, string xParam, P xRequestPago, object xParametroOriginal)
        {
            try
            {
                _client.ActualizarAuthorization(Token, ClientApiEntity.TipeAuthorization.Bearer);
                R sReturn = await _client.PutAsync<P, R>(xEndPoint, xParam, xRequestPago);

                OnRespuestaBase(this, new RespuestaEventArgs(xTipo, sReturn, xParametroOriginal));
            }
            catch { }
        }

        public virtual async Task EnviarConsultaEstadoService<P, R>(CommonPago.TipoRespuestaEvento xTipo, string xEndPoint, string xParam, P xRequestConsultaEstadoPago)
        {
            try
            {
                _client.ActualizarAuthorization(Token, ClientApiEntity.TipeAuthorization.Bearer);
                //object sReturnTemp = await _client.GetAsync<object>(xEndPoint, xParam);
                R sReturn = await _client.GetAsync<R>(xEndPoint, xParam);

                OnRespuestaBase(this, new RespuestaEventArgs(xTipo, sReturn, xRequestConsultaEstadoPago));
            }
            catch(Exception ex)
            {
                string sMensaje = ex.Message;
            }
        }

        public virtual async Task EnviarConsultaEstadoService<P, R>(CommonPago.TipoRespuestaEvento xTipo, string xAddressSuffix, string xEndPoint, string xParam, P xRequestConsultaEstadoPago)
        {
            try
            {
                _client.ActualizarAuthorization(Token, ClientApiEntity.TipeAuthorization.Bearer);
                R sReturn = await _client.GetAsync<R>(xAddressSuffix, xEndPoint, xParam);

                OnRespuestaBase(this, new RespuestaEventArgs(xTipo, sReturn, xRequestConsultaEstadoPago));
            }
            catch { }
        }

        public virtual async Task EnviarCancelacionService<P, R>(CommonPago.TipoRespuestaEvento xTipo, string xEndPoint, string xParam, P xRequestConsultaEstadoPago)
        {
            try
            {
                _client.ActualizarAuthorization(Token, ClientApiEntity.TipeAuthorization.Bearer);
                //R sReturn = await _client.PutAsync<R>(xEndPoint, xParam);
                await _client.DeleteAsync(xEndPoint);

                OnRespuestaBase(this, new RespuestaEventArgs(xTipo, "", xRequestConsultaEstadoPago));
            }
            catch { }
        }

        public virtual async Task EnviarCancelacionPutService<P, R>(CommonPago.TipoRespuestaEvento xTipo, string xEndPoint, string xParam, P xRequestConsultaEstadoPago)
        {
            try
            {
                _client.ActualizarAuthorization(Token, ClientApiEntity.TipeAuthorization.Bearer);
                R sReturn = await _client.PutAsync<R>(xEndPoint, xParam);
                
                OnRespuestaBase(this, new RespuestaEventArgs(xTipo, sReturn, xRequestConsultaEstadoPago));
            }
            catch { }
        }

        public static async Task<R> EnviarCreateSucursalService<P, R>(ClientApiEntity xClient, string xEndPoint, string xParam, P xSucursalRequest)
        {
            //xClient.ActualizarAuthorization(Token, ClientApiEntity.TipeAuthorization.Bearer);
            return await xClient.PostAsync<P, R>("", xEndPoint, xParam, xSucursalRequest);
        }
    }
}
