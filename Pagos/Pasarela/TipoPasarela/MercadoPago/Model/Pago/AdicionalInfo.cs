
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class AdicionalInfo
    {
        public string external_reference { get; set; }

        public bool print_on_terminal { get; set; }

        public string ticket_number { get; set; }
    }
}
