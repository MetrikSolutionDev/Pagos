using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pagos.Pasarela.PagoBase;

namespace Pagos.Pasarela
{
    public interface IPago
    {
        event RespuestaPagoHandler OnRespuestaPago;
        event SolicitudPagoHandler OnSolicitudPago;

        void RenovarToken();

        void EnviarSolicitudPago(SolicitudPago xModel);

        bool EnviarConsultaEstadoPago(ConsultaEstadoPago xModel);

        RespuestaConsultaEstadoPago ConsultaEstadoPagoPersistente(ConsultaEstadoPago xModel);
    }
}
