using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagos.Pasarela.PrismaModel
{
    public static class Enums
    {
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

        public enum RefundStatus
        {
            REFUND_REQUEST,
            PROCESSING_REFUND,
            CONFIRMED,
            UNDO_REQUEST,
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

        public enum SettlemnetsStatus
        {
            SETTLEMENT_REQUEST,
            PROCESSING_SETTLEMENT,
            CONFIRMED,
            DECLINED,
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
