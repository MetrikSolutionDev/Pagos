using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.Eventos
{
    public class SolicitudPagoEventArgs : EventArgs
    {
        private bool _confirmado;

        public bool Confirmado
        {
            get { return _confirmado; }
            set { _confirmado = value; }
        }

        public SolicitudPagoEventArgs(bool xConfirmado)
        {
            _confirmado = xConfirmado;
        }
    }
}
