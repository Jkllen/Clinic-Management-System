namespace CruzNeryClinic.Models
{
    public class AppointmentServiceOption
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public double DefaultPrice { get; set; }

        public string DisplayText => ServiceName;
    }
}