using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    internal class Reversal_request_data
    {
        /// <summary>
        /// 1-Sandbox,2-Produccion,9-Homologacion
        /// </summary>
        public string subnet_acquirer_id { get; set; }

        public string payment_id { get; set; }

        public string ecr_transaction_id { get; set; }

        public string terminal_menu_text { get; set; }

        public Enums.PrintCopies print_copies { get; set; }
        
        public List<Terminal> terminals_list { get; set; }
    }
}
