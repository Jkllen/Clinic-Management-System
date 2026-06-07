using System;
using System.Collections.Generic;
using System.Linq;
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

        public string? ServiceStage { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public string TreatmentDetails { get; set; } = string.Empty;

        public int? DentistUserId { get; set; }

        public string DentistName { get; set; } = "Unassigned";

        public DateTime AppointmentDate { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

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

        public string ServiceStageDisplay =>
            string.IsNullOrWhiteSpace(ServiceStage) ? "-" : ServiceStage;

        public string FollowUpDateDisplay =>
            FollowUpDate.HasValue ? FollowUpDate.Value.ToString("MM/dd/yyyy") : "-";

        public string TreatmentDetailsDisplay =>
            string.IsNullOrWhiteSpace(TreatmentDetails)
                ? "No treatment details recorded."
                : TreatmentDetails;

        public List<AppointmentToothOption> ToothChart =>
            Enumerable.Range(1, 32)
                .Select(number => new AppointmentToothOption
                {
                    ToothNumber = number,
                    IsSelected = SelectedToothNumbers.Contains(number)
                })
                .ToList();

        public bool HasSelectedTeeth => SelectedToothNumbers.Count > 0;

        private List<int> SelectedToothNumbers
        {
            get
            {
                string line = TreatmentDetails
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .FirstOrDefault(value =>
                        value.StartsWith("Teeth involved:", StringComparison.OrdinalIgnoreCase) ||
                        value.StartsWith("Treated teeth:", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(line))
                    return new List<int>();

                return line
                    .Replace("Teeth involved:", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace("Treated teeth:", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Split(',')
                    .Select(value => int.TryParse(value.Trim(), out int number) ? number : 0)
                    .Where(number => number >= 1 && number <= 32)
                    .Distinct()
                    .OrderBy(number => number)
                    .ToList();
            }
        }

        public string StartedAtDisplay =>
            StartedAt.HasValue ? StartedAt.Value.ToString("MM/dd/yyyy hh:mm tt") : "-";

        public string CompletedAtDisplay =>
            CompletedAt.HasValue ? CompletedAt.Value.ToString("MM/dd/yyyy hh:mm tt") : "-";

        public string CancelledAtDisplay =>
            CancelledAt.HasValue ? CancelledAt.Value.ToString("MM/dd/yyyy hh:mm tt") : "-";

        public string NotesDisplay =>
            string.IsNullOrWhiteSpace(Notes)
                ? "No notes or remarks recorded."
                : Notes;

        public string PriorityDisplay
        {
            get
            {
                if (IsUrgent)
                    return "Urgent";

                if (HasAgingPriority)
                    return "Long Waiting";

                if (Priority == "Scheduled")
                    return "Scheduled";

                if (Category == "PWD")
                    return "PWD";

                if (Category == "Senior")
                    return "Senior";

                return "Normal";
            }
        }

        public bool HasPriorityCategory =>
            AppointmentType == "Walk-In" &&
            Status == "Waiting" &&
            Category is "PWD" or "Senior";

        public string PriorityBrush =>
            IsUrgent ? "#E67E22" :
            HasAgingPriority ? "#B8860B" :
            Priority == "Scheduled" ? "#073C98" :
            Category is "PWD" or "Senior" ? "#8E44AD" :
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
        public Visibility NoShowButtonVisibility =>
            Status == "Scheduled" ? Visibility.Visible : Visibility.Collapsed;

        public Visibility RescheduleButtonVisibility =>
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

        public string ArrivalTimeDisplay =>
            ArrivalTime.HasValue
                ? DateTime.Today.Add(ArrivalTime.Value).ToString("hh:mm tt")
                : "-";

        public bool HasAgingPriority =>
            Status == "Waiting" &&
            !IsUrgent &&
            WaitingMinutes >= 60;
        public int WaitingMinutes
        {
            get
            {
                if (AppointmentDate.Date != DateTime.Today)
                    return 0;

                if (Status != "Waiting")
                    return 0;

                TimeSpan startTime = ArrivalTime ?? AppointmentTime;
                TimeSpan currentTime = DateTime.Now.TimeOfDay;

                if (currentTime < startTime)
                    return 0;

                return (int)(currentTime - startTime).TotalMinutes;
            }
        }

        public string WaitingDurationDisplay
        {
            get
            {
                if (Status == "In Treatment")
                    return "In treatment";

                if (Status != "Waiting")
                    return "-";

                if (AppointmentDate.Date != DateTime.Today)
                    return "-";

                int minutes = WaitingMinutes;

                if (minutes <= 0)
                    return "0 min";

                if (minutes < 60)
                    return $"{minutes} min";

                int hours = minutes / 60;
                int remainingMinutes = minutes % 60;

                if (remainingMinutes == 0)
                    return $"{hours} hr";

                return $"{hours} hr {remainingMinutes} min";
            }
        }
    }


}
