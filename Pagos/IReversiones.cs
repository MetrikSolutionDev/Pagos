using Pagos.Pasarela.Eventos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pagos.Pasarela.PagoBase;

namespace Pagos
{
    public interface IReversiones : IPagoEvento
    {
        void EnviarSolicitudReversion(SolicitudReversion xModel);

        /// <summary>
        /// Cuando se termina el tiempo de persistencia, se puede volver a reiniciar
        /// </summary>
        void ReiniciarConsultaEstadoReversion();

        /// <summary>
        /// Cancela la intencion de reversion
        /// </summary>
        void EnviarCancelacionReversion();
    }
}
