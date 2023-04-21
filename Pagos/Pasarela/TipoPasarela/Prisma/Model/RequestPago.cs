using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    internal class RequestPago
    {
        public Payment_request_data payment_request_data { get; set; }

        public Payment_data payment_data { get; set; }

        public List<Errors> errors { get; set; }
    }
}
