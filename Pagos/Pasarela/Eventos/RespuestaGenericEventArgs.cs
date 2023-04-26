using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pagos.CommonPago;
using static Pagos.Pasarela.PagoBase;

namespace Pagos.Pasarela.Eventos
{
    public class RespuestaGenericEventArgs<T> : EventArgs
    {
        private TipoRespuestaEvento _tipo;

        private object _respuesta;
        private object _parametroOriginal;
        private DelegateParamVoid<T> _delegate;

        public TipoRespuestaEvento TipoRespuesta
        {
            get { return _tipo; }
            set { _tipo = value; }
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

        public DelegateParamVoid<T> Delegate
        {
            get { return _delegate; }
            set { _delegate = value; }
        }

        public RespuestaGenericEventArgs(TipoRespuestaEvento xTipo, object xRespuesta, object xParametroOriginal)
        {
            _tipo = xTipo;
            _respuesta = xRespuesta;
            _parametroOriginal = xParametroOriginal;
        }

        public RespuestaGenericEventArgs(TipoRespuestaEvento xTipo, object xRespuesta, object xParametroOriginal, DelegateParamVoid<T> xDelegate)
        {
            _tipo = xTipo;
            _respuesta = xRespuesta;
            _parametroOriginal = xParametroOriginal;
            _delegate = xDelegate;
        }
    }
}
