using System;

namespace CruzNeryClinic.Models
{
    // This model represents one selectable security question.
    // These questions will be shown in dropdowns during user registration later.
    public class SecurityQuestion
    {
        public int SecurityQuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}