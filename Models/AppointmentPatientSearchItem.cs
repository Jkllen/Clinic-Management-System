using System;

namespace CruzNeryClinic.Models
{
    public class AppointmentPatientSearchItem
    {
        public int PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public bool IsPwd { get; set; }

        public bool IsSeniorCitizen { get; set; }

        public string FullName
        {
            get
            {
                string middle = string.IsNullOrWhiteSpace(MiddleName)
                    ? string.Empty
                    : $" {MiddleName.Trim()}";

                return $"{FirstName.Trim()}{middle} {LastName.Trim()}".Trim();
            }
        }

        public string Category
        {
            get
            {
                if (IsPwd)
                    return "PWD";

                if (IsSeniorCitizen)
                    return "Senior";

                return "Regular";
            }
        }

        public string DisplayText =>
            $"{PatientCode} - {FullName}";
    }
}