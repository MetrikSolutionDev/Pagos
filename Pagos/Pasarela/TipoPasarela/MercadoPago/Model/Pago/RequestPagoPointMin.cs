
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class RequestPagoPointMin
    {   
        public AdicionalInfo additional_info { get; set; }

        public int amount { get; set; }
    }
}
