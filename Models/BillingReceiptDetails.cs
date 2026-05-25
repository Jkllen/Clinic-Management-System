using System;

namespace CruzNeryClinic.Models
{
    public class BillingReceiptDetail
    {
        public int BillingId { get; set; }

        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        public int? TreatmentRecordId { get; set; }

        public string ReceiptNumber { get; set; } = string.Empty;

        public string BillingSource { get; set; } = string.Empty;

        public string PatientCode { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string PatientCategory { get; set; } = "Regular";

        public string ServiceName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string DiscountType { get; set; } = "None";

        public decimal DiscountAmount { get; set; }
        public decimal VatExemptSales { get; set; }
    
        public bool HasVatExemption => DiscountType is "PWD" or "Senior Citizen" or "PWD/Senior";

        public decimal SubtotalAfterDiscount { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal RemainingBalance { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "Cash";

        public DateTime TransactionDate { get; set; }

        public DateTime? LatestPaymentDate { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string ReceiptNumberDisplay => ReceiptNumber;

        public string TransactionDateDisplay =>
            TransactionDate == DateTime.MinValue
                ? string.Empty
                : TransactionDate.ToString("MM/dd/yyyy");

        public string LatestPaymentDateDisplay =>
            LatestPaymentDate.HasValue
                ? LatestPaymentDate.Value.ToString("MM/dd/yyyy")
                : "N/A";

        public string TotalAmountDisplay => $"₱{TotalAmount:N2}";

        public string DiscountAmountDisplay => $"₱{DiscountAmount:N2}";

        public string SubtotalAfterDiscountDisplay => $"₱{SubtotalAfterDiscount:N2}";

        public string AmountPaidDisplay => $"₱{AmountPaid:N2}";

        public string RemainingBalanceDisplay => $"₱{RemainingBalance:N2}";

        public string VatExemptSalesDisplay => $"₱{VatExemptSales:N2}";
    }
}