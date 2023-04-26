
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class RequestPagoQr
    {
        public string user_id { get; set; }

        public string external_store_id { get; set; }

        public string external_pos_id { get; set; }

        public CashOut cash_out { get; set; }

        public string description { get; set; }

        public string expiration_date { get; set; }

        public string external_reference { get; set; }

        public List<Item> items { get; set; }

        public string notification_url { get; set; }

        public Sponsor sponsor { get; set; }

        public List<Taxes> taxes { get; set; }

        public string title { get; set; }

        public decimal total_amount { get; set; }

        public SolicitudPago Parametro_original { get; set; }

        public int Nro_persistencia { get; set; }

        public DateTime Inicio_persistencia { get; set; }
    }
}
