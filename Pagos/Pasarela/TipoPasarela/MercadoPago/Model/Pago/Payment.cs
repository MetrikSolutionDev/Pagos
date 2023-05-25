
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class Payment
    {
        public long id { get; set; }

        public decimal transaction_amount { get; set; }

        public decimal total_paid_amount { get; set; }

        public decimal shipping_cost { get; set; }

        public string currency_id { get; set; }

        public string status { get; set; }

        public string status_detail { get; set; }

        public string operation_type { get; set; }

        public DateTime date_approved { get; set; }

        public DateTime date_created { get; set; }

        public DateTime last_modified { get; set; }

        public decimal amount_refunded { get; set; }
    }
}
