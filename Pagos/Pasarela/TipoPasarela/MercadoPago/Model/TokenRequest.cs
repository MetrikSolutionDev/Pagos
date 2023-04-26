using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class TokenRequest
    {
        public string client_id { get; set; }

        public string client_secret { get; set; }

        /// <summary>
        /// Obligatorio cuando el grant_type = authorization_code
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// authorization_code, refresh_token
        /// </summary>
        public string grant_type { get; set; }

        public string redirect_uri { get; set; }

        public string refresh_token { get; set; }

        public string test_token { get; set; }

        public List<Errors> errors { get; set; }
    }
}
