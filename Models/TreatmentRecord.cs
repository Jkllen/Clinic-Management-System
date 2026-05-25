using System;

namespace CruzNeryClinic.Models
{
    public class TreatmentRecord
    {
        public int TreatmentRecordId { get; set; }

        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        public int? ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public int? DentistUserId { get; set; }

        public string DentistName { get; set; } = "Unassigned";

        public DateTime TreatmentDate { get; set; } = DateTime.Today;

        public TimeSpan? TreatmentTime { get; set; }

        public string TreatmentNotes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}