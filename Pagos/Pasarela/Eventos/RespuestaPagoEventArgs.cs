using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public class RespuestaPagoEventArgs : EventArgs
    {
        private bool _confirmado;
        private RespuestaConsultaEstadoPago _respuesta;

        public bool Confirmado
        {
            get { return _confirmado; }
            set { _confirmado = value; }
        }

        public RespuestaConsultaEstadoPago Respuesta
        {
            get { return _respuesta; }
            set { _respuesta = value; }
        }

        public RespuestaPagoEventArgs(RespuestaConsultaEstadoPago xRespuesta)
        {
            _respuesta = xRespuesta;
        }
    }
}
