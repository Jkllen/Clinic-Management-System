using System;

namespace CruzNeryClinic.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public string AppointmentType { get; set; } = string.Empty;

        public string Category { get; set; } = "Regular";

        public int? ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public int? DentistUserId { get; set; }

        public string DentistName { get; set; } = "Unassigned";

        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        public TimeSpan AppointmentTime { get; set; } = DateTime.Now.TimeOfDay;

        public TimeSpan? ArrivalTime { get; set; }

        public int? QueueNumber { get; set; }

        public bool IsUrgent { get; set; }

        public string Priority { get; set; } = "Normal";

        public string Status { get; set; } = "Scheduled";

        public string Notes { get; set; } = string.Empty;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string CancellationReason { get; set; } = string.Empty;

        public int? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}