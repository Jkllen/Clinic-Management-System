using System;

namespace CruzNeryClinic.Models
{
    public class BalancePaymentItem
    {
        public int BillingId { get; set; }

        public int PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string ReceiptNumber { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal RemainingBalance { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; }

        public string TransactionDateDisplay =>
            TransactionDate.ToString("MM/dd/yyyy");

        public string TotalAmountDisplay =>
            $"₱{TotalAmount:N2}";

        public string AmountPaidDisplay =>
            $"₱{AmountPaid:N2}";

        public string RemainingBalanceDisplay =>
            $"₱{RemainingBalance:N2}";
    }
}