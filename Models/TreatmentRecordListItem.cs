using System;

namespace CruzNeryClinic.Models
{
    public class TreatmentRecordListItem
    {
        public int TreatmentRecordId { get; set; }

        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string DentistName { get; set; } = "Unassigned";

        public DateTime TreatmentDate { get; set; }

        public TimeSpan? TreatmentTime { get; set; }

        public string TreatmentNotes { get; set; } = string.Empty;

        public string TreatmentDateDisplay =>
            TreatmentDate.ToString("MM/dd/yyyy");

        public string TreatmentTimeDisplay =>
            TreatmentTime.HasValue
                ? DateTime.Today.Add(TreatmentTime.Value).ToString("hh:mm tt")
                : "-";

        public string TreatmentNotesDisplay =>
            string.IsNullOrWhiteSpace(TreatmentNotes)
                ? "No treatment notes recorded."
                : TreatmentNotes;
    }
}