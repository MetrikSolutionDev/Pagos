using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public class RespuestaConsultaEstadoPago
    {
        public bool Persistencia_finalizada { get; set; }

        public bool Confirmado { get; set; }

        public string Lote { get; set; }

        public int Cantidad_intentos_persistencia { get; set; }
    }
}
