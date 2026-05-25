namespace CruzNeryClinic.Models
{
    public class ServiceItem
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public double DefaultPrice { get; set; }

        public bool IsActive { get; set; }

        public override string ToString()
        {
            return ServiceName;
        }
    }
}