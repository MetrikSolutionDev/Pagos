using Pagos.Pasarela.Eventos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pagos.Pasarela.PagoBase;

namespace Pagos
{
    public interface IDevoluciones : IPagoEvento
    {
        void EnviarSolicitudDevolucion(SolicitudDevolucion xModel);

        /// <summary>
        /// Cuando se termina el tiempo de persistencia, se puede volver a reiniciar
        /// </summary>
        void ReiniciarConsultaEstadoDevolucion();

        /// <summary>
        /// Cancela la intencion de devolcuion
        /// </summary>
        void EnviarCancelacionDevolucion();

        void EnviarCancelacionDevolucion(Cancelacion xModel);
    }
}
