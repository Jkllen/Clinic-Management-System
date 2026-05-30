using System;

namespace CruzNeryClinic.Models
{
    public class BillingTransaction
    {
        public int BillingId { get; set; }

        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        public int? TreatmentRecordId { get; set; }

        public string BillingSource { get; set; } = "Manual";

        public string ReceiptNumber { get; set; } = string.Empty;

        public int? ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string DiscountType { get; set; } = "None";

        public decimal DiscountAmount { get; set; }

        public decimal SubtotalAfterDiscount { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal RemainingBalance { get; set; }

        public string PaymentStatus { get; set; } = "Unpaid";

        public DateTime TransactionDate { get; set; } = DateTime.Today;

        public int? CreatedByUserId { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string InvoiceTitle { get; set; } = string.Empty;

        public bool IsInvoiceOpen { get; set; } = true;
    }
}