using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pagos.Pasarela.PagoBase;
using static Pagos.Pasarela.PrismaModel.Enums;

namespace Pagos.Pasarela.Eventos
{
    public class RespuestaEventArgs : EventArgs
    {
        private TipoRespuestaEvento _tipo;
        private TipoRespuestaEvento _tipoOrigen;

        private object _respuesta;
        private object _parametroOriginal;
        private string _mensaje;

        public TipoRespuestaEvento TipoRespuesta
        {
            get { return _tipo; }
            set { _tipo = value; }
        }

        public TipoRespuestaEvento TipoRespuestaOrigen
        {
            get { return _tipoOrigen; }
            set { _tipoOrigen = value; }
        }

        public object Respuesta
        {
            get { return _respuesta; }
            set { _respuesta = value; }
        }

        public object ParametroOriginal
        {
            get { return _parametroOriginal; }
            set { _parametroOriginal = value; }
        }

        public string Mensaje
        {
            get { return _mensaje; }
            set { _mensaje = value; }
        }

        public RespuestaEventArgs(TipoRespuestaEvento xTipo, string xMensaje)
        {
            _tipo = xTipo;
            _mensaje = xMensaje;
        }

        public RespuestaEventArgs(TipoRespuestaEvento xTipo, object xRespuesta, object xParametroOriginal)
        {
            _tipo = xTipo;
            _respuesta = xRespuesta;
            _parametroOriginal = xParametroOriginal;
            _tipoOrigen = TipoRespuestaEvento.NINGUNO;
        }

        public RespuestaEventArgs(TipoRespuestaEvento xTipo, object xRespuesta, object xParametroOriginal, TipoRespuestaEvento xTipoOrigen)
        {
            _tipo = xTipo;
            _respuesta = xRespuesta;
            _parametroOriginal = xParametroOriginal;
            _tipoOrigen = xTipoOrigen;
        }
    }
}
