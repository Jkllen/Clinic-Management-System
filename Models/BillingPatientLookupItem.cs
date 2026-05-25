namespace CruzNeryClinic.Models
{
    public class BillingPatientLookupItem
    {
        public int PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string Category { get; set; } = "Regular";
    }
}