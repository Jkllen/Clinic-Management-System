using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly ReportsRepository _repository = new();

        public event Action? ChartDataRefreshed;

        // ── Tab selection ──────────────────────────────────────────────────────

        private bool _isPatientVisitsSelected = true;
        private bool _isTransactionReportsSelected;
        private bool _isInventoryReportsSelected;
        private bool _isUserActivityLogSelected;

        public bool IsPatientVisitsSelected
        {
            get => _isPatientVisitsSelected;
            set => SetProperty(ref _isPatientVisitsSelected, value);
        }
        public bool IsTransactionReportsSelected
        {
            get => _isTransactionReportsSelected;
            set => SetProperty(ref _isTransactionReportsSelected, value);
        }
        public bool IsInventoryReportsSelected
        {
            get => _isInventoryReportsSelected;
            set => SetProperty(ref _isInventoryReportsSelected, value);
        }
        public bool IsUserActivityLogSelected
        {
            get => _isUserActivityLogSelected;
            set => SetProperty(ref _isUserActivityLogSelected, value);
        }

        // ── Filter state ───────────────────────────────────────────────────────

        private bool _isThisMonthSelected = true;
        private bool _isThisYearSelected;
        private bool _isCustomRangeSelected;
        private DateTime? _filterFromDate;
        private DateTime? _filterToDate;
        private string _showingText = "Showing: Monthly Report";
        private bool _isDateRangeVisible;
        private int _selectedFilterMonthIndex = DateTime.Now.Month - 1;
        private int _selectedFilterYear = DateTime.Now.Year;

        public List<string> MonthNames { get; } = new List<string>
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        public List<int> AvailableYears { get; } = new List<int>(
            System.Linq.Enumerable.Range(2020, DateTime.Now.Year - 2020 + 2));

        public int SelectedFilterMonthIndex
        {
            get => _selectedFilterMonthIndex;
            set
            {
                SetProperty(ref _selectedFilterMonthIndex, value);
                if (IsThisMonthSelected) UpdateMonthRange();
            }
        }

        public int SelectedFilterYear
        {
            get => _selectedFilterYear;
            set
            {
                SetProperty(ref _selectedFilterYear, value);
                if (IsThisMonthSelected) UpdateMonthRange();
                else if (IsThisYearSelected) UpdateYearRange();
            }
        }

        public bool IsThisMonthSelected
        {
            get => _isThisMonthSelected;
            set => SetProperty(ref _isThisMonthSelected, value);
        }
        public bool IsThisYearSelected
        {
            get => _isThisYearSelected;
            set => SetProperty(ref _isThisYearSelected, value);
        }
        public bool IsCustomRangeSelected
        {
            get => _isCustomRangeSelected;
            set => SetProperty(ref _isCustomRangeSelected, value);
        }
        public DateTime? FilterFromDate
        {
            get => _filterFromDate;
            set
            {
                SetProperty(ref _filterFromDate, value);
                if (IsCustomRangeSelected && _filterFromDate.HasValue && _filterToDate.HasValue)
                {
                    ShowingText = $"Showing: {_filterFromDate:MMM dd} – {_filterToDate:MMM dd, yyyy}";
                    LoadData();
                }
            }
        }
        public DateTime? FilterToDate
        {
            get => _filterToDate;
            set
            {
                SetProperty(ref _filterToDate, value);
                if (IsCustomRangeSelected && _filterFromDate.HasValue && _filterToDate.HasValue)
                {
                    ShowingText = $"Showing: {_filterFromDate:MMM dd} – {_filterToDate:MMM dd, yyyy}";
                    LoadData();
                }
            }
        }
        public bool IsDateRangeVisible
        {
            get => _isDateRangeVisible;
            set => SetProperty(ref _isDateRangeVisible, value);
        }
        public string ShowingText
        {
            get => _showingText;
            set => SetProperty(ref _showingText, value);
        }

        // ── Chart & table data ─────────────────────────────────────────────────

        public ObservableCollection<PatientVisitReportItem> PatientVisitsItems { get; } = new();
        public List<DualChartDataPoint> PatientVisitTrend { get; private set; } = new();

        public ObservableCollection<TransactionReportItem> TransactionItems { get; } = new();
        public List<ChartDataPoint> RevenueTrend { get; private set; } = new();
        public List<ChartDataPoint> DailyTransactionCounts { get; private set; } = new();

        public ObservableCollection<InventoryReportItem> InventoryItems { get; } = new();
        public List<ChartDataPoint> InventoryChartData { get; private set; } = new();

        public ObservableCollection<ActivityLogReportItem> ActivityLogItems { get; } = new();
        public List<PieChartSlice> ActivityByType { get; private set; } = new();
        public List<ChartDataPoint> ActivityByModule { get; private set; } = new();

        // ── Commands ───────────────────────────────────────────────────────────

        public ICommand SelectPatientVisitsCommand { get; }
        public ICommand SelectTransactionReportsCommand { get; }
        public ICommand SelectInventoryReportsCommand { get; }
        public ICommand SelectUserActivityLogCommand { get; }
        public ICommand ApplyThisMonthCommand { get; }
        public ICommand ApplyThisYearCommand { get; }
        public ICommand ApplyCustomRangeCommand { get; }

        public ReportsViewModel()
        {
            SelectPatientVisitsCommand = new RelayCommand(SelectPatientVisits);
            SelectTransactionReportsCommand = new RelayCommand(SelectTransactionReports);
            SelectInventoryReportsCommand = new RelayCommand(SelectInventoryReports);
            SelectUserActivityLogCommand = new RelayCommand(SelectUserActivityLog);
            ApplyThisMonthCommand = new RelayCommand(ApplyThisMonth);
            ApplyThisYearCommand = new RelayCommand(ApplyThisYear);
            ApplyCustomRangeCommand = new RelayCommand(ApplyCustomRange);

            ApplyThisMonth();
        }

        // ── Tab switching ──────────────────────────────────────────────────────

        private void SelectPatientVisits()
        {
            IsPatientVisitsSelected = true;
            IsTransactionReportsSelected = false;
            IsInventoryReportsSelected = false;
            IsUserActivityLogSelected = false;
            LoadData();
        }

        private void SelectTransactionReports()
        {
            IsPatientVisitsSelected = false;
            IsTransactionReportsSelected = true;
            IsInventoryReportsSelected = false;
            IsUserActivityLogSelected = false;
            LoadData();
        }

        private void SelectInventoryReports()
        {
            IsPatientVisitsSelected = false;
            IsTransactionReportsSelected = false;
            IsInventoryReportsSelected = true;
            IsUserActivityLogSelected = false;
            LoadData();
        }

        private void SelectUserActivityLog()
        {
            IsPatientVisitsSelected = false;
            IsTransactionReportsSelected = false;
            IsInventoryReportsSelected = false;
            IsUserActivityLogSelected = true;
            LoadData();
        }

        // ── Filter switching ───────────────────────────────────────────────────

        private void ApplyThisMonth()
        {
            IsThisMonthSelected = true;
            IsThisYearSelected = false;
            IsCustomRangeSelected = false;
            _selectedFilterMonthIndex = DateTime.Now.Month - 1;
            _selectedFilterYear = DateTime.Now.Year;
            OnPropertyChanged(nameof(SelectedFilterMonthIndex));
            OnPropertyChanged(nameof(SelectedFilterYear));
            UpdateMonthRange();
        }

        private void ApplyThisYear()
        {
            IsThisMonthSelected = false;
            IsThisYearSelected = true;
            IsCustomRangeSelected = false;
            _selectedFilterYear = DateTime.Now.Year;
            OnPropertyChanged(nameof(SelectedFilterYear));
            UpdateYearRange();
        }

        private void ApplyCustomRange()
        {
            IsThisMonthSelected = false;
            IsThisYearSelected = false;
            IsCustomRangeSelected = true;
            ShowingText = "Showing: Custom Range";
        }

        private void UpdateMonthRange()
        {
            int month = _selectedFilterMonthIndex + 1;
            var first = new DateTime(_selectedFilterYear, month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            _filterFromDate = first;
            _filterToDate = last;
            OnPropertyChanged(nameof(FilterFromDate));
            OnPropertyChanged(nameof(FilterToDate));
            ShowingText = $"Showing: {first:MMMM yyyy}";
            LoadData();
        }

        private void UpdateYearRange()
        {
            _filterFromDate = new DateTime(_selectedFilterYear, 1, 1);
            _filterToDate = new DateTime(_selectedFilterYear, 12, 31);
            OnPropertyChanged(nameof(FilterFromDate));
            OnPropertyChanged(nameof(FilterToDate));
            ShowingText = $"Showing: {_selectedFilterYear}";
            LoadData();
        }

        // ── Data loading ───────────────────────────────────────────────────────

        private (string from, string to) GetDateRange()
        {
            if (_filterFromDate.HasValue && _filterToDate.HasValue)
                return (_filterFromDate.Value.ToString("yyyy-MM-dd"), _filterToDate.Value.ToString("yyyy-MM-dd"));

            var now = DateTime.Now;
            var first = new DateTime(now.Year, now.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            return (first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd"));
        }

        private void LoadData()
        {
            var (from, to) = GetDateRange();

            if (IsPatientVisitsSelected)
            {
                PatientVisitsItems.Clear();
                foreach (var item in _repository.GetPatientVisits(from, to))
                    PatientVisitsItems.Add(item);
                PatientVisitTrend = _repository.GetPatientVisitTrend(from, to);
            }
            else if (IsTransactionReportsSelected)
            {
                TransactionItems.Clear();
                foreach (var item in _repository.GetTransactions(from, to))
                    TransactionItems.Add(item);
                RevenueTrend = _repository.GetRevenueTrend(from, to);
                DailyTransactionCounts = _repository.GetDailyTransactionCounts(from, to);
            }
            else if (IsInventoryReportsSelected)
            {
                InventoryItems.Clear();
                foreach (var item in _repository.GetInventoryItems())
                    InventoryItems.Add(item);
                InventoryChartData = _repository.GetInventoryChartData();
            }
            else if (IsUserActivityLogSelected)
            {
                ActivityLogItems.Clear();
                foreach (var item in _repository.GetActivityLogs(from, to))
                    ActivityLogItems.Add(item);
                ActivityByType = _repository.GetActivityByType(from, to);
                ActivityByModule = _repository.GetActivityByModule(from, to);
            }

            ChartDataRefreshed?.Invoke();
        }
    }
}
