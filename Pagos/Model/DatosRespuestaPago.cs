using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public class DatosRespuestaPago
    {
        public string Nro_lote { get; set; }

        public string Nro_cupon { get; set; }

        public string Descripcion_tarjeta { get; set; }

        /// <summary>
        /// VI = VISA, MC = MASTERCARD, AM = AMEX
        /// </summary>
        public string Tarjeta { get; set; }

        public string Nro_terminal { get; set; }
    }
}
