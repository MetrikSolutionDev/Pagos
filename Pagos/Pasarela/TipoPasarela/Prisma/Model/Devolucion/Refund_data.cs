using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    internal class Refund_data
    {
        public string refund_id { get; set; }

        public string refund_request_date { get; set; }

        public Enums.RefundStatus refund_status { get; set; }

        public string refund_status_date { get; set; }

        public string merchant_id { get; set; }

        public string merchant_name { get; set; }

        public string cuit_cuil { get; set; }

        public string terminal_id { get; set; }

        public string terminal_serial { get; set; }

        public string terminal_model { get; set; }

        public string mobitef_app_vers { get; set; }

        public Enums.Acquirer acquirer_id { get; set; }

        public string card_brand_product { get; set; }

        public int refund_client_id { get; set; }

        public string refunds_transaction_receipt { get; set; }

        public string batch_number { get; set; }

        public string acquirer_response_code { get; set; }

        public string acquirer_response_date { get; set; }

        public string authorization_code { get; set; }

        public string client_receipt_detail { get; set; }

        public string merchant_receipt_detail { get; set; }

        public string refund_auth_nsu { get; set; }

        public Errors error { get; set; }
    }
}
