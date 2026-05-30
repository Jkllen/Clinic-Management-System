using System;

namespace CruzNeryClinic.Models
{
    public class BillingTransactionItem
    {
        public int BillingItemId { get; set; }

        public int BillingId { get; set; }

        public int? AppointmentId { get; set; }

        public int? TreatmentRecordId { get; set; }

        public int? ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string ItemDescription { get; set; } = string.Empty;

        public DateTime? TreatmentDate { get; set; }

        public decimal Amount { get; set; }

        public bool IsIncluded { get; set; }

        public DateTime CreatedAt { get; set; }

        public string TreatmentDateDisplay =>
            TreatmentDate.HasValue
                ? TreatmentDate.Value.ToString("MM/dd/yyyy")
                : string.Empty;

        public string AmountDisplay =>
            IsIncluded
                ? "Included"
                : $"₱{Amount:N2}";

    }
}