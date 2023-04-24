using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    public static class Enums
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
            PROCESO_EN_EJECUCION

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
            CANCELACION_REVERSION
        }

        public enum PrintMethod 
        {
            MOBITEF_NON_FISCAL,
            MOBITEF_FISCAL
        }

        public enum PrintCopies
        {
            MERCHANT_ONLY,
            CUSTOMER_ONLY,
            BOTH,
            NONE
        }

        public enum TerminalOPerationMethod
        {
            CARD,
            QR_CODE
        }

        public enum PaymentStatus
        {
            PAYMENT_REQUEST,
            PROCESSING_PAYMENT,
            WAITING_CONFIRMATION,
            CONFIRM_REQUEST,
            UNDO_REQUEST,
            PROCESSING_CONFIRMATION,
            PROCESSING_UNDO,
            CONFIRMED,
            DECLINED,
            UNDONE,
            ERROR
        }

        public enum ReversalStatus
        {
            REVERSAL_REQUEST,
            PROCESSING_REVERSAL,
            REVERSED,
            REVERSAL_ERROR,
            REVERSAL_DECLINED,
            UNDO_REVERSAL_REQUEST,
            PROCESSING_UNDO_REVERSAL,
            REVERSAL_UNDONE,
            UNDO_REVERSAL_DECLINED
        }

        public enum Acquirer
        {
            PRISMA,
            AMEX
        }

        public enum CardType
        {
            MANUAL, 
            MAGNETIC_STRIPE, 
            CHIP, 
            FALLBACK_MANUAL, 
            FALLBACK_MAGNETIC_STRIPE, 
            CHIP_CONTACTLESS, 
            MAGNETIC_STRIP_CONTACTLESS, 
            QR_CODE
        }
    }
}
