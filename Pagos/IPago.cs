using Pagos.Pasarela;
using Pagos.Pasarela.Eventos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public interface IPago : IPagos, IReversiones
    {
    }
}
