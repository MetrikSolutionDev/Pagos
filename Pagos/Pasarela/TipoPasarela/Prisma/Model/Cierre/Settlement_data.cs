using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.TipoPasarela.Prisma.Model.Cierre
{
    internal class Settlements_data
    {
        public string settlement_id { get; set; }

        public string terminal_id { get; set; }

        public string print_receipt { get; set; }

        public string merchant_id { get; set; }

        public string merchant_name { get; set; }

        public string cuit_cuil { get; set; }

        public string subnet_acquirer_id { get; set; }

        public string settlement_status { get; set; }

        public string settlement_status_date { get; set; }

        public string settlement_request_date { get; set; }

        public string terminal_serial { get; set; }

        public string terminal_model { get; set; }

        public string mobitef_app_vers { get; set; }

        public string acquirer_response_code { get; set; }

        public string acquirer_additional_message { get; set; }

        public string batch_number { get; set; }

        public string merchant_receipt_detail { get; set; }
    }
}
