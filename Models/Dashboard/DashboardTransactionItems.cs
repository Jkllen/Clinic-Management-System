namespace CruzNeryClinic.Models.Dashboard
{
    // Represents one recent billing/payment transaction shown on the dashboard.
    public class DashboardTransactionItem
    {
        public string Time { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string Service { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;
    }
}