using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    internal class RequestReversion
    {
        public Reversal_request_data reversal_request_data { get; set; }

        public Reversal_data reversal_data { get; set; }

        public List<Errors> errors { get; set; }

        public SolicitudReversion Parametro_original { get; set; }

        public int Nro_persistencia { get; set; }

        public DateTime Inicio_persistencia { get; set; }
    }
}
