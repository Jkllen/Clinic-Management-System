namespace CruzNeryClinic.Models.Inventory
{
    public class InventoryItem
    {
        public int RawItemId { get; set; }
        public string ItemId => $"INV-{RawItemId:D3}";

        public string ItemName { get; set; } = string.Empty;
        public int QuantityInStock { get; set; }
        public decimal UnitPrice { get; set; }
        public int MinimumStockLevel { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastRestock { get; set; }
        public string UpdatedAt { get; set; } = string.Empty;
        public string ItemCreated { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        // Computed properties used by the DataGrid columns
        public int Stock => QuantityInStock;

        public string Status =>
            QuantityInStock == 0 ? "Out of Stock" :
            QuantityInStock <= MinimumStockLevel ? "Low Stock" :
            "In Stock";
    }
}
