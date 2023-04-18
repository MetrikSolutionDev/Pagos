using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    internal class RequestPago
    {
        public string cuit_cuil { get; set; }

        public Payment_request_data payment_request_data { get; set; }

        public Payment_data payment_data { get; set; }
    }
}
