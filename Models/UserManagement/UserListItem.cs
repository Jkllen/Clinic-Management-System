namespace CruzNeryClinic.Models.UserManagement
{
    // This model is used only for displaying users in the User Management table.
    // It prevents the UI from directly exposing password hashes, salts, and security answers.
    public class UserListItem
    {
        public int UserId { get; set; }

        public string UserCode { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;
        
        public string FirstName { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int? CreatedByUserId { get; set; }

        public string CreatedByDisplay { get; set; } = "System / Existing Data";

        public System.DateTime CreatedAt { get; set; }

        public System.DateTime? UpdatedAt { get; set; }

        public System.DateTime? LastLoginAt { get; set; }

        public string AccountStatus => IsActive ? "Active" : "Archived";

        public bool IsArchived => !IsActive;

        public string FullName =>
            string.IsNullOrWhiteSpace(MiddleName)
                ? $"{FirstName} {LastName}".Trim()
                : $"{FirstName} {MiddleName} {LastName}".Trim();

        public string CreatedAtDisplay => CreatedAt.ToString("MM/dd/yyyy hh:mm tt");

        public string UpdatedAtDisplay => UpdatedAt.HasValue ? UpdatedAt.Value.ToString("MM/dd/yyyy hh:mm tt") : "-";

        public string LastLoginAtDisplay => LastLoginAt.HasValue ? LastLoginAt.Value.ToString("MM/dd/yyyy hh:mm tt") : "-";
    }
}
