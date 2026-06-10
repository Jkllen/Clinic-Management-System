using System;

namespace CruzNeryClinic.Models
{
    public class BillingRecordListItem
    {
        public int BillingId { get; set; }

        public int PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string ReceiptNumber { get; set; } = string.Empty;

        public string BillingSource { get; set; } = string.Empty;

        public string ServiceName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal SubtotalAfterDiscount { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal RemainingBalance { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;

        public bool IsArchived { get; set; }

        public bool IsActive => !IsArchived;

        public DateTime TransactionDate { get; set; }

        public string TransactionDateDisplay =>
            TransactionDate.ToString("MM/dd/yyyy");

        public string TotalAmountDisplay =>
            $"₱{TotalAmount:N2}";

        public string BalanceDisplay =>
            $"₱{RemainingBalance:N2}";
        public string ArchiveStatusDisplay =>
            IsArchived ? "Archived" : PaymentStatus;
    }
}
