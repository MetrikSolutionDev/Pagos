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
    internal class TokenResponse
    {
        public string token_type { get; set; }

        public string access_token { get; set; }

        public string expires_in { get; set; }

        public string scope { get; set; }

        public List<Errors> errors { get; set; }
    }
}
