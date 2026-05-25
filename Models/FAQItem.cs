namespace CruzNeryClinic.Models
{
    // Represents one frequently asked question in the Help screen.
    public class FaqItem
    {
        #region Properties

        // TODO:
        // Set this to the question shown to the user.
        // Example: "How do I reset my password?"
        public string Question { get; set; } = string.Empty;

        // TODO:
        // Set this to the answer/instruction for the question.
        public string Answer { get; set; } = string.Empty;

        // TODO:
        // Optional later:
        // Add DisplayOrder if FAQs need sorting.
        // public int DisplayOrder { get; set; }

        #endregion
    }
}