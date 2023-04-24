using Pagos.Pasarela.Eventos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pagos.Pasarela.PagoBase;

namespace Pagos
{
    public interface IPagos : IPagoEvento
    {
        void EnviarSolicitudPago(SolicitudPago xModel);

        /// <summary>
        /// Cuando se termina el tiempo de persistencia, se puede volver a reiniciar
        /// </summary>
        void ReiniciarConsultaEstadoPago();

        /// <summary>
        /// Cancela la intencion de pago
        /// </summary>
        void EnviarCancelacionPago();

        //bool EnviarConsultaEstadoPago(ConsultaEstadoPago xModel);

        //RespuestaConsultaEstadoPago ConsultaEstadoPagoPersistente(ConsultaEstadoPago xModel);
    }
}
