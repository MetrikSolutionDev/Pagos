using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos
{
    public static class CommonPago
    {
        public enum TipoRespuestaExternaEvento
        {
            NINGUNO,
            ERROR_401,
            ERROR_400,
            ERROR_404,
            ERROR_409,
            ERROR_500,
            ERROR_503,
            SOLICITUD_ENVIADA,
            ESTADO_PAGO,
            CANCELACION_PAGO,
            PROCESO_EN_EJECUCION,
            TOKEN

        }
        public enum TipoRespuestaEvento
        {
            NINGUNO,
            TOKEN,
            SOLICITUD_PAGO,
            CONSULTA_ESTADO_PAGO,
            CANCELACION_PAGO,
            SOLICITUD_REVERSION,
            CONSULTA_ESTADO_REVERSION,
            CANCELACION_REVERSION,
            SOLICITUD_DEVOLUCION,
            CONSULTA_ESTADO_DEVOLUCION,
            CANCELACION_DEVOLUCION,
            SOLICITUD_CIERRE,
            CONSULTA_ESTADO_CIERRE
        }

        public enum Tipo
        {
            NINGUNO,
            PRISMA,
            MERCADO_PAGO
        }

        public enum TipoIntegracion
        {
            POINT,
            QR
        }

        public enum TipoEntorno
        {
            PRODUCCION,
            HOMOLOGACION,
            PRUEBA
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
