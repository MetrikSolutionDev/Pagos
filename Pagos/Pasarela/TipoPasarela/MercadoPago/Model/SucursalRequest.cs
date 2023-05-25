using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class SucursalRequest
    {
        public string external_id { get; set; }

        public string name { get; set; }

        public Location location { get; set; }
    }
}
