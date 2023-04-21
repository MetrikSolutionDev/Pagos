using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public class MercadoPago : PagoBase, IPago
    {
        public MercadoPago(Configuracion xConfiguracion)
        {
            _configuracion = xConfiguracion;
            SetClient();
        }

        public event RespuestaPagoHandler OnRespuestaPago;
        public event RespuestaHandler OnRespuesta;

        public RespuestaConsultaEstadoPago ConsultaEstadoPagoPersistente(ConsultaEstadoPago xModel)
        {
            throw new NotImplementedException();
        }

        public bool EnviarConsultaEstadoPago(ConsultaEstadoPago xModel)
        {
            PersistirConsultaPago(ConsultaEstadoPagoPersistente, xModel, ref OnRespuestaPago);

            throw new NotImplementedException();
        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            throw new NotImplementedException();
        }

        public void RenovarToken()
        {
            throw new NotImplementedException();
        }
    }
}
