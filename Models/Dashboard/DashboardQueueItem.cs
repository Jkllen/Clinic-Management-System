namespace CruzNeryClinic.Models.Dashboard
{
    // Represents one appointment row shown in the Dashboard Queue section.
    public class DashboardQueueItem
    {
        public int QueueNumber { get; set; }

        public string Time { get; set; } = string.Empty;

        public string PatientCode { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string AppointmentType { get; set; } = string.Empty;

        public string Treatment { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}