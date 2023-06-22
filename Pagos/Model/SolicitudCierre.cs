
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public class SolicitudCierre
    {
        public string Cuit_cuil { get; set; }

        public string Terminal { get; set; }

        public bool Imprimir_comprobante { get; set; }

        public int Nro_intento_generacion_token { get; set; }

        public DateTime Inicio_persistencia { get; set; }
    }
}
