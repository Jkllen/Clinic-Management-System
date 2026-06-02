namespace CruzNeryClinic.Models.Dashboard
{
    // A single suggestion shown in the Dashboard global search popup.
    // It represents either a patient or a user, and carries the information
    // needed to navigate to the matching module when clicked.
    public class DashboardSearchResultItem
    {
        // "Patient" or "User".
        public string ResultType { get; set; } = string.Empty;

        // Module to navigate to when selected: "Patients" or "ManageUsers".
        public string TargetModule { get; set; } = string.Empty;

        // Value used to pre-fill the target module's search box (the record code).
        public string SearchKey { get; set; } = string.Empty;

        // Primary line, e.g. "P-0001 - Dela Cruz, Juan M.".
        public string DisplayText { get; set; } = string.Empty;

        // Secondary line, e.g. "Patient" or the user's role.
        public string Subtitle { get; set; } = string.Empty;
    }
}
