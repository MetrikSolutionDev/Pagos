
using Pagos.Pasarela.PrismaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public class Cancelacion
    {
        public string Cuit_cuil { get; set; }

        public CommonPago.TipoEntorno Entorno { get; set; }

        public string Pago_id { get; set; }

        public Cancelacion Parametro_original { get; set; }

        public int Nro_persistencia { get; set; }

        public int Nro_intento_generacion_token { get; set; }

        public DateTime Inicio_persistencia { get; set; }
    }
}
