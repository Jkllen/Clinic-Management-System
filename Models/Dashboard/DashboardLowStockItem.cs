namespace CruzNeryClinic.Models.Dashboard
{
    // Represents one low-stock inventory item displayed in the dashboard card.
    public class DashboardLowStockItem
    {
        public string ItemName { get; set; } = string.Empty;

        public int QuantityLeft { get; set; }
    }
}