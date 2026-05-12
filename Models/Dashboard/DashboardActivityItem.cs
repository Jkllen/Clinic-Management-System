namespace CruzNeryClinic.Models.Dashboard
{
    // Represents one recent user activity log shown on the dashboard.
    public class DashboardActivityItem
    {
        public string Action { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Time { get; set; } = string.Empty;
    }
}