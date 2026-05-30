using CruzNeryClinic.Data;
using CruzNeryClinic.Models.Inventory;
using CruzNeryClinic.Services;
using CruzNeryClinic.ViewModels;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace CruzNeryClinic.Repositories
{
    public class InventoryRepository
    {
        public List<InventoryItem> GetAllItems()
            => QueryItems("WHERE IsActive = 1");

        public List<InventoryItem> GetArchivedItems()
            => QueryItems("WHERE IsActive = 0");

        private List<InventoryItem> QueryItems(string whereClause)
        {
            var items = new List<InventoryItem>();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                $"SELECT ItemId, ItemName, Quantity, UnitPrice, MinimumThreshold, IsActive, " +
                $"LastRestock, UpdatedAt, ItemCreated, Note FROM InventoryItems {whereClause};";

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new InventoryItem
                {
                    RawItemId         = reader.GetInt32(0),
                    ItemName          = reader.GetString(1),
                    QuantityInStock   = reader.GetInt32(2),
                    UnitPrice         = Convert.ToDecimal(reader.GetDouble(3)),
                    MinimumStockLevel = reader.GetInt32(4),
                    IsActive          = reader.GetInt32(5) == 1,
                    LastRestock       = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                    UpdatedAt         = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    ItemCreated       = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    Note              = reader.IsDBNull(9) ? "" : reader.GetString(9)
                });
            }

            return items;
        }

        public string GetNextItemId()
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT IFNULL(MAX(ItemId), 0) + 1 FROM InventoryItems;";
            int nextId = Convert.ToInt32(command.ExecuteScalar());
            return $"INV-{nextId:D3}";
        }

        public void AddItem(string itemName, int quantity, double unitPrice, int minimumThreshold, string stockStatus, string note, string timestamp)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO InventoryItems (ItemName, Quantity, UnitPrice, MinimumThreshold, [Stock Status], IsActive, ItemCreated, Note)
                VALUES (@ItemName, @Quantity, @UnitPrice, @MinimumThreshold, @StockStatus, 1, @ItemCreated, @Note);";

            command.Parameters.AddWithValue("@ItemName", itemName);
            command.Parameters.AddWithValue("@Quantity", quantity);
            command.Parameters.AddWithValue("@UnitPrice", unitPrice);
            command.Parameters.AddWithValue("@MinimumThreshold", minimumThreshold);
            command.Parameters.AddWithValue("@StockStatus", stockStatus);
            command.Parameters.AddWithValue("@ItemCreated", timestamp);
            command.Parameters.AddWithValue("@Note", note);
            command.ExecuteNonQuery();

            ActivityLogService.Log(
                "Create",
                "Inventory",
                $"Added inventory item '{itemName}' (quantity {quantity})");
        }

        public void UpdateItem(int itemId, string itemName, int quantity, double unitPrice, int minimumThreshold, string stockStatus, string note, string timestamp)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE InventoryItems
                SET ItemName = @ItemName,
                    Quantity = @Quantity,
                    UnitPrice = @UnitPrice,
                    MinimumThreshold = @MinimumThreshold,
                    [Stock Status] = @StockStatus,
                    Note = @Note,
                    UpdatedAt = @UpdatedAt
                WHERE ItemId = @ItemId;";

            command.Parameters.AddWithValue("@ItemName", itemName);
            command.Parameters.AddWithValue("@Quantity", quantity);
            command.Parameters.AddWithValue("@UnitPrice", unitPrice);
            command.Parameters.AddWithValue("@MinimumThreshold", minimumThreshold);
            command.Parameters.AddWithValue("@StockStatus", stockStatus);
            command.Parameters.AddWithValue("@Note", note);
            command.Parameters.AddWithValue("@UpdatedAt", timestamp);
            command.Parameters.AddWithValue("@ItemId", itemId);
            command.ExecuteNonQuery();

            ActivityLogService.Log(
                "Update",
                "Inventory",
                $"Updated inventory item '{itemName}'");
        }

        public void ArchiveItem(int itemId, string timestamp)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE InventoryItems
                SET IsActive = 0,
                    UpdatedAt = @UpdatedAt
                WHERE ItemId = @ItemId;";

            command.Parameters.AddWithValue("@ItemId", itemId);
            command.Parameters.AddWithValue("@UpdatedAt", timestamp);
            command.ExecuteNonQuery();

            ActivityLogService.Log(
                "Archive",
                "Inventory",
                $"Archived inventory item '{GetItemName(itemId)}'");
        }

        public void RestockItem(int itemId, string itemName, int quantityAdded, string restockedDate,
            string supplier, double unitPrice, string note, int currentQuantity, int minimumThreshold)
        {
            int newQuantity = currentQuantity + quantityAdded;
            string stockStatus = newQuantity == 0 ? "Out of Stock"
                : newQuantity <= minimumThreshold ? "Low Stock"
                : "In Stock";
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();
            using SqliteTransaction tx = connection.BeginTransaction();

            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO InventoryRestocks (ItemId, ItemName, QuantityAdded, RestockedDate, Supplier, UnitPrice, Note)
                    VALUES (@ItemId, @ItemName, @QuantityAdded, @RestockedDate, @Supplier, @UnitPrice, @Note);";
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                cmd.Parameters.AddWithValue("@ItemName", itemName);
                cmd.Parameters.AddWithValue("@QuantityAdded", quantityAdded);
                cmd.Parameters.AddWithValue("@RestockedDate", restockedDate);
                cmd.Parameters.AddWithValue("@Supplier", string.IsNullOrWhiteSpace(supplier) ? (object)DBNull.Value : supplier);
                cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                cmd.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(note) ? (object)DBNull.Value : note);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    UPDATE InventoryItems
                    SET Quantity = @Quantity,
                        [Stock Status] = @StockStatus,
                        LastRestock = @LastRestock,
                        UpdatedAt = @UpdatedAt
                    WHERE ItemId = @ItemId;";
                cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                cmd.Parameters.AddWithValue("@StockStatus", stockStatus);
                cmd.Parameters.AddWithValue("@LastRestock", restockedDate);
                cmd.Parameters.AddWithValue("@UpdatedAt", now);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();

            ActivityLogService.Log(
                "Update",
                "Inventory",
                $"Restocked '{itemName}' by {quantityAdded} (new quantity {newQuantity})");
        }

        public List<RecentItemUsage> GetRecentUsages(int count = 5)
            => QueryUsages($"ORDER BY UsageId DESC LIMIT {count}");

        public List<RecentItemUsage> GetAllUsages()
            => QueryUsages("ORDER BY UsageId DESC");

        private List<RecentItemUsage> QueryUsages(string orderClause)
        {
            var list = new List<RecentItemUsage>();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                $"SELECT UsageId, ItemId, ItemName, QuantityUsed, UsageDate, IFNULL(Notes,'') " +
                $"FROM InventoryUsage {orderClause};";

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new RecentItemUsage
                {
                    UsageId      = reader.GetInt32(0),
                    ItemId       = $"INV-{reader.GetInt32(1):D3}",
                    ItemName     = reader.GetString(2),
                    QuantityUsed = reader.GetInt32(3),
                    Date         = DateTime.Parse(reader.GetString(4)),
                    Notes        = reader.GetString(5)
                });
            }

            return list;
        }

        public List<RecentRestockItem> GetRecentRestocks(int count = 5)
            => QueryRestocks($"ORDER BY RestockId DESC LIMIT {count}");

        public List<RecentRestockItem> GetAllRestocks()
            => QueryRestocks("ORDER BY RestockId DESC");

        private List<RecentRestockItem> QueryRestocks(string orderClause)
        {
            var list = new List<RecentRestockItem>();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                $"SELECT RestockId, ItemId, ItemName, QuantityAdded, RestockedDate, " +
                $"IFNULL(Supplier,''), UnitPrice, IFNULL(Note,'') " +
                $"FROM InventoryRestocks {orderClause};";

            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new RecentRestockItem
                {
                    RestockId  = reader.GetInt32(0),
                    ItemId     = $"INV-{reader.GetInt32(1):D3}",
                    ItemName   = reader.GetString(2),
                    StockAdded = reader.GetInt32(3),
                    Date       = DateTime.Parse(reader.GetString(4)),
                    Supplier   = reader.GetString(5),
                    UnitPrice  = Convert.ToDecimal(reader.GetDouble(6)),
                    Notes      = reader.GetString(7)
                });
            }

            return list;
        }

        public void RestoreItem(int itemId, string timestamp)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE InventoryItems
                SET IsActive = 1,
                    UpdatedAt = @UpdatedAt
                WHERE ItemId = @ItemId;";

            command.Parameters.AddWithValue("@ItemId", itemId);
            command.Parameters.AddWithValue("@UpdatedAt", timestamp);
            command.ExecuteNonQuery();

            ActivityLogService.Log(
                "Restore",
                "Inventory",
                $"Restored inventory item '{GetItemName(itemId)}'");
        }

        public void LogItemUsage(int itemId, string itemName, int quantityUsed, string usageDate,
            string notes, int currentQuantity, int minimumThreshold)
        {
            int newQuantity = Math.Max(0, currentQuantity - quantityUsed);
            string stockStatus = newQuantity == 0 ? "Out of Stock"
                : newQuantity <= minimumThreshold ? "Low Stock"
                : "In Stock";
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();
            using SqliteTransaction tx = connection.BeginTransaction();

            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO InventoryUsage (ItemId, ItemName, QuantityUsed, UsageDate, Notes)
                    VALUES (@ItemId, @ItemName, @QuantityUsed, @UsageDate, @Notes);";
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                cmd.Parameters.AddWithValue("@ItemName", itemName);
                cmd.Parameters.AddWithValue("@QuantityUsed", quantityUsed);
                cmd.Parameters.AddWithValue("@UsageDate", usageDate);
                cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    UPDATE InventoryItems
                    SET Quantity = @Quantity,
                        [Stock Status] = @StockStatus,
                        UpdatedAt = @UpdatedAt
                    WHERE ItemId = @ItemId;";
                cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                cmd.Parameters.AddWithValue("@StockStatus", stockStatus);
                cmd.Parameters.AddWithValue("@UpdatedAt", now);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();

            ActivityLogService.Log(
                "Update",
                "Inventory",
                $"Logged usage of {quantityUsed} for '{itemName}' (new quantity {newQuantity})");
        }

        // Looks up an item's name for use in activity-log descriptions.
        // Returns a "#id" fallback if the name cannot be resolved.
        private static string GetItemName(int itemId)
        {
            try
            {
                using SqliteConnection connection = DatabaseService.GetConnection();
                connection.Open();
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT ItemName FROM InventoryItems WHERE ItemId = @ItemId;";
                command.Parameters.AddWithValue("@ItemId", itemId);
                return command.ExecuteScalar()?.ToString() ?? $"#{itemId}";
            }
            catch
            {
                return $"#{itemId}";
            }
        }
    }
}
