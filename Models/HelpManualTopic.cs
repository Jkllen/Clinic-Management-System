namespace CruzNeryClinic.Models
{
    // Represents one topic/section in the Help user manual.
    // Example topics:
    // - Login and Logout
    // - User Management
    // - Patient Management
    // - Appointment Scheduling
    // - Billing
    // - Inventory
    // - Reports
    // - Backup and Restore
    public class HelpManualTopic
    {
        #region Properties

        // TODO:
        // Set this to the title of the manual section.
        // Example: "Login and Logout"
        public string Title { get; set; } = string.Empty;

        // TODO:
        // Set this to the actual instruction content.
        // This should explain the step-by-step process for the user.
        public string Content { get; set; } = string.Empty;

        // TODO:
        // Optional later:
        // Add DisplayOrder if manual topics need to be arranged.
        // public int DisplayOrder { get; set; }

        #endregion
    }
}