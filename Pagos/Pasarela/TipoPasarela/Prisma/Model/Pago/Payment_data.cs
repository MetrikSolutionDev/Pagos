using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pagos.Pasarela.PrismaModel
{
    internal class Payment_data
    {
        public string payment_id { get; set; }

        public string payment_request_date { get; set; }

        public Enums.PaymentStatus payment_status { get; set; }

        public string payment_status_date { get; set; }

        public string payment_type { get; set; }

        public string merchant_id { get; set; }

        public string merchant_name { get; set; }

        public string cuit_cuil { get; set; }

        public string terminal_id { get; set; }

        public string terminal_serial { get; set; }

        public string terminal_model { get; set; }

        public string mobitef_app_vers { get; set; }

        public string authorized_payment_amount { get; set; }

        public string change_amount { get; set; }

        public int installments_number { get; set; }

        public int bank_account_type { get; set; }

        public string payment_plan_id { get; set; }

        public Enums.Acquirer acquirer_id { get; set; }

        public string card_brand_product { get; set; }

        public Enums.CardType card_type { get; set; }

        public string card_brand { get; set; }

        public string bin { get; set; }

        public string last_four_digits { get; set; }

        public string card_holder { get; set; }

        public int client_transaction_id { get; set; }

        public int transaction_receipt { get; set; }

        public int batch_number { get; set; }

        public string qr_code_id { get; set; }

        public string qr_original_amount { get; set; }

        public string acquirer_response_code { get; set; }

        public string acquirer_additional_message { get; set; }

        public string acquirer_response_date { get; set; }

        public string authorization_code { get; set; }

        public string client_receipt_detail { get; set; }

        public string merchant_receipt_detail { get; set; }

        public string payment_auth_nsu { get; set; }

        public string trx_receipt_notes { get; set; }

        public string card_holder_id { get; set; }

        public string tax_receipt_content { get; set; }

        public string order_notes { get; set; }
    }
}
