using Newtonsoft.Json;
using Pagos.Pasarela.Eventos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public class Prisma : PagoBase, IPago
    {
        public Prisma(Configuracion xConfiguracion)
        {
            _configuracion = xConfiguracion;
            SetClient();
        }

        public event RespuestaPagoHandler OnRespuestaPago;
        public event SolicitudPagoHandler OnSolicitudPago;

        public RespuestaConsultaEstadoPago ConsultaEstadoPagoPersistente(ConsultaEstadoPago xModel)
        {
            RequestPago sRequest = new RequestPago();


            //HAGO LA CONSULTA A LA API
            HttpContent sHttpContent = new StringContent(JsonConvert.SerializeObject(xModel), Encoding.UTF8);

            var sReturn = _client.PostAsync(_configuracion.Sub_end_point, sHttpContent).Result;

            return new RespuestaConsultaEstadoPago() { Confirmado = false, Lote = "12345" };
        }

        public bool EnviarConsultaEstadoPago(ConsultaEstadoPago xModel)
        {
            PersistirConsultaPago(ConsultaEstadoPagoPersistente, xModel, ref OnRespuestaPago);

            return true;
        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            OnSolicitudPago(this, new SolicitudPagoEventArgs(true));
        }

        public void RenovarToken()
        {
            string sToken = "b144664236a29f8dd12d6e950b67";

            //API METODO PARA RENOVAR TOKEN

            Token = sToken;
        }
    }
}
