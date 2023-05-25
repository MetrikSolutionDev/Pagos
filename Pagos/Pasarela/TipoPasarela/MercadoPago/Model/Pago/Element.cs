
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class Element
    {
        public long id { get; set; }

        public string status { get; set; }

        public string external_reference { get; set; }

        public string preference_id { get; set; }

        public List<Payment> payments { get; set; }
    }
}
