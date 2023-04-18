using Pagos.Pasarela.Eventos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public abstract class PagoBase
    {
        public event EventHandler OnEventHandler;
        public delegate void RespuestaPagoHandler(object sender, RespuestaPagoEventArgs e);
        public event RespuestaPagoHandler OnRespuestaPagoInt;
        public delegate void SolicitudPagoHandler(object sender, SolicitudPagoEventArgs e);
        public event SolicitudPagoHandler OnSolicitudPagoInt;

        private ConsultaEstadoPago _param;
        private Func<ConsultaEstadoPago, RespuestaConsultaEstadoPago> _func;
        internal Configuracion _configuracion;
        internal string _token;
        public HttpClient _client = new HttpClient();

        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public void SetClient()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_configuracion.End_point);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer b144664236a29f8dd12d6e950b67");
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
    }
}
