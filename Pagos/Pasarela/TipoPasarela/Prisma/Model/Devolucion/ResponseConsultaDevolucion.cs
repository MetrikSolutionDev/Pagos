using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    internal class ResponseConsultaDevolucion
    {
        public RequestCierre refund_request_data { get; set; }

        public Refund_data refund_data { get; set; }

        public List<Errors> errors { get; set; }
    }
}
