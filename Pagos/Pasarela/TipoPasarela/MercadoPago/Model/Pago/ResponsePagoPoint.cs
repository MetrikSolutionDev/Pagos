
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.MercadoPagoModel
{
    internal class ResponsePagoPoint : Errores
    {
        public string id { get; set; }

        public string device_id { get; set; }

        public string state { get; set; }

        public Payment payment { get; set; }

        public AdicionalInfo additional_info { get; set; }

        public int amount { get; set; }

        public SolicitudPago Parametro_original { get; set; }

        public int Nro_persistencia { get; set; }

        public DateTime Inicio_persistencia { get; set; }
    }
}
