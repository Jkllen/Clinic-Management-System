namespace CruzNeryClinic.Models.Dashboard
{
    // DashboardSummary stores the main number values displayed on the dashboard.
    // These values come from Patients, Appointments, BillingTransactions, and InventoryItems.
    public class DashboardSummary
    {
        public int TotalPatients { get; set; }

        public int NewPatientsThisMonth { get; set; }

        public int PendingPayments { get; set; }

        public decimal TotalUnpaidBalance { get; set; }

        public int LowStockItemCount { get; set; }
    }
}