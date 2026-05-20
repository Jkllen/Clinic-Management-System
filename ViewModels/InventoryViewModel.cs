using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models.Inventory;
using CruzNeryClinic.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    public class InventoryViewModel : BaseViewModel
    {
        // ── Summary counts ───────────────────────────────────────────────────

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            set => SetProperty(ref _totalItems, value);
        }

        private int _lowStockCount;
        public int LowStockCount
        {
            get => _lowStockCount;
            set => SetProperty(ref _lowStockCount, value);
        }

        private int _outOfStockCount;
        public int OutOfStockCount
        {
            get => _outOfStockCount;
            set => SetProperty(ref _outOfStockCount, value);
        }

        // ── Collections ──────────────────────────────────────────────────────

        private List<InventoryItem> _allItems = new();

        public ObservableCollection<InventoryItem> InventoryItems { get; set; } = new();
        public ObservableCollection<RecentItemUsage> RecentItemUsages { get; set; } = new();
        public ObservableCollection<RecentRestockItem> RecentRestockItems { get; set; } = new();

        // ── Toolbar ──────────────────────────────────────────────────────────

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) ApplyView(); }
        }

        public ObservableCollection<string> FilterOptions { get; set; } = new()
        {
            "All", "In Stock", "Low Stock", "Out of Stock", "Active", "Inactive"
        };

        private string _selectedFilterOption = "All";
        public string SelectedFilterOption
        {
            get => _selectedFilterOption;
            set { if (SetProperty(ref _selectedFilterOption, value)) ApplyView(); }
        }

        public ObservableCollection<string> SortOptions { get; set; } = new()
        {
            "Item Id Ascending", "Item Id Descending", "Item Name A-Z", "Item Name Z-A", "Stock Lowest to Highest", "Stock Highest to Lowest", "Unit Price Low - High", "Unit Price High - Low"
        };

        private string _selectedSortOption = "Item Id Ascending";
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set { if (SetProperty(ref _selectedSortOption, value)) ApplyView(); }
        }

        // ── Selected row ─────────────────────────────────────────────────────

        private InventoryItem? _selectedInventoryItem;
        public InventoryItem? SelectedInventoryItem
        {
            get => _selectedInventoryItem;
            set => SetProperty(ref _selectedInventoryItem, value);
        }

        // ── Overlay (Add / Edit) ─────────────────────────────────────────────

        private bool _isItemOverlayOpen;
        public bool IsItemOverlayOpen
        {
            get => _isItemOverlayOpen;
            set => SetProperty(ref _isItemOverlayOpen, value);
        }

        private string _overlayTitle = "Add New Item";
        public string OverlayTitle
        {
            get => _overlayTitle;
            set => SetProperty(ref _overlayTitle, value);
        }

        private string _saveButtonLabel = "Save";
        public string SaveButtonLabel
        {
            get => _saveButtonLabel;
            set => SetProperty(ref _saveButtonLabel, value);
        }

        private string _previewItemId = "";
        public string PreviewItemId
        {
            get => _previewItemId;
            set => SetProperty(ref _previewItemId, value);
        }

        // Edit form fields
        private string _editItemName = "";
        public string EditItemName
        {
            get => _editItemName;
            set => SetProperty(ref _editItemName, value);
        }

        private string _editStock = "";
        public string EditStock
        {
            get => _editStock;
            set => SetProperty(ref _editStock, value);
        }

        private string _editUnitPrice = "";
        public string EditUnitPrice
        {
            get => _editUnitPrice;
            set => SetProperty(ref _editUnitPrice, value);
        }

        private string _editLowStockThreshold = "";
        public string EditLowStockThreshold
        {
            get => _editLowStockThreshold;
            set => SetProperty(ref _editLowStockThreshold, value);
        }

        private string _editNotes = "";
        public string EditNotes
        {
            get => _editNotes;
            set => SetProperty(ref _editNotes, value);
        }

        // Overlay validation
        private string _itemOverlayErrorMessage = "";
        public string ItemOverlayErrorMessage
        {
            get => _itemOverlayErrorMessage;
            set => SetProperty(ref _itemOverlayErrorMessage, value);
        }

        private bool _hasItemOverlayError;
        public bool HasItemOverlayError
        {
            get => _hasItemOverlayError;
            set => SetProperty(ref _hasItemOverlayError, value);
        }

        // ── Success prompt ───────────────────────────────────────────────────

        private bool _isSuccessPromptVisible;
        public bool IsSuccessPromptVisible
        {
            get => _isSuccessPromptVisible;
            set => SetProperty(ref _isSuccessPromptVisible, value);
        }

        private string _successMessage = "";
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        private string _successSubMessage = "";
        public string SuccessSubMessage
        {
            get => _successSubMessage;
            set => SetProperty(ref _successSubMessage, value);
        }

        // ── Error ────────────────────────────────────────────────────────────

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        // ── Commands ─────────────────────────────────────────────────────────

        public ICommand AddNewItemCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand CloseItemOverlayCommand { get; }
        public ICommand LogItemUsageCommand { get; }
        public ICommand RestockItemCommand { get; }
        public ICommand ViewAllUsagesCommand { get; }
        public ICommand ViewAllRestocksCommand { get; }
        public ICommand CloseSuccessPromptCommand { get; }
        public ICommand AddAnotherItemCommand { get; }
        public ICommand ViewItemCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand ArchiveItemCommand { get; }
        public ICommand CloseRestockOverlayCommand { get; }
        public ICommand ConfirmRestockCommand { get; }
        public ICommand CloseUsageOverlayCommand { get; }
        public ICommand ConfirmUsageCommand { get; }

        // ── Repository ───────────────────────────────────────────────────────

        private readonly InventoryRepository _repository = new();

        // ── Constructor ──────────────────────────────────────────────────────

        public InventoryViewModel()
        {
            AddNewItemCommand         = new RelayCommand(OpenAddOverlay);
            SaveItemCommand           = new RelayCommand(SaveItem);
            CloseItemOverlayCommand   = new RelayCommand(CloseOverlay);
            LogItemUsageCommand        = new RelayCommand(OpenUsageOverlay);
            RestockItemCommand         = new RelayCommand(OpenRestockOverlay);
            CloseRestockOverlayCommand = new RelayCommand(CloseRestockOverlay);
            ConfirmRestockCommand      = new RelayCommand(ConfirmRestock);
            CloseUsageOverlayCommand   = new RelayCommand(CloseUsageOverlay);
            ConfirmUsageCommand        = new RelayCommand(ConfirmUsage);
            ViewAllUsagesCommand      = new RelayCommand(() => MessageBox.Show("View all usages – coming soon."));
            ViewAllRestocksCommand    = new RelayCommand(() => MessageBox.Show("View all restocks – coming soon."));
            CloseSuccessPromptCommand = new RelayCommand(() => IsSuccessPromptVisible = false);
            AddAnotherItemCommand     = new RelayCommand(() => { IsSuccessPromptVisible = false; OpenAddOverlay(); });

            ViewItemCommand           = new RelayCommand<InventoryItem>(ViewItem);
            EditItemCommand           = new RelayCommand<InventoryItem>(OpenEditOverlay);
            ArchiveItemCommand        = new RelayCommand<InventoryItem>(ArchiveItem);

            LoadInventoryFromDatabase();
        }

        // ── Database Data Sync ───────────────────────────────────────────────

        private void LoadInventoryFromDatabase()
        {
            HasError = false;

            try
            {
                _allItems = _repository.GetAllItems();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to read items from database: {ex.Message}";
                HasError = true;
                _allItems = new List<InventoryItem>();
            }

            ApplyView();

            RecentItemUsages.Clear();
            RecentRestockItems.Clear();
        }

        private void RefreshCounts()
        {
            TotalItems      = _allItems.Count;
            LowStockCount   = _allItems.Count(i => i.QuantityInStock > 0 && i.QuantityInStock <= i.MinimumStockLevel);
            OutOfStockCount = _allItems.Count(i => i.QuantityInStock == 0);
        }

        private void ApplyView()
        {
            IEnumerable<InventoryItem> view = _allItems;

            // Search
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string q = SearchText.Trim();
                view = view.Where(i =>
                    i.ItemId.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    i.ItemName.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            // Filter
            view = SelectedFilterOption switch
            {
                "In Stock"     => view.Where(i => i.Status == "In Stock"),
                "Low Stock"    => view.Where(i => i.Status == "Low Stock"),
                "Out of Stock" => view.Where(i => i.Status == "Out of Stock"),
                "Active"       => view.Where(i => i.IsActive),
                "Inactive"     => view.Where(i => !i.IsActive),
                _              => view
            };

            // Sort
            view = SelectedSortOption switch
            {
                "Item Id Descending"      => view.OrderByDescending(i => i.RawItemId),
                "Item Name A-Z"           => view.OrderBy(i => i.ItemName),
                "Item Name Z-A"           => view.OrderByDescending(i => i.ItemName),
                "Stock Lowest to Highest" => view.OrderBy(i => i.QuantityInStock),
                "Stock Highest to Lowest" => view.OrderByDescending(i => i.QuantityInStock),
                "Unit Price Low - High"   => view.OrderBy(i => i.UnitPrice),
                "Unit Price High - Low"   => view.OrderByDescending(i => i.UnitPrice),
                _                         => view.OrderBy(i => i.RawItemId)
            };

            InventoryItems.Clear();
            foreach (var item in view)
                InventoryItems.Add(item);

            RefreshCounts();
        }


        // ── Command handlers ─────────────────────────────────────────────────

        private void OpenAddOverlay()
        {
            EditIsEditMode          = false;
            OverlayTitle            = "Add New Item";
            SaveButtonLabel         = "Add Item";
            PreviewItemId           = _repository.GetNextItemId();
            EditItemName            = "";
            EditStock               = "";
            EditUnitPrice           = "";
            EditLowStockThreshold   = "";
            EditNotes               = "";
            HasItemOverlayError     = false;
            ItemOverlayErrorMessage = "";
            IsItemOverlayOpen       = true;
        }

        private int _editingItemId;

        private void OpenEditOverlay(InventoryItem? item)
        {
            if (item == null) return;
            SelectedInventoryItem    = item;
            _editingItemId           = item.RawItemId;
            EditIsEditMode           = true;
            OverlayTitle             = "Update Item Information";
            SaveButtonLabel          = "Save Changes";
            PreviewItemId            = item.ItemId;
            EditItemName             = item.ItemName;
            EditStock                = item.QuantityInStock.ToString();
            EditUnitPrice            = item.UnitPrice.ToString("N2");
            EditLowStockThreshold    = item.MinimumStockLevel.ToString();
            EditNotes                = item.Note;
            EditCurrentStatusDisplay = item.Status;
            EditItemCreatedDisplay   = DateTime.TryParse(item.ItemCreated, out var created)
                ? created.ToString("MM/dd/yyyy") : "—";
            EditLastRestockDisplay   = item.LastRestock.HasValue
                ? item.LastRestock.Value.ToString("MM/dd/yyyy") : "—";
            EditItemStatusDisplay    = item.IsActive ? "Active" : "Inactive";
            HasItemOverlayError      = false;
            ItemOverlayErrorMessage  = "";
            IsItemOverlayOpen        = true;
        }

        private void SaveItem()
        {
            if (string.IsNullOrWhiteSpace(EditItemName))
            {
                ItemOverlayErrorMessage = "Item Name is required.";
                HasItemOverlayError = true;
                return;
            }
            if (!int.TryParse(EditStock, out int stock) || stock < 0)
            {
                ItemOverlayErrorMessage = "Stock must be a valid non-negative number.";
                HasItemOverlayError = true;
                return;
            }
            if (!decimal.TryParse(EditUnitPrice, out decimal price) || price < 0)
            {
                ItemOverlayErrorMessage = "Unit Price must be a valid number.";
                HasItemOverlayError = true;
                return;
            }
            if (!int.TryParse(EditLowStockThreshold, out int threshold) || threshold < 0)
            {
                ItemOverlayErrorMessage = "Low Stock Threshold must be a valid number.";
                HasItemOverlayError = true;
                return;
            }

            HasItemOverlayError = false;

            // Compute database text tags based on calculation
            string stockStatus = stock == 0 ? "Out of Stock" : (stock <= threshold ? "Low Stock" : "In Stock");
            string timeStampStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                if (SaveButtonLabel == "Add Item")
                {
                    _repository.AddItem(EditItemName.Trim(), stock, Convert.ToDouble(price), threshold, stockStatus, EditNotes.Trim(), timeStampStr);
                    SuccessMessage    = "Item Added!";
                    SuccessSubMessage = $"{EditItemName.Trim()} has been added to inventory.";
                }
                else
                {
                    _repository.UpdateItem(_editingItemId, EditItemName.Trim(), stock, Convert.ToDouble(price), threshold, stockStatus, EditNotes.Trim(), timeStampStr);
                    SuccessMessage    = "Item Updated!";
                    SuccessSubMessage = $"{EditItemName.Trim()} has been updated.";
                }

                // Sync view data instantly with the SQL state 
                LoadInventoryFromDatabase();

                IsItemOverlayOpen      = false;
                IsSuccessPromptVisible = true;
            }
            catch (Exception ex)
            {
                ItemOverlayErrorMessage = $"Database error: {ex.Message}";
                HasItemOverlayError = true;
            }
        }

        private void ViewItem(InventoryItem? item)
        {
            if (item == null) return;
            MessageBox.Show(
                $"Name:       {item.ItemName}\n" +
                $"Stock:      {item.QuantityInStock}\n" +
                $"Unit Price: ₱{item.UnitPrice:N2}\n" +
                $"Min Level:  {item.MinimumStockLevel}\n" +
                $"Note:       {item.Note}\n" +
                $"Active:     {(item.IsActive ? "Yes" : "No")}",
                "Item Details",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // ── ARCHIVE ITEM (Soft Deactivate) ───────────────────────────────────
        private void ArchiveItem(InventoryItem? item)
        {
            if (item == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to archive '{item.ItemName}'?\nThis will deactivate the item without permanently deleting its history.",
                "Confirm Archive",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _repository.ArchiveItem(item.RawItemId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    LoadInventoryFromDatabase();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to archive item from database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseOverlay()
        {
            IsItemOverlayOpen       = false;
            HasItemOverlayError     = false;
            ItemOverlayErrorMessage = "";
        }

        // ── Edit overlay – extra read-only display fields ────────────────────

        private bool _editIsEditMode;
        public bool EditIsEditMode
        {
            get => _editIsEditMode;
            set { SetProperty(ref _editIsEditMode, value); OnPropertyChanged(nameof(EditIsAddMode)); }
        }
        public bool EditIsAddMode => !EditIsEditMode;

        private string _editCurrentStatusDisplay = "";
        public string EditCurrentStatusDisplay
        {
            get => _editCurrentStatusDisplay;
            set => SetProperty(ref _editCurrentStatusDisplay, value);
        }

        private string _editItemCreatedDisplay = "";
        public string EditItemCreatedDisplay
        {
            get => _editItemCreatedDisplay;
            set => SetProperty(ref _editItemCreatedDisplay, value);
        }

        private string _editLastRestockDisplay = "";
        public string EditLastRestockDisplay
        {
            get => _editLastRestockDisplay;
            set => SetProperty(ref _editLastRestockDisplay, value);
        }

        private string _editItemStatusDisplay = "";
        public string EditItemStatusDisplay
        {
            get => _editItemStatusDisplay;
            set => SetProperty(ref _editItemStatusDisplay, value);
        }

        // ── Restock Item overlay ─────────────────────────────────────────────

        private bool _isRestockOverlayOpen;
        public bool IsRestockOverlayOpen
        {
            get => _isRestockOverlayOpen;
            set => SetProperty(ref _isRestockOverlayOpen, value);
        }

        public ObservableCollection<string> ActiveItemNames { get; set; } = new();

        private InventoryItem? _restockSelectedItem;

        private string _restockSelectedItemName = "";
        public string RestockSelectedItemName
        {
            get => _restockSelectedItemName;
            set { SetProperty(ref _restockSelectedItemName, value); OnRestockItemSelected(); }
        }

        private string _restockItemIdDisplay = "Auto-Filled";
        public string RestockItemIdDisplay
        {
            get => _restockItemIdDisplay;
            set => SetProperty(ref _restockItemIdDisplay, value);
        }

        private string _restockStockAdded = "";
        public string RestockStockAdded
        {
            get => _restockStockAdded;
            set { SetProperty(ref _restockStockAdded, value); OnPropertyChanged(nameof(RestockNewTotalDisplay)); }
        }

        public string RestockNewTotalDisplay
        {
            get
            {
                if (_restockSelectedItem == null || !int.TryParse(RestockStockAdded, out int added) || added < 0)
                    return "New Stock Total: —";
                return $"New Stock Total: {_restockSelectedItem.QuantityInStock + added}";
            }
        }

        private string _restockCurrentStockDisplay = "Item Current Stock: —";
        public string RestockCurrentStockDisplay
        {
            get => _restockCurrentStockDisplay;
            set => SetProperty(ref _restockCurrentStockDisplay, value);
        }

        private DateTime? _restockDate = null;
        public DateTime? RestockDate
        {
            get => _restockDate;
            set => SetProperty(ref _restockDate, value);
        }

        private string _restockUnitPriceStr = "";
        public string RestockUnitPriceStr
        {
            get => _restockUnitPriceStr;
            set => SetProperty(ref _restockUnitPriceStr, value);
        }

        private string _restockSupplier = "";
        public string RestockSupplier
        {
            get => _restockSupplier;
            set => SetProperty(ref _restockSupplier, value);
        }

        private string _restockNotes = "";
        public string RestockNotes
        {
            get => _restockNotes;
            set => SetProperty(ref _restockNotes, value);
        }

        private string _restockErrorMessage = "";
        public string RestockErrorMessage
        {
            get => _restockErrorMessage;
            set => SetProperty(ref _restockErrorMessage, value);
        }

        private bool _hasRestockError;
        public bool HasRestockError
        {
            get => _hasRestockError;
            set => SetProperty(ref _hasRestockError, value);
        }

        // ── Log Item Usage overlay ───────────────────────────────────────────

        private bool _isUsageOverlayOpen;
        public bool IsUsageOverlayOpen
        {
            get => _isUsageOverlayOpen;
            set => SetProperty(ref _isUsageOverlayOpen, value);
        }

        private InventoryItem? _usageSelectedItem;

        private string _usageSelectedItemName = "";
        public string UsageSelectedItemName
        {
            get => _usageSelectedItemName;
            set { SetProperty(ref _usageSelectedItemName, value); OnUsageItemSelected(); }
        }

        private string _usageItemIdDisplay = "Auto-Filled";
        public string UsageItemIdDisplay
        {
            get => _usageItemIdDisplay;
            set => SetProperty(ref _usageItemIdDisplay, value);
        }

        private string _usageQuantityUsed = "";
        public string UsageQuantityUsed
        {
            get => _usageQuantityUsed;
            set { SetProperty(ref _usageQuantityUsed, value); OnPropertyChanged(nameof(UsageRemainingStockDisplay)); }
        }

        public string UsageRemainingStockDisplay
        {
            get
            {
                if (_usageSelectedItem == null || !int.TryParse(UsageQuantityUsed, out int used) || used < 0)
                    return "Remaining Stock Left: —";
                int remaining = _usageSelectedItem.QuantityInStock - used;
                return remaining < 0
                    ? "Remaining Stock Left: ⚠ Exceeds stock"
                    : $"Remaining Stock Left: {remaining}";
            }
        }

        private string _usageCurrentStockDisplay = "Item Current Stock: —";
        public string UsageCurrentStockDisplay
        {
            get => _usageCurrentStockDisplay;
            set => SetProperty(ref _usageCurrentStockDisplay, value);
        }

        private DateTime? _usageDate = null;
        public DateTime? UsageDate
        {
            get => _usageDate;
            set => SetProperty(ref _usageDate, value);
        }

        private string _usageNotes = "";
        public string UsageNotes
        {
            get => _usageNotes;
            set => SetProperty(ref _usageNotes, value);
        }

        private string _usageErrorMessage = "";
        public string UsageErrorMessage
        {
            get => _usageErrorMessage;
            set => SetProperty(ref _usageErrorMessage, value);
        }

        private bool _hasUsageError;
        public bool HasUsageError
        {
            get => _hasUsageError;
            set => SetProperty(ref _hasUsageError, value);
        }

        // ── Overlay item-list helpers ────────────────────────────────────────

        private void PopulateActiveItemNames()
        {
            ActiveItemNames.Clear();
            foreach (var item in _allItems.Where(i => i.IsActive).OrderBy(i => i.ItemName))
                ActiveItemNames.Add(item.ItemName);
        }

        private void OnRestockItemSelected()
        {
            _restockSelectedItem = _allItems.FirstOrDefault(i => i.ItemName == RestockSelectedItemName);
            RestockItemIdDisplay = _restockSelectedItem?.ItemId ?? "Auto-Filled";
            RestockCurrentStockDisplay = _restockSelectedItem != null
                ? $"Item Current Stock: {_restockSelectedItem.QuantityInStock}"
                : "Item Current Stock: —";
            OnPropertyChanged(nameof(RestockNewTotalDisplay));
        }

        private void OnUsageItemSelected()
        {
            _usageSelectedItem = _allItems.FirstOrDefault(i => i.ItemName == UsageSelectedItemName);
            UsageItemIdDisplay = _usageSelectedItem?.ItemId ?? "Auto-Filled";
            UsageCurrentStockDisplay = _usageSelectedItem != null
                ? $"Item Current Stock: {_usageSelectedItem.QuantityInStock}"
                : "Item Current Stock: —";
            OnPropertyChanged(nameof(UsageRemainingStockDisplay));
        }

        // ── Restock handlers ─────────────────────────────────────────────────

        private void OpenRestockOverlay()
        {
            PopulateActiveItemNames();
            _restockSelectedItem       = null;
            RestockSelectedItemName    = "";
            RestockItemIdDisplay       = "Auto-Filled";
            RestockStockAdded          = "";
            RestockDate                = null;
            RestockUnitPriceStr        = "";
            RestockSupplier            = "";
            RestockNotes               = "";
            RestockCurrentStockDisplay = "Item Current Stock: —";
            HasRestockError            = false;
            RestockErrorMessage        = "";
            IsRestockOverlayOpen       = true;
        }

        private void CloseRestockOverlay()
        {
            IsRestockOverlayOpen = false;
            HasRestockError      = false;
            RestockErrorMessage  = "";
        }

        private void ConfirmRestock()
        {
            if (_restockSelectedItem == null)
            {
                RestockErrorMessage = "Please select an item to restock.";
                HasRestockError = true;
                return;
            }
            if (!int.TryParse(RestockStockAdded, out int qty) || qty <= 0)
            {
                RestockErrorMessage = "Stock Added must be a positive number.";
                HasRestockError = true;
                return;
            }
            if (RestockDate == null)
            {
                RestockErrorMessage = "Please select a Date of Restock.";
                HasRestockError = true;
                return;
            }
            if (!decimal.TryParse(RestockUnitPriceStr, out decimal unitPrice) || unitPrice < 0)
            {
                RestockErrorMessage = "Unit Price must be a valid non-negative number.";
                HasRestockError = true;
                return;
            }

            HasRestockError = false;

            try
            {
                _repository.RestockItem(
                    _restockSelectedItem.RawItemId,
                    _restockSelectedItem.ItemName,
                    qty,
                    RestockDate.Value.ToString("yyyy-MM-dd"),
                    RestockSupplier.Trim(),
                    Convert.ToDouble(unitPrice),
                    RestockNotes.Trim(),
                    _restockSelectedItem.QuantityInStock,
                    _restockSelectedItem.MinimumStockLevel);

                LoadInventoryFromDatabase();
                IsRestockOverlayOpen   = false;
                SuccessMessage         = "Item Restocked!";
                SuccessSubMessage      = $"{_restockSelectedItem.ItemName} restocked with {qty} unit(s).";
                IsSuccessPromptVisible = true;
            }
            catch (Exception ex)
            {
                RestockErrorMessage = $"Database error: {ex.Message}";
                HasRestockError = true;
            }
        }

        // ── Log Usage handlers ───────────────────────────────────────────────

        private void OpenUsageOverlay()
        {
            PopulateActiveItemNames();
            _usageSelectedItem       = null;
            UsageSelectedItemName    = "";
            UsageItemIdDisplay       = "Auto-Filled";
            UsageQuantityUsed        = "";
            UsageDate                = null;
            UsageNotes               = "";
            UsageCurrentStockDisplay = "Item Current Stock: —";
            HasUsageError            = false;
            UsageErrorMessage        = "";
            IsUsageOverlayOpen       = true;
        }

        private void CloseUsageOverlay()
        {
            IsUsageOverlayOpen = false;
            HasUsageError      = false;
            UsageErrorMessage  = "";
        }

        private void ConfirmUsage()
        {
            if (_usageSelectedItem == null)
            {
                UsageErrorMessage = "Please select an item.";
                HasUsageError = true;
                return;
            }
            if (!int.TryParse(UsageQuantityUsed, out int qty) || qty <= 0)
            {
                UsageErrorMessage = "Quantity Used must be a positive number.";
                HasUsageError = true;
                return;
            }
            if (qty > _usageSelectedItem.QuantityInStock)
            {
                UsageErrorMessage = $"Quantity ({qty}) exceeds available stock ({_usageSelectedItem.QuantityInStock}).";
                HasUsageError = true;
                return;
            }
            if (UsageDate == null)
            {
                UsageErrorMessage = "Please select a Date of Usage.";
                HasUsageError = true;
                return;
            }

            HasUsageError = false;

            try
            {
                _repository.LogItemUsage(
                    _usageSelectedItem.RawItemId,
                    _usageSelectedItem.ItemName,
                    qty,
                    UsageDate.Value.ToString("yyyy-MM-dd"),
                    UsageNotes.Trim(),
                    _usageSelectedItem.QuantityInStock,
                    _usageSelectedItem.MinimumStockLevel);

                LoadInventoryFromDatabase();
                IsUsageOverlayOpen     = false;
                SuccessMessage         = "Usage Recorded!";
                SuccessSubMessage      = $"{qty} unit(s) of {_usageSelectedItem.ItemName} recorded.";
                IsSuccessPromptVisible = true;
            }
            catch (Exception ex)
            {
                UsageErrorMessage = $"Database error: {ex.Message}";
                HasUsageError = true;
            }
        }
    }

    public class RecentItemUsage
    {
        public string ItemId { get; set; } = "";
        public string ItemName { get; set; } = "";
        public int QuantityUsed { get; set; }
        public DateTime Date { get; set; }
    }

    public class RecentRestockItem
    {
        public string ItemId { get; set; } = "";
        public string ItemName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int StockAdded { get; set; }
        public DateTime Date { get; set; }
    }
}