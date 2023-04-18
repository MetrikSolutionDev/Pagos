using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela
{
    public class PagoFactory
    {
        public static IPago Instance(Configuracion xConfiguracion)
        {
            IPago sPago = null;

            switch (xConfiguracion.Tipo)
            {
                case Common.CommonPago.Tipo.PRISMA:

                    return new Prisma(xConfiguracion);

                case Common.CommonPago.Tipo.MERCADO_PAGO:

                    return new MercadoPago(xConfiguracion);
            }

            return sPago;
        }
    }
}
