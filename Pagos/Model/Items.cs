using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public class Items
    {
        public string Codigo { get; set; }

        public string Categoria { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public decimal Precio_unitario { get; set; }

        public decimal Cantidad { get; set; }

        public string Unidad_medida { get; set; }

        public decimal Total { get; set; }
    }
}
