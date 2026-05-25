namespace CruzNeryClinic.Models
{
    public class AppointmentDentistOption
    {
        public int? DentistUserId { get; set; }

        public string DentistName { get; set; } = "Unassigned";

        public string DisplayText => DentistName;
    }
}