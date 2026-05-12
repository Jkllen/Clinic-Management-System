using System;

namespace CruzNeryClinic.Models
{
    // This model represents one user account from the Users table.
    // It is used for login, user management, role-based access, and activity logs.
    public class User
    {
        public int UserId { get; set; }

        // Visible user ID shown in the UI, example: 2026-001.
        public string UserCode { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        // Stored hashed password, not the real password.
        public string PasswordHash { get; set; } = string.Empty;

        // Salt used when hashing the password.
        public string PasswordSalt { get; set; } = string.Empty;

        // Example roles: Admin, Dentist, Secretary, Dental Assistant.
        public string Role { get; set; } = string.Empty;

        // These are foreign keys from the SecurityQuestions table.
        public int SecurityQuestionId1 { get; set; }
        public int SecurityQuestionId2 { get; set; }
        public int SecurityQuestionId3 { get; set; }

        // These are the actual question texts loaded through JOIN queries.
        // They are displayed in the Security Questions screen.
        public string SecurityQuestion1 { get; set; } = string.Empty;
        public string SecurityQuestion2 { get; set; } = string.Empty;
        public string SecurityQuestion3 { get; set; } = string.Empty;

        // Security answers are stored as hashes and salts.
        // The real answer text is never stored.
        public string SecurityAnswerHash1 { get; set; } = string.Empty;
        public string SecurityAnswerSalt1 { get; set; } = string.Empty;

        public string SecurityAnswerHash2 { get; set; } = string.Empty;
        public string SecurityAnswerSalt2 { get; set; } = string.Empty;

        public string SecurityAnswerHash3 { get; set; } = string.Empty;
        public string SecurityAnswerSalt3 { get; set; } = string.Empty;

        // 1 in database means active, 0 means archived/deactivated.
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        // Helper property for displaying the full name in the sidebar or tables.
        public string FullName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MiddleName))
                    return $"{FirstName} {LastName}";

                return $"{FirstName} {MiddleName} {LastName}";
            }
        }
    }
}