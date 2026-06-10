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
        // Number of rows shown per page in the usage / restock history overlays.
        private const int HistoryPageSize = 10;

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
        private List<InventoryItem> _archivedItems = new();

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
            "All", "In Stock", "Low Stock", "Out of Stock", "Archived"
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

        // ── Confirmation Dialog ──────────────────────────────────────────────

        private bool _isConfirmDialogVisible;
        public bool IsConfirmDialogVisible
        {
            get => _isConfirmDialogVisible;
            set => SetProperty(ref _isConfirmDialogVisible, value);
        }

        private string _confirmTitle = "";
        public string ConfirmTitle
        {
            get => _confirmTitle;
            set => SetProperty(ref _confirmTitle, value);
        }

        private string _confirmSubtitle = "";
        public string ConfirmSubtitle
        {
            get => _confirmSubtitle;
            set => SetProperty(ref _confirmSubtitle, value);
        }

        private string _confirmField1Label = "";
        public string ConfirmField1Label { get => _confirmField1Label; set => SetProperty(ref _confirmField1Label, value); }
        private string _confirmField1Value = "";
        public string ConfirmField1Value { get => _confirmField1Value; set => SetProperty(ref _confirmField1Value, value); }

        private string _confirmField2Label = "";
        public string ConfirmField2Label { get => _confirmField2Label; set => SetProperty(ref _confirmField2Label, value); }
        private string _confirmField2Value = "";
        public string ConfirmField2Value { get => _confirmField2Value; set => SetProperty(ref _confirmField2Value, value); }

        private string _confirmField3Label = "";
        public string ConfirmField3Label { get => _confirmField3Label; set => SetProperty(ref _confirmField3Label, value); }
        private string _confirmField3Value = "";
        public string ConfirmField3Value { get => _confirmField3Value; set => SetProperty(ref _confirmField3Value, value); }

        private string _confirmField4Label = "";
        public string ConfirmField4Label { get => _confirmField4Label; set => SetProperty(ref _confirmField4Label, value); }
        private string _confirmField4Value = "";
        public string ConfirmField4Value { get => _confirmField4Value; set => SetProperty(ref _confirmField4Value, value); }

        // ── Usage History overlay ────────────────────────────────────────────

        private bool _isUsageHistoryOpen;
        public bool IsUsageHistoryOpen
        {
            get => _isUsageHistoryOpen;
            set => SetProperty(ref _isUsageHistoryOpen, value);
        }

        private List<RecentItemUsage> _allUsageHistory = new();
        private List<RecentItemUsage> _usageFiltered = new();
        public ObservableCollection<RecentItemUsage> UsageHistoryItems { get; } = new();

        private string _usageHistorySearch = "";
        public string UsageHistorySearch
        {
            get => _usageHistorySearch;
            set { if (SetProperty(ref _usageHistorySearch, value)) ApplyUsageHistoryFilter(); }
        }

        // ── Usage history pagination (HistoryPageSize rows per page) ──────────
        private int _usagePage = 1;
        public int UsagePage
        {
            get => _usagePage;
            private set { if (SetProperty(ref _usagePage, value)) OnPropertyChanged(nameof(UsagePageInfo)); }
        }

        public int UsageTotalPages =>
            Math.Max(1, (int)Math.Ceiling(_usageFiltered.Count / (double)HistoryPageSize));

        public string UsagePageInfo => $"Page {UsagePage} of {UsageTotalPages}";

        public bool UsageHasMultiplePages => _usageFiltered.Count > HistoryPageSize;

        private void ApplyUsageHistoryFilter()
        {
            var q = UsageHistorySearch.Trim();
            _usageFiltered = (string.IsNullOrWhiteSpace(q)
                ? _allUsageHistory
                : _allUsageHistory.Where(u =>
                    u.ItemId.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    u.ItemName.Contains(q, StringComparison.OrdinalIgnoreCase))).ToList();

            UsagePage = 1;
            RefreshUsagePage();
        }

        private void RefreshUsagePage()
        {
            if (UsagePage > UsageTotalPages) UsagePage = UsageTotalPages;

            UsageHistoryItems.Clear();
            foreach (var u in _usageFiltered.Skip((UsagePage - 1) * HistoryPageSize).Take(HistoryPageSize))
                UsageHistoryItems.Add(u);

            OnPropertyChanged(nameof(UsageTotalPages));
            OnPropertyChanged(nameof(UsagePageInfo));
            OnPropertyChanged(nameof(UsageHasMultiplePages));
            UsageNextPageCommand.NotifyCanExecuteChanged();
            UsagePrevPageCommand.NotifyCanExecuteChanged();
        }

        private void UsageNextPage()
        {
            if (UsagePage < UsageTotalPages) { UsagePage++; RefreshUsagePage(); }
        }

        private void UsagePrevPage()
        {
            if (UsagePage > 1) { UsagePage--; RefreshUsagePage(); }
        }

        // ── Restock History overlay ──────────────────────────────────────────

        private bool _isRestockHistoryOpen;
        public bool IsRestockHistoryOpen
        {
            get => _isRestockHistoryOpen;
            set => SetProperty(ref _isRestockHistoryOpen, value);
        }

        private List<RecentRestockItem> _allRestockHistory = new();
        private List<RecentRestockItem> _restockFiltered = new();
        public ObservableCollection<RecentRestockItem> RestockHistoryItems { get; } = new();

        private string _restockHistorySearch = "";
        public string RestockHistorySearch
        {
            get => _restockHistorySearch;
            set { if (SetProperty(ref _restockHistorySearch, value)) ApplyRestockHistoryFilter(); }
        }

        // ── Restock history pagination (HistoryPageSize rows per page) ────────
        private int _restockPage = 1;
        public int RestockPage
        {
            get => _restockPage;
            private set { if (SetProperty(ref _restockPage, value)) OnPropertyChanged(nameof(RestockPageInfo)); }
        }

        public int RestockTotalPages =>
            Math.Max(1, (int)Math.Ceiling(_restockFiltered.Count / (double)HistoryPageSize));

        public string RestockPageInfo => $"Page {RestockPage} of {RestockTotalPages}";

        public bool RestockHasMultiplePages => _restockFiltered.Count > HistoryPageSize;

        private void ApplyRestockHistoryFilter()
        {
            var q = RestockHistorySearch.Trim();
            _restockFiltered = (string.IsNullOrWhiteSpace(q)
                ? _allRestockHistory
                : _allRestockHistory.Where(r =>
                    r.ItemId.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    r.ItemName.Contains(q, StringComparison.OrdinalIgnoreCase))).ToList();

            RestockPage = 1;
            RefreshRestockPage();
        }

        private void RefreshRestockPage()
        {
            if (RestockPage > RestockTotalPages) RestockPage = RestockTotalPages;

            RestockHistoryItems.Clear();
            foreach (var r in _restockFiltered.Skip((RestockPage - 1) * HistoryPageSize).Take(HistoryPageSize))
                RestockHistoryItems.Add(r);

            OnPropertyChanged(nameof(RestockTotalPages));
            OnPropertyChanged(nameof(RestockPageInfo));
            OnPropertyChanged(nameof(RestockHasMultiplePages));
            RestockNextPageCommand.NotifyCanExecuteChanged();
            RestockPrevPageCommand.NotifyCanExecuteChanged();
        }

        private void RestockNextPage()
        {
            if (RestockPage < RestockTotalPages) { RestockPage++; RefreshRestockPage(); }
        }

        private void RestockPrevPage()
        {
            if (RestockPage > 1) { RestockPage--; RefreshRestockPage(); }
        }

        // ── View Item overlay ────────────────────────────────────────────────

        private bool _isViewOverlayOpen;
        public bool IsViewOverlayOpen
        {
            get => _isViewOverlayOpen;
            set => SetProperty(ref _isViewOverlayOpen, value);
        }

        private string _viewItemName = "";
        public string ViewItemName { get => _viewItemName; set => SetProperty(ref _viewItemName, value); }

        private string _viewItemId = "";
        public string ViewItemId { get => _viewItemId; set => SetProperty(ref _viewItemId, value); }

        private string _viewItemStock = "";
        public string ViewItemStock { get => _viewItemStock; set => SetProperty(ref _viewItemStock, value); }

        private string _viewItemPrice = "";
        public string ViewItemPrice { get => _viewItemPrice; set => SetProperty(ref _viewItemPrice, value); }

        private string _viewItemStockStatus = "";
        public string ViewItemStockStatus { get => _viewItemStockStatus; set => SetProperty(ref _viewItemStockStatus, value); }

        private string _viewItemDateAdded = "";
        public string ViewItemDateAdded { get => _viewItemDateAdded; set => SetProperty(ref _viewItemDateAdded, value); }

        private string _viewItemLastRestock = "";
        public string ViewItemLastRestock { get => _viewItemLastRestock; set => SetProperty(ref _viewItemLastRestock, value); }

        private string _viewItemActiveStatus = "";
        public string ViewItemActiveStatus { get => _viewItemActiveStatus; set => SetProperty(ref _viewItemActiveStatus, value); }

        private string _viewItemThreshold = "";
        public string ViewItemThreshold { get => _viewItemThreshold; set => SetProperty(ref _viewItemThreshold, value); }

        private string _viewItemNotes = "";
        public string ViewItemNotes { get => _viewItemNotes; set => SetProperty(ref _viewItemNotes, value); }

        // ── Success prompt ───────────────────────────────────────────────────

        private bool _isSuccessPromptVisible;
        public bool IsSuccessPromptVisible
        {
            get => _isSuccessPromptVisible;
            set => SetProperty(ref _isSuccessPromptVisible, value);
        }

        private string _successSubMessage = "";
        public string SuccessSubMessage
        {
            get => _successSubMessage;
            set => SetProperty(ref _successSubMessage, value);
        }

        private string _successActionLabel = "";
        public string SuccessActionLabel
        {
            get => _successActionLabel;
            set => SetProperty(ref _successActionLabel, value);
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

        // ── Private state ─────────────────────────────────────────────────────

        private string _currentOperation = "";
        private Action? _pendingFinalConfirm;

        // ── Commands ─────────────────────────────────────────────────────────

        public ICommand AddNewItemCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand CloseItemOverlayCommand { get; }
        public ICommand LogItemUsageCommand { get; }
        public ICommand RestockItemCommand { get; }
        public ICommand ViewAllUsagesCommand { get; }
        public ICommand CloseUsageHistoryCommand { get; }
        public ICommand ViewAllRestocksCommand { get; }
        public ICommand CloseRestockHistoryCommand { get; }
        public IRelayCommand UsageNextPageCommand { get; }
        public IRelayCommand UsagePrevPageCommand { get; }
        public IRelayCommand RestockNextPageCommand { get; }
        public IRelayCommand RestockPrevPageCommand { get; }
        public ICommand CloseSuccessPromptCommand { get; }
        public ICommand SuccessActionCommand { get; }
        public ICommand GoBackConfirmCommand { get; }
        public ICommand FinalConfirmCommand { get; }
        public ICommand ViewItemCommand { get; }
        public ICommand CloseViewOverlayCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand ArchiveItemCommand { get; }
        public ICommand RestoreItemCommand { get; }
        public ICommand CloseRestockOverlayCommand { get; }
        public ICommand ConfirmRestockCommand { get; }
        public ICommand CloseUsageOverlayCommand { get; }
        public ICommand ConfirmUsageCommand { get; }

        // Spin box step commands (increment / decrement)
        public ICommand UsageQtyIncrementCommand { get; }
        public ICommand UsageQtyDecrementCommand { get; }
        public ICommand RestockQtyIncrementCommand { get; }
        public ICommand RestockQtyDecrementCommand { get; }
        public ICommand RestockPriceIncrementCommand { get; }
        public ICommand RestockPriceDecrementCommand { get; }
        public ICommand EditStockIncrementCommand { get; }
        public ICommand EditStockDecrementCommand { get; }
        public ICommand EditUnitPriceIncrementCommand { get; }
        public ICommand EditUnitPriceDecrementCommand { get; }
        public ICommand EditThresholdIncrementCommand { get; }
        public ICommand EditThresholdDecrementCommand { get; }

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
            ViewAllUsagesCommand      = new RelayCommand(OpenUsageHistory);
            CloseUsageHistoryCommand  = new RelayCommand(() => { IsUsageHistoryOpen = false; UsageHistorySearch = ""; });
            ViewAllRestocksCommand    = new RelayCommand(OpenRestockHistory);
            CloseRestockHistoryCommand = new RelayCommand(() => { IsRestockHistoryOpen = false; RestockHistorySearch = ""; });
            UsageNextPageCommand      = new RelayCommand(UsageNextPage, () => UsagePage < UsageTotalPages);
            UsagePrevPageCommand      = new RelayCommand(UsagePrevPage, () => UsagePage > 1);
            RestockNextPageCommand    = new RelayCommand(RestockNextPage, () => RestockPage < RestockTotalPages);
            RestockPrevPageCommand    = new RelayCommand(RestockPrevPage, () => RestockPage > 1);
            CloseSuccessPromptCommand = new RelayCommand(() => IsSuccessPromptVisible = false);
            SuccessActionCommand      = new RelayCommand(SuccessAction);
            GoBackConfirmCommand      = new RelayCommand(GoBackConfirm);
            FinalConfirmCommand       = new RelayCommand(FinalConfirm);

            UsageQtyIncrementCommand     = new RelayCommand(() => UsageQuantityUsed = StepInt(UsageQuantityUsed, +1).ToString());
            UsageQtyDecrementCommand     = new RelayCommand(() => UsageQuantityUsed = StepInt(UsageQuantityUsed, -1).ToString());
            RestockQtyIncrementCommand   = new RelayCommand(() => RestockStockAdded = StepInt(RestockStockAdded, +1).ToString());
            RestockQtyDecrementCommand   = new RelayCommand(() => RestockStockAdded = StepInt(RestockStockAdded, -1).ToString());
            RestockPriceIncrementCommand = new RelayCommand(() => RestockUnitPriceStr = StepDecimal(RestockUnitPriceStr, +1m).ToString("N2"));
            RestockPriceDecrementCommand = new RelayCommand(() => RestockUnitPriceStr = StepDecimal(RestockUnitPriceStr, -1m).ToString("N2"));
            EditStockIncrementCommand     = new RelayCommand(() => EditStock = StepInt(EditStock, +1).ToString());
            EditStockDecrementCommand     = new RelayCommand(() => EditStock = StepInt(EditStock, -1).ToString());
            EditUnitPriceIncrementCommand = new RelayCommand(() => EditUnitPrice = StepDecimal(EditUnitPrice, +1m).ToString("N2"));
            EditUnitPriceDecrementCommand = new RelayCommand(() => EditUnitPrice = StepDecimal(EditUnitPrice, -1m).ToString("N2"));
            EditThresholdIncrementCommand = new RelayCommand(() => EditLowStockThreshold = StepInt(EditLowStockThreshold, +1).ToString());
            EditThresholdDecrementCommand = new RelayCommand(() => EditLowStockThreshold = StepInt(EditLowStockThreshold, -1).ToString());

            ViewItemCommand           = new RelayCommand<InventoryItem>(ViewItem);
            CloseViewOverlayCommand   = new RelayCommand(() => IsViewOverlayOpen = false);
            EditItemCommand           = new RelayCommand<InventoryItem>(OpenEditOverlay);
            ArchiveItemCommand        = new RelayCommand<InventoryItem>(ArchiveItem);
            RestoreItemCommand        = new RelayCommand<InventoryItem>(RestoreItem);

            LoadInventoryFromDatabase();
        }

        // ── Database Data Sync ───────────────────────────────────────────────

        private void LoadInventoryFromDatabase()
        {
            HasError = false;

            try
            {
                _allItems      = _repository.GetAllItems();
                _archivedItems = _repository.GetArchivedItems();
            }
            catch (Exception ex)
            {
                ErrorMessage   = $"Failed to read items from database: {ex.Message}";
                HasError       = true;
                _allItems      = new List<InventoryItem>();
                _archivedItems = new List<InventoryItem>();
            }

            ApplyView();

            RecentItemUsages.Clear();
            foreach (var u in _repository.GetRecentUsages(5))
                RecentItemUsages.Add(u);

            RecentRestockItems.Clear();
            foreach (var r in _repository.GetRecentRestocks(5))
                RecentRestockItems.Add(r);
        }

        private void RefreshCounts()
        {
            TotalItems      = _allItems.Count;
            LowStockCount   = _allItems.Count(i => i.QuantityInStock > 0 && i.QuantityInStock <= i.MinimumStockLevel);
            OutOfStockCount = _allItems.Count(i => i.QuantityInStock == 0);
            // _allItems is active-only, so all three counts reflect only active inventory
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

            // Filter — active items only unless "Archived" is selected
            if (SelectedFilterOption == "Archived")
            {
                view = _archivedItems.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string q = SearchText.Trim();
                    view = view.Where(i =>
                        i.ItemId.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        i.ItemName.Contains(q, StringComparison.OrdinalIgnoreCase));
                }
            }
            else
            {
                view = SelectedFilterOption switch
                {
                    "In Stock"     => view.Where(i => i.Status == "In Stock"),
                    "Low Stock"    => view.Where(i => i.Status == "Low Stock"),
                    "Out of Stock" => view.Where(i => i.Status == "Out of Stock"),
                    _              => view
                };
            }

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
            EditStock               = "1";
            EditUnitPrice           = "";
            EditLowStockThreshold   = "1";
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
            EditItemStatusDisplay    = item.IsActive ? "Active" : "Archived";
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
            if (_repository.ItemNameExists(EditItemName.Trim(), EditIsAddMode ? 0 : _editingItemId))
            {
                MessageBox.Show(
                    $"An item named '{EditItemName.Trim()}' already exists. Please use a different name.",
                    "Duplicate Item Name",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            HasItemOverlayError = false;

            string stockStatus  = stock == 0 ? "Out of Stock" : (stock <= threshold ? "Low Stock" : "In Stock");
            string timeStampStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (EditIsAddMode)
            {
                string name  = EditItemName.Trim();
                string notes = EditNotes.Trim();

                _currentOperation  = "additem";
                ConfirmTitle       = "Add New Item?";
                ConfirmSubtitle    = "Are you sure you want to add this item to inventory?";
                ConfirmField1Label = "Item Name :";
                ConfirmField1Value = name;
                ConfirmField2Label = "Item Id:";
                ConfirmField2Value = PreviewItemId;
                ConfirmField3Label = "Stock :";
                ConfirmField3Value = stock.ToString();
                ConfirmField4Label = "Unit Price:";
                ConfirmField4Value = $"₱{price:N2}";
                SuccessActionLabel = "Add New Item";

                _pendingFinalConfirm = () => ExecuteAddItem(name, stock, price, threshold, stockStatus, notes, timeStampStr);

                IsItemOverlayOpen      = false;
                IsConfirmDialogVisible = true;
            }
            else
            {
                // Edit mode – save directly, no confirmation
                try
                {
                    _repository.UpdateItem(_editingItemId, EditItemName.Trim(), stock, Convert.ToDouble(price), threshold, stockStatus, EditNotes.Trim(), timeStampStr);
                    LoadInventoryFromDatabase();
                    IsItemOverlayOpen = false;
                    MessageBox.Show(
                        "Item updated successfully.",
                        "Changes Saved",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ItemOverlayErrorMessage = $"Database error: {ex.Message}";
                    HasItemOverlayError = true;
                }
            }
        }

        private void ExecuteAddItem(string name, int stock, decimal price, int threshold, string stockStatus, string notes, string timeStampStr)
        {
            try
            {
                _repository.AddItem(name, stock, Convert.ToDouble(price), threshold, stockStatus, notes, timeStampStr);
                LoadInventoryFromDatabase();
                IsConfirmDialogVisible = false;
                SuccessSubMessage      = "Item added successfully.";
                IsSuccessPromptVisible = true;
            }
            catch (Exception ex)
            {
                IsConfirmDialogVisible  = false;
                IsItemOverlayOpen       = true;
                ItemOverlayErrorMessage = $"Database error: {ex.Message}";
                HasItemOverlayError     = true;
            }
        }

        private void ViewItem(InventoryItem? item)
        {
            if (item == null) return;

            ViewItemName        = item.ItemName;
            ViewItemId          = item.ItemId;
            ViewItemStock       = item.QuantityInStock.ToString();
            ViewItemPrice       = $"₱{item.UnitPrice:N2}";
            ViewItemStockStatus = item.Status;
            ViewItemDateAdded   = DateTime.TryParse(item.ItemCreated, out var created)
                ? created.ToString("MM/dd/yyyy") : "—";
            ViewItemLastRestock = item.LastRestock.HasValue
                ? item.LastRestock.Value.ToString("MM/dd/yyyy") : "—";
            ViewItemActiveStatus = item.IsActive ? "Active" : "Archived";
            ViewItemThreshold   = item.MinimumStockLevel.ToString();
            ViewItemNotes       = item.Note;

            IsViewOverlayOpen = true;
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

        private void RestoreItem(InventoryItem? item)
        {
            if (item == null) return;

            try
            {
                _repository.RestoreItem(item.RawItemId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                LoadInventoryFromDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to restore item: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseOverlay()
        {
            IsItemOverlayOpen       = false;
            HasItemOverlayError     = false;
            ItemOverlayErrorMessage = "";
        }

        // ── Confirm / Success dialog handlers ────────────────────────────────

        private void GoBackConfirm()
        {
            IsConfirmDialogVisible = false;
            switch (_currentOperation)
            {
                case "usage":   IsUsageOverlayOpen   = true; break;
                case "restock": IsRestockOverlayOpen = true; break;
                case "additem": IsItemOverlayOpen    = true; break;
            }
        }

        private void FinalConfirm()
        {
            _pendingFinalConfirm?.Invoke();
        }

        private void SuccessAction()
        {
            IsSuccessPromptVisible = false;
            switch (_currentOperation)
            {
                case "usage":   OpenUsageOverlay();   break;
                case "restock": OpenRestockOverlay(); break;
                case "additem": OpenAddOverlay();     break;
            }
        }

        // ── History overlay handlers ─────────────────────────────────────────

        private void OpenUsageHistory()
        {
            try
            {
                _allUsageHistory = _repository.GetAllUsages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load usage history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            UsageHistorySearch = "";
            ApplyUsageHistoryFilter();
            IsUsageHistoryOpen = true;
        }

        private void OpenRestockHistory()
        {
            try
            {
                _allRestockHistory = _repository.GetAllRestocks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load restock history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            RestockHistorySearch = "";
            ApplyRestockHistoryFilter();
            IsRestockHistoryOpen = true;
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

        // ── Spin box step helpers ────────────────────────────────────────────

        // Steps an integer field by delta, clamped to a minimum of 1.
        private static int StepInt(string current, int delta)
        {
            int.TryParse(current, out int value);
            value += delta;
            return value < 1 ? 1 : value;
        }

        // Steps a decimal field by delta, clamped to a minimum of 0.
        private static decimal StepDecimal(string current, decimal delta)
        {
            decimal.TryParse(current, out decimal value);
            value += delta;
            return value < 0 ? 0 : value;
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
            // Auto-fill the unit price from the selected item's stored information.
            RestockUnitPriceStr = _restockSelectedItem != null
                ? _restockSelectedItem.UnitPrice.ToString("N2")
                : "";
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
            RestockStockAdded          = "1";
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

            var item          = _restockSelectedItem;
            var capturedQty   = qty;
            var capturedDate  = RestockDate.Value;
            var capturedPrice = unitPrice;

            _currentOperation  = "restock";
            ConfirmTitle       = "Restock Item?";
            ConfirmSubtitle    = "Are you sure you want to restock this item?";
            ConfirmField1Label = "Item Name :";
            ConfirmField1Value = item.ItemName;
            ConfirmField2Label = "Item Id:";
            ConfirmField2Value = item.ItemId;
            ConfirmField3Label = "Stock Added:";
            ConfirmField3Value = capturedQty.ToString();
            ConfirmField4Label = "Restock Date:";
            ConfirmField4Value = capturedDate.ToString("MM/dd/yyyy");
            SuccessActionLabel = "Restock Another";

            _pendingFinalConfirm = () => ExecuteRestock(item, capturedQty, capturedDate, capturedPrice);

            IsRestockOverlayOpen   = false;
            IsConfirmDialogVisible = true;
        }

        private void ExecuteRestock(InventoryItem item, int qty, DateTime date, decimal unitPrice)
        {
            try
            {
                _repository.RestockItem(
                    item.RawItemId, item.ItemName, qty,
                    date.ToString("yyyy-MM-dd"),
                    RestockSupplier.Trim(),
                    Convert.ToDouble(unitPrice),
                    RestockNotes.Trim(),
                    item.QuantityInStock,
                    item.MinimumStockLevel);

                LoadInventoryFromDatabase();
                IsConfirmDialogVisible = false;
                SuccessSubMessage      = "Item restocked successfully.";
                IsSuccessPromptVisible = true;
            }
            catch (Exception ex)
            {
                IsConfirmDialogVisible = false;
                IsRestockOverlayOpen   = true;
                RestockErrorMessage    = $"Database error: {ex.Message}";
                HasRestockError        = true;
            }
        }

        // ── Log Usage handlers ───────────────────────────────────────────────

        private void OpenUsageOverlay()
        {
            PopulateActiveItemNames();
            _usageSelectedItem       = null;
            UsageSelectedItemName    = "";
            UsageItemIdDisplay       = "Auto-Filled";
            UsageQuantityUsed        = "1";
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

            var item         = _usageSelectedItem;
            var capturedQty  = qty;
            var capturedDate = UsageDate.Value;

            _currentOperation  = "usage";
            ConfirmTitle       = "Log Item Usage?";
            ConfirmSubtitle    = "Are you sure you want to log the usage of this item?";
            ConfirmField1Label = "Item Name :";
            ConfirmField1Value = item.ItemName;
            ConfirmField2Label = "Item Id:";
            ConfirmField2Value = item.ItemId;
            ConfirmField3Label = "Quantity Used:";
            ConfirmField3Value = capturedQty.ToString();
            ConfirmField4Label = "Usage Date:";
            ConfirmField4Value = capturedDate.ToString("MM/dd/yyyy");
            SuccessActionLabel = "Logged New Item";

            _pendingFinalConfirm = () => ExecuteUsage(item, capturedQty, capturedDate);

            IsUsageOverlayOpen     = false;
            IsConfirmDialogVisible = true;
        }

        private void ExecuteUsage(InventoryItem item, int qty, DateTime date)
        {
            try
            {
                _repository.LogItemUsage(
                    item.RawItemId, item.ItemName, qty,
                    date.ToString("yyyy-MM-dd"),
                    UsageNotes.Trim(),
                    item.QuantityInStock,
                    item.MinimumStockLevel);

                LoadInventoryFromDatabase();
                IsConfirmDialogVisible = false;
                SuccessSubMessage      = "Item logged successfully.";
                IsSuccessPromptVisible = true;
            }
            catch (Exception ex)
            {
                IsConfirmDialogVisible = false;
                IsUsageOverlayOpen     = true;
                UsageErrorMessage      = $"Database error: {ex.Message}";
                HasUsageError          = true;
            }
        }
    }

    public class RecentItemUsage
    {
        public int UsageId { get; set; }
        public string UsageIdDisplay => $"USG-{UsageId:D3}";
        public string ItemId { get; set; } = "";
        public string ItemName { get; set; } = "";
        public int QuantityUsed { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; } = "";
    }

    public class RecentRestockItem
    {
        public int RestockId { get; set; }
        public string RestockIdDisplay => $"RST-{RestockId:D3}";
        public string ItemId { get; set; } = "";
        public string ItemName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int StockAdded { get; set; }
        public string Supplier { get; set; } = "";
        public DateTime Date { get; set; }
        public string Notes { get; set; } = "";
    }
}
