using Pagos.Pasarela.Eventos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pagos.Pasarela.PagoBase;

namespace Pagos.Pasarela
{
    public interface IAutenticacion
    {
        void RenovarToken();

        //async Task<string> RenovarTokenSync();
    }
}
