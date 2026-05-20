using System;

namespace CruzNeryClinic.Models
{
    public class PaymentRecord
    {
        public int PaymentRecordId { get; set; }

        public int BillingId { get; set; }

        public int PatientId { get; set; }

        public decimal AmountPaid { get; set; }

        public string PaymentMethod { get; set; } = "Cash";

        public DateTime PaymentDate { get; set; } = DateTime.Today;

        public int? ReceivedByUserId { get; set; }

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}