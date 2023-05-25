
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class RequestPagoQrMin //: Errores
    {
        public string external_reference { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public decimal total_amount { get; set; }

        public List<Item> items { get; set; }
    }
}
