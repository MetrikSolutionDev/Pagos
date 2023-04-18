using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    internal class Payment_request_data
    {
        public string subnet_acquirer_id { get; set; }

        public string payment_amount { get; set; }

        public string terminal_menu_text { get; set; }

        public string ecr_transaction_id { get; set; }

        public int installments_number { get; set; }

        public string ecr_provider { get; set; }

        public string ecr_name { get; set; }

        public string ecr_version { get; set; }

        public string change_amount { get; set; }

        public int bank_account_type { get; set; }

        public string payment_plan_id { get; set; }

        public Enums.PrintMethod print_method { get; set; }

        public Enums.PrintCopies print_copies { get; set; }
        
        public List<Terminal> terminals_list { get; set; }

        /// <summary>
        /// VI = VISA, MC = MASTERCARD, AM = AMEX
        /// </summary>
        public string card_brand_product { get; set; }

        public Enums.TerminalOPerationMethod terminal_operation_method { get; set; }

        public bool qr_benefit_code { get; set; }

        public string trx_receipt_notes { get; set; }

        public string card_holder_id { get; set; }
    }
}
