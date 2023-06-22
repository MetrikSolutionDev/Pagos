using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    internal class RequestCierre
    {
        public string subnet_acquirer_id { get; set; }

        public string print_receipt { get; set; }

        public List<Errors> errors { get; set; }

        public SolicitudCierre Parametro_original { get; set; }

        public int Nro_persistencia { get; set; }

        public DateTime Inicio_persistencia { get; set; }
    }
}
