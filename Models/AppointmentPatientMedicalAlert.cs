namespace CruzNeryClinic.Models
{
    public class AppointmentPatientMedicalAlert
    {
        public bool HasMedicalCondition { get; set; }

        public string MedicalConditionNotes { get; set; } = string.Empty;

        public string AllergyNotes { get; set; } = string.Empty;

        public string CurrentMedication { get; set; } = string.Empty;

        public bool RequiresMedicalClearance { get; set; }

        public string ClearanceNotes { get; set; } = string.Empty;

        public bool HasAnyAlert =>
            HasMedicalCondition ||
            RequiresMedicalClearance ||
            !string.IsNullOrWhiteSpace(MedicalConditionNotes) ||
            !string.IsNullOrWhiteSpace(AllergyNotes) ||
            !string.IsNullOrWhiteSpace(CurrentMedication) ||
            !string.IsNullOrWhiteSpace(ClearanceNotes);
    }
}