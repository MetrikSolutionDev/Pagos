using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public static class CommonPago
    {
        public enum Tipo
        {
            NINGUNO,
            PRISMA,
            MERCADO_PAGO
        }

        public enum TipoAuthorization
        {
            Bearer,
            Basic
        }

        public enum MetodoImpresion
        {
            NO_FISCAL,
            FISCAL
        }

        public enum CopiasComprobantePago
        {
            NINGUNO,
            SOLO_COMERCIANTE,
            SOLO_CLIENTE,
            AMBOS
        }

        public enum MetodoOperacion
        {
            TARJETA,
            QR
        }
    }
}
