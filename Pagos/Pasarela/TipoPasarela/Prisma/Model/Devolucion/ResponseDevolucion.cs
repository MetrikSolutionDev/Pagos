using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    internal class ResponseDevolucion
    {
        public RequestDevolucion refund_request { get; set; }

        public Refund_data refund_data { get; set; }

        public List<Errors> errors { get; set; }

        public SolicitudDevolucion Parametro_original { get; set; }

        public int Nro_persistencia { get; set; }

        public DateTime Inicio_persistencia { get; set; }
    }
}
