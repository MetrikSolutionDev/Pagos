using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class TokenResponse
    {
        public string token_type { get; set; }

        public string access_token { get; set; }

        public int expires_in { get; set; }

        public string scope { get; set; }

        public int user_id { get; set; }

        public string refresh_token { get; set; }

        public string public_key { get; set; }

        public bool live_mode { get; set; }

        public List<Errors> errors { get; set; }
    }
}
