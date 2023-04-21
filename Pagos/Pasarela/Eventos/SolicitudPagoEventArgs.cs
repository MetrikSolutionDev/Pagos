using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.Eventos
{
    public class RespuestaEventArgs : EventArgs
    {
        public enum Tipo
        {
            TOKEN,
            SOLICITUD_PAGO,
            CONSULTA_PAGO
        }

        private Tipo _tipo;

        private object _respuesta;

        public Tipo TipoRespuesta
        {
            get { return _tipo; }
            set { _tipo = value; }
        }

        public object Respuesta
        {
            get { return _respuesta; }
            set { _respuesta = value; }
        }

        public RespuestaEventArgs(Tipo xTipo, object xRespuesta)
        {
            _tipo = xTipo;
            _respuesta = xRespuesta;
        }
    }
}
