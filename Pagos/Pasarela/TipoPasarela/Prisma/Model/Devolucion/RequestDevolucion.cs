using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    internal class RequestDevolucion
    {
        public string subnet_acquirer_id { get; set; }

        public string ecr_transaction_id { get; set; }

        public string terminal_menu_text { get; set; }

        public string print_copies { get; set; }

        public string ecr_provider { get; set; }

        public string ecr_name { get; set; }

        public string ecr_version { get; set; }

        public string refund_amount { get; set; }

        /// <summary>
        /// VI = VISA, MC = MASTERCARD, AM = AMEX
        /// </summary>
        public string card_brand_product { get; set; }

        public List<Terminal> terminals_list { get; set; }

        public List<Errors> errors { get; set; }

        public SolicitudDevolucion Parametro_original { get; set; }

        public int Nro_persistencia { get; set; }

        public DateTime Inicio_persistencia { get; set; }
    }
}
