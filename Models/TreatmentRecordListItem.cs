using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CruzNeryClinic.Models
{
    public class TreatmentRecordListItem
    {
        public int TreatmentRecordId { get; set; }

        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string DentistName { get; set; } = "Unassigned";

        public DateTime TreatmentDate { get; set; }

        public TimeSpan? TreatmentTime { get; set; }

        public string TreatmentNotes { get; set; } = string.Empty;

        public string? ServiceStage { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public string TreatmentDetails { get; set; } = string.Empty;

        public ObservableCollection<AppointmentImageItem> TeethImages { get; } = new();

        public string TreatmentDateDisplay =>
            TreatmentDate.ToString("MM/dd/yyyy");

        public string TreatmentTimeDisplay =>
            TreatmentTime.HasValue
                ? DateTime.Today.Add(TreatmentTime.Value).ToString("hh:mm tt")
                : "-";

        public string TreatmentNotesDisplay =>
            string.IsNullOrWhiteSpace(TreatmentNotes)
                ? "No treatment notes recorded."
                : TreatmentNotes;

        public string ServiceStageDisplay =>
            string.IsNullOrWhiteSpace(ServiceStage) ? "-" : ServiceStage;

        public string FollowUpDateDisplay =>
            FollowUpDate.HasValue ? FollowUpDate.Value.ToString("MM/dd/yyyy") : "-";

        public string TreatmentDetailsDisplay =>
            string.IsNullOrWhiteSpace(TreatmentDetails)
                ? "No treatment details recorded."
                : TreatmentDetails;

        public bool HasTeethImages => TeethImages.Count > 0;

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
    }
}
