using Pagos.Pasarela;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pagos.Configuracion;

namespace Pagos
{
    public class Pago : IPago
    {
        private IPago _pago;

        public Pago()
        {
        }

        public Pago(Configuracion xConfiguracion) 
        {
            _pago = PagoFactory.Instance(xConfiguracion);
        }

        public event RespuestaExternaHandler OnRespuesta;

        public void EnviarCancelacionPago()
        {
            _pago.EnviarCancelacionPago();
        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            _pago.EnviarSolicitudPago(xModel);
        }

        public void EnviarSolicitudReversion(SolicitudReversion xModel)
        {
            throw new NotImplementedException();
        }

        public void ReiniciarConsultaEstadoPago()
        {
            _pago.ReiniciarConsultaEstadoPago();
        }
    }
}
