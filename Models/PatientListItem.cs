using System;

namespace CruzNeryClinic.Models
{
    public class PatientListItem
    {
        public int PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string DateOfBirthDisplay =>
            DateOfBirth.HasValue ? DateOfBirth.Value.ToString("MM/dd/yyyy") : "N/A";

        public string Gender { get; set; } = string.Empty;

        public string Treatment { get; set; } = string.Empty;

        public bool HasPatientHistory { get; set; }

        public bool IsPwd { get; set; }

        public bool IsSenior { get; set; }

        public bool HasBalance { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public string PwdStatusText =>
            IsPwd || IsSenior ? "YES" : "NO";

        public string PwdStatusBrush =>
            IsPwd || IsSenior ? "#00A423" : "#E31D1D";

        public string ArchiveButtonText =>
            IsActive ? "Archive" : "Restore";

        public string ArchiveButtonBrush =>
            IsActive ? "#E31D1D" : "#073C98";

        public string ArchiveButtonIcon =>
            IsActive ? "Archive" : "Undo";
    }
}