using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pagos.Pasarela.PrismaModel
{
    internal class Reversal_data
    {
        public string payment_id { get; set; }

        public Payment_request_data payment_request { get; set; }

        public Payment_data payment_data { get; set; }

        public string reversal_request_date { get; set; }

        public Enums.ReversalStatus reversal_status { get; set; }

        public string reversal_status_date { get; set; }
    }
}
