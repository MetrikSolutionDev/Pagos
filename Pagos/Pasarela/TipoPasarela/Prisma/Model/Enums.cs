using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    public static class Enums
    {
        public enum TipoRespuestaEvento
        {
            NINGUNO,
            TOKEN,
            SOLICITUD_PAGO,
            CONSULTA_ESTADO_PAGO
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
