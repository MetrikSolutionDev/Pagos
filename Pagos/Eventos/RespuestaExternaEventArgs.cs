using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pagos.CommonPago;
using static Pagos.Pasarela.PagoBase;

namespace Pagos
{
    public class RespuestaExternaEventArgs : EventArgs
    {
        private TipoRespuestaExternaEvento _tipo;

        private object _respuesta;
        private string _mensaje;
        private string _info;
        private bool _error;

        public TipoRespuestaExternaEvento TipoRespuesta
        {
            get { return _tipo; }
            set { _tipo = value; }
        }

        public object Respuesta
        {
            get { return _respuesta; }
            set { _respuesta = value; }
        }

        public string Mensaje
        {
            get { return _mensaje; }
            set { _mensaje = value; }
        }

        public string Info
        {
            get { return _info; }
            set { _info = value; }
        }

        public bool Error
        {
            get { return _error; }
            set { _error = value; }
        }

        public RespuestaExternaEventArgs()
        {
        }

        public RespuestaExternaEventArgs(TipoRespuestaExternaEvento xTipo, string xMensaje)
        {
            _tipo = xTipo;
            _mensaje = xMensaje;
        }

        public RespuestaExternaEventArgs(TipoRespuestaExternaEvento xTipo, string xMensaje, string xInfo)
        {
            _tipo = xTipo;
            _mensaje = xMensaje;
            _info = xInfo;
        }

        public RespuestaExternaEventArgs(TipoRespuestaExternaEvento xTipo, string xMensaje, bool xError)
        {
            _tipo = xTipo;
            _mensaje = xMensaje;
            _error = xError;
        }
    }
}
