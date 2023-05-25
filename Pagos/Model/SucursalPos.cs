
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public class SucursalPos
    {
        public string Id { get; set; }

        public string Descripcion { get; set; }

        public Configuracion Configuracion { get; set; }
    }
}
