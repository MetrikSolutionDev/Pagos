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

namespace Pagos.Pasarela
{
    public abstract class PagoBase
    {
        public event EventHandler OnEventHandler;
        public delegate void RespuestaPagoHandler(object sender, RespuestaPagoEventArgs e);
        public event RespuestaPagoHandler OnRespuestaPagoInt;
        public delegate void RespuestaHandler(object sender, RespuestaEventArgs e);
        public event RespuestaHandler OnRespuestaInt;

        private ConsultaEstadoPago _param;
        private Func<ConsultaEstadoPago, RespuestaConsultaEstadoPago> _func;
        internal Configuracion _configuracion;
        internal string _token;
        public ClientApiEntity _client = new ClientApiEntity();
        public string _subEndPointToken = "";
        
        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public void SetClient()
        {
            _client = new ClientApiEntity(_configuracion.End_point, _configuracion.Sub_end_point);
        }

        public virtual void PersistirConsultaPago(Func<ConsultaEstadoPago, RespuestaConsultaEstadoPago> xFunc, ConsultaEstadoPago xParam, ref RespuestaPagoHandler xOnRespuestaPagoHandler)
        {
            _param = xParam;
            _func = xFunc;
            OnRespuestaPagoInt = xOnRespuestaPagoHandler;

            Task sTask = new Task(PersistirPago);
            sTask.Start();
        }

        private void PersistirPago()
        {
            int sCiclo = 0;

            while (sCiclo < _configuracion.Cantidad_persistencias_pago)
            {
                RespuestaConsultaEstadoPago sRespuesta = _func.Invoke(_param);
                sRespuesta.Cantidad_intentos_persistencia = sCiclo + 1;
                sRespuesta.Persistencia_finalizada = false;

                if ((sCiclo + 1) == _configuracion.Cantidad_persistencias_pago)
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

        public virtual async Task GenerarToken<T>(string xAddressSuffix, string xEndPoint, string xParam)
        {
            try
            {
                _client.ActualizarAuthorization(_configuracion.Key, ClientApiEntity.TipeAuthorization.Basic);
                T sReturn = await _client.GetAsync<T>(xAddressSuffix, xEndPoint, xParam);

                //Token = sReturn.access_token != null ? sReturn.access_token : "";

                OnRespuestaInt(this, new RespuestaEventArgs(RespuestaEventArgs.Tipo.TOKEN, sReturn));
            }
            catch (Exception e)
            {
                string asasad = e.Message;
            }

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

        public virtual async Task EnviarSolicitudPagoService<P, R>(string xEndPoint, string xParam, P xRequestPago)
        {
            try
            {
                R sReturn = await _client.PostAsync<P, R>(xEndPoint, xParam, xRequestPago);

                OnRespuestaInt(this, new RespuestaEventArgs(RespuestaEventArgs.Tipo.SOLICITUD_PAGO, sReturn));
            }
            catch { }
        }
    }
}
