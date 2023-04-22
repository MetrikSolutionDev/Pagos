using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public class ConsultaEstadoPago
    {
        public string Pago_id { get; set; }

        public string Cuit_cuil { get; set; }

        public string Referencia { get; set; }
    }
}
