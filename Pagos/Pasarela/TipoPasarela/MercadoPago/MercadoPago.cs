using Pagos.Pasarela.Eventos;
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public class MercadoPago : PagoBase, IPago, IPagoHandler, IAutenticacion
    {
        public MercadoPago(Configuracion xConfiguracion)
        {
            _configuracion = xConfiguracion;
            SetClient();

            OnRespuestaBase += RespuestaBase;
        }

        public event RespuestaExternaHandler OnRespuesta;

        public void EnviarCancelacionPago()
        {
            throw new NotImplementedException();
        }

        public void EnviarSolicitudPago(SolicitudPago xModel)
        {
            throw new NotImplementedException();
        }

        public void EnviarSolicitudReversion(SolicitudReversion xModel)
        {
            throw new NotImplementedException();
        }

        public void ReiniciarConsultaEstadoPago()
        {
            throw new NotImplementedException();
        }

        public void RenovarToken()
        {
            throw new NotImplementedException();
        }

        public void RespuestaBase(object sender, RespuestaEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
