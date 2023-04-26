using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public delegate void RespuestaExternaHandler(object sender, RespuestaExternaEventArgs e);

    public class Configuracion
    {
        public string End_point { get; set; }

        public string Sub_end_point { get; set; }

        public string Sub_end_point_authorization { get; set; }

        public string Key { get; set; }

        public string Client_id { get; set; }

        public string Client_secret { get; set; }

        public string Code { get; set; }

        public string User_id { get; set; }

        public string Token { get; set; }

        public List<string> Id_terminales { get; set; }

        public CommonPago.Tipo Tipo { get; set; }

        public CommonPago.TipoIntegracion Tipo_integracion { get; set; }

        public int Cantidad_persistencias { get; set; }

        /// <summary>
        /// Si no se configura y esta en 0, por defecto va a tener 60 segundos
        /// </summary>
        public int Tiempo_segundos_persistencias { get; set; }
    }
}
