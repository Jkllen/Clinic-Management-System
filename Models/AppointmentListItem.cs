using System;
using System.Windows;

namespace CruzNeryClinic.Models
{
    public class AppointmentListItem
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string AppointmentType { get; set; } = string.Empty;

        public string Category { get; set; } = "Regular";

        public int? ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public int? DentistUserId { get; set; }

        public string DentistName { get; set; } = "Unassigned";

        public DateTime AppointmentDate { get; set; }

        public TimeSpan AppointmentTime { get; set; }

        public TimeSpan? ArrivalTime { get; set; }

        public int? QueueNumber { get; set; }

        public bool IsUrgent { get; set; }

        public string Priority { get; set; } = "Normal";

        public string Status { get; set; } = "Scheduled";

        public string Notes { get; set; } = string.Empty;

        public string QueueNumberDisplay =>
            QueueNumber.HasValue ? QueueNumber.Value.ToString() : "-";

        public string AppointmentDateDisplay =>
            AppointmentDate.ToString("MM/dd/yyyy");

        public string AppointmentTimeDisplay =>
            DateTime.Today.Add(AppointmentTime).ToString("hh:mm tt");

        public string PriorityDisplay =>
            IsUrgent ? "Urgent" : Priority;

        public string PriorityBrush =>
            IsUrgent ? "#E67E22" :
            Priority == "Scheduled" ? "#073C98" :
            "#555555";

        public string StatusBrush =>
            Status switch
            {
                "Scheduled" => "#073C98",
                "Waiting" => "#B8860B",
                "In Treatment" => "#8E44AD",
                "Completed" => "#00A423",
                "Cancelled" => "#E31D1D",
                "No Show" => "#777777",
                _ => "#555555"
            };

        public Visibility MarkArrivedButtonVisibility =>
            Status == "Scheduled" ? Visibility.Visible : Visibility.Collapsed;

        public Visibility StartTreatmentButtonVisibility =>
            Status == "Waiting" ? Visibility.Visible : Visibility.Collapsed;

        public Visibility CompleteButtonVisibility =>
            Status == "In Treatment" ? Visibility.Visible : Visibility.Collapsed;

        public Visibility CancelButtonVisibility =>
            Status is "Scheduled" or "Waiting" or "In Treatment"
                ? Visibility.Visible
                : Visibility.Collapsed;

        public Visibility UrgentButtonVisibility =>
            Status == "Waiting" ? Visibility.Visible : Visibility.Collapsed;

        public string UrgentButtonText =>
            IsUrgent ? "Remove Urgent" : "Urgent";

        public string UrgentButtonBrush =>
            IsUrgent ? "#777777" : "#E67E22";
    }
}