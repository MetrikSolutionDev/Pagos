using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public class Configuracion
    {
        public string End_point { get; set; }

        public string Sub_end_point { get; set; }

        public string Sub_end_point_authorization { get; set; }

        public string Key { get; set; }

        public List<string> Id_equipos { get; set; }

        public CommonPago.Tipo Tipo { get; set; }

        public int Cantidad_persistencias_pago { get; set; }
    }
}
