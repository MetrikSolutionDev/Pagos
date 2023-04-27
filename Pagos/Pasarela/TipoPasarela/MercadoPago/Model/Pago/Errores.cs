
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class Errores
    {
        public string message { get; set; }

        public string error { get; set; }

        public string status { get; set; }
    }
}
