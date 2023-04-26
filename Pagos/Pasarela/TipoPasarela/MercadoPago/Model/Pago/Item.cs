
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class Item
    {
        public string sku_number { get; set; }

        public string category { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public decimal unit_price { get; set; }

        public decimal quantity { get; set; }

        public string unit_measure { get; set; }

        public decimal total_amount { get; set; }
    }
}
