using System;

namespace CruzNeryClinic.Models
{
    public class AppointmentPaymentItem
    {
        public int TreatmentRecordId { get; set; }

        public int? AppointmentId { get; set; }

        public int PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public int? ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string DentistName { get; set; } = "Unassigned";

        public DateTime TreatmentDate { get; set; }

        public TimeSpan? TreatmentTime { get; set; }

        public decimal DefaultPrice { get; set; }

        public string TreatmentDateDisplay =>
            TreatmentDate.ToString("MM/dd/yyyy");

        public string TreatmentTimeDisplay =>
            TreatmentTime.HasValue
                ? DateTime.Today.Add(TreatmentTime.Value).ToString("hh:mm tt")
                : "-";

        public string DefaultPriceDisplay =>
            $"₱{DefaultPrice:N2}";
    }
}