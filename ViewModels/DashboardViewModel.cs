using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Models.Dashboard;
using CruzNeryClinic.Repositories;
using CruzNeryClinic.Services;
using CruzNeryClinic.Views.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    // DashboardViewModel controls the Dashboard screen.
    // It loads summary cards, today's queue, low-stock items,
    // recent activity logs, and recent patient transactions.
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DashboardRepository dashboardRepository;
        private readonly ReportsRepository reportsRepository = new();

        // Raised after chart data is reloaded so the view can redraw its canvases.
        public event Action? ChartDataRefreshed;

        private int totalPatients;
        private int newPatientsThisMonth;
        private int pendingPayments;
        private decimal totalUnpaidBalance;
        private int lowStockItemCount;
        private string searchText = string.Empty;

        public bool CanViewAdminDashboardAnalytics => SessionService.IsAdmin;
        public bool CanViewStaffAppointmentCalendar => !SessionService.IsAdmin;

        private DateTime _selectedCalendarDate = DateTime.Today;
        private DateTime _displayedStaffCalendarMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        private string _selectedAppointmentHeader = DateTime.Today.ToString("dddd | MMMM d, yyyy").ToUpper();

        public event Action<string>? NavigationRequested;

        // Raised when a global-search suggestion is clicked. Carries the target
        // module ("Patients" / "ManageUsers") and the record code to pre-fill in
        // that module's own search box.
        public event Action<string, string>? NavigationWithSearchRequested;

        // Raised when a Dashboard "View All" opens a specific Reports tab.
        // Carries the report key ("PatientVisits" / "Transaction" / "UserActivity").
        public event Action<string>? NavigationToReportRequested;

        public event Action? LogoutRequested;

        public DashboardViewModel()
        {
            dashboardRepository = new DashboardRepository();

            LowStockItems = new ObservableCollection<DashboardLowStockItem>();
            RecentActivities = new ObservableCollection<DashboardActivityItem>();
            RecentActivityLog = new ObservableCollection<ActivityLogReportItem>();

            TodayQueue = new ObservableCollection<DashboardQueueItem>();
            SelectedDateAppointments = new ObservableCollection<DashboardQueueItem>();
            WeekDays = new ObservableCollection<DashboardDayItem>();
            StaffCalendarDays = new ObservableCollection<AppointmentCalendarDay>();

            RecentTransactions = new ObservableCollection<DashboardTransactionItem>();

            SearchResults = new ObservableCollection<DashboardSearchResultItem>();
            PeriodOptions = new ObservableCollection<string> { "Month", "Year" };

            SelectSearchResultCommand = new RelayCommand<DashboardSearchResultItem>(SelectSearchResult);

            RefreshCommand = new RelayCommand(LoadDashboard);
            LogoutCommand = new RelayCommand(Logout);

            ViewInventoryCommand = new RelayCommand(() => NavigateTo("Inventory"));
            ViewBillingCommand = new RelayCommand(() => NavigateTo("Billing"));
            ViewReportsCommand = new RelayCommand(() => NavigateTo("Reports"));
            ViewPatientVisitsReportCommand = new RelayCommand(() => NavigateToReport("PatientVisits"));
            ViewRevenueReportCommand = new RelayCommand(() => NavigateToReport("Transaction"));
            ViewActivityLogReportCommand = new RelayCommand(() => NavigateToReport("UserActivity"));

            OpenChartDetailCommand = new RelayCommand<string>(OpenChartDetail);
            CloseChartDetailCommand = new RelayCommand(() => IsChartDetailOpen = false);

            SelectDayCommand = new RelayCommand<DashboardDayItem>(SelectDay);
            PreviousWeekCommand = new RelayCommand(() => SelectedCalendarDate = SelectedCalendarDate.AddDays(-7));
            NextWeekCommand = new RelayCommand(() => SelectedCalendarDate = SelectedCalendarDate.AddDays(7));
            SelectStaffCalendarDateCommand = new RelayCommand<AppointmentCalendarDay>(SelectStaffCalendarDate);
            PreviousStaffCalendarMonthCommand = new RelayCommand(() => DisplayedStaffCalendarMonth = DisplayedStaffCalendarMonth.AddMonths(-1));
            NextStaffCalendarMonthCommand = new RelayCommand(() => DisplayedStaffCalendarMonth = DisplayedStaffCalendarMonth.AddMonths(1));

            ShowQueueTabCommand = new RelayCommand(() => IsQueueTabSelected = true);
            ShowTransactionsTabCommand = new RelayCommand(() => IsQueueTabSelected = false);

            BuildWeekStrip();
            BuildStaffCalendarDays();
            LoadDashboard();
        }

        // ── Tab selection for the lower table section ────────────────────────────
        private bool _isQueueTabSelected = true;
        public bool IsQueueTabSelected
        {
            get => _isQueueTabSelected;
            set => SetProperty(ref _isQueueTabSelected, value);
        }

        public string CurrentTimeText => DateTime.Now.ToString("hh:mm tt");

        public string SelectedDateText => SelectedCalendarDate.ToString("MM/dd/yyyy");

        public string CurrentUserName => SessionService.GetCurrentUserFullName();

        public string CurrentUserCode => SessionService.GetCurrentUserCode();

        public string CurrentUserRole => SessionService.GetCurrentUserRole();

        public string SelectedAppointmentHeader{
            get => _selectedAppointmentHeader;
            set => SetProperty(ref _selectedAppointmentHeader, value);
        }

        public string CurrentMonth => SelectedCalendarDate.ToString("MMMM").ToUpper();

        public string StaffCalendarMonthTitle => DisplayedStaffCalendarMonth.ToString("MMMM yyyy");

        public int TotalPatients
        {
            get => totalPatients;
            set => SetProperty(ref totalPatients, value);
        }

        public int NewPatientsThisMonth
        {
            get => newPatientsThisMonth;
            set => SetProperty(ref newPatientsThisMonth, value);
        }

        // Follows the Month/Year selector, e.g. "New Patients This Month".
        private string newPatientsLabel = "New Patients This Month";
        public string NewPatientsLabel
        {
            get => newPatientsLabel;
            set => SetProperty(ref newPatientsLabel, value);
        }

        public int PendingPayments
        {
            get => pendingPayments;
            set => SetProperty(ref pendingPayments, value);
        }

        public decimal TotalUnpaidBalance
        {
            get => totalUnpaidBalance;
            set => SetProperty(ref totalUnpaidBalance, value);
        }

        public int LowStockItemCount
        {
            get => lowStockItemCount;
            set => SetProperty(ref lowStockItemCount, value);
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                if (SetProperty(ref searchText, value))
                    RunGlobalSearch();
            }
        }

        // Global search suggestions (patients + users) shown in the popup.
        public ObservableCollection<DashboardSearchResultItem> SearchResults { get; }

        private bool isSearchPopupOpen;
        public bool IsSearchPopupOpen
        {
            get => isSearchPopupOpen;
            set => SetProperty(ref isSearchPopupOpen, value);
        }

        // Period selector beside the search bar. Drives the date range used by
        // the "New Patients" card and the admin analytics (visit trend, revenue
        // trend, activity log).
        public ObservableCollection<string> PeriodOptions { get; }

        private string selectedPeriod = "Month";
        public string SelectedPeriod
        {
            get => selectedPeriod;
            set
            {
                if (!SetProperty(ref selectedPeriod, value))
                    return;

                RefreshNewPatientsForPeriod();

                if (CanViewAdminDashboardAnalytics)
                    LoadAdminAnalytics();
            }
        }

        // Selected date from the dashboard calendar.
        // When the date changes, the appointment list on the right updates.
        public DateTime SelectedCalendarDate
        {
            get => _selectedCalendarDate;
            set
            {
                if (SetProperty(ref _selectedCalendarDate, value))
                {
                    SelectedAppointmentHeader = value.ToString("dddd | MMMM d, yyyy").ToUpper();
                    LoadAppointmentsForSelectedDate();
                    BuildWeekStrip();
                    BuildStaffCalendarDays();
                    OnPropertyChanged(nameof(CurrentMonth));
                    OnPropertyChanged(nameof(SelectedDateText));
                }
            }
        }

        public DateTime DisplayedStaffCalendarMonth
        {
            get => _displayedStaffCalendarMonth;
            set
            {
                DateTime normalizedValue = new(value.Year, value.Month, 1);

                if (SetProperty(ref _displayedStaffCalendarMonth, normalizedValue))
                {
                    OnPropertyChanged(nameof(StaffCalendarMonthTitle));
                    BuildStaffCalendarDays();
                }
            }
        }

        public ObservableCollection<DashboardLowStockItem> LowStockItems { get; }

        public ObservableCollection<DashboardActivityItem> RecentActivities { get; }

        // Richer activity rows (Timestamp/Name/Action/Module/Details) for the admin
        // Recent Activity Log panel on the right of the dashboard.
        public ObservableCollection<ActivityLogReportItem> RecentActivityLog { get; }

        public ObservableCollection<DashboardQueueItem> TodayQueue { get; }

        public ObservableCollection<DashboardQueueItem> SelectedDateAppointments { get; }

        // The 7-day strip shown in the Appointment List header.
        public ObservableCollection<DashboardDayItem> WeekDays { get; }

        public ObservableCollection<AppointmentCalendarDay> StaffCalendarDays { get; }

        public ObservableCollection<DashboardTransactionItem> RecentTransactions { get; }

        // Chart data (current month) for the admin analytics row.
        public List<DualChartDataPoint> PatientVisitTrend { get; private set; } = new();
        public List<ChartDataPoint> RevenueTrend { get; private set; } = new();

        // Plain-language interpretation shown under each chart.
        private string _patientVisitInsight = "";
        public string PatientVisitInsight
        {
            get => _patientVisitInsight;
            private set => SetProperty(ref _patientVisitInsight, value);
        }

        private string _revenueInsight = "";
        public string RevenueInsight
        {
            get => _revenueInsight;
            private set => SetProperty(ref _revenueInsight, value);
        }

        // ── Chart detail panel (opens when a chart is clicked) ──────────────────
        public event Action? ChartDetailRequested;

        public string SelectedChartKey { get; private set; } = "";

        private bool _isChartDetailOpen;
        public bool IsChartDetailOpen
        {
            get => _isChartDetailOpen;
            set => SetProperty(ref _isChartDetailOpen, value);
        }

        private string _chartDetailTitle = "";
        public string ChartDetailTitle
        {
            get => _chartDetailTitle;
            private set => SetProperty(ref _chartDetailTitle, value);
        }

        private string _chartDetailDescription = "";
        public string ChartDetailDescription
        {
            get => _chartDetailDescription;
            private set => SetProperty(ref _chartDetailDescription, value);
        }

        private string _chartDetailInsight = "";
        public string ChartDetailInsight
        {
            get => _chartDetailInsight;
            private set => SetProperty(ref _chartDetailInsight, value);
        }

        public ObservableCollection<KeyFigure> ChartDetailFigures { get; } = new();

        public ICommand OpenChartDetailCommand { get; }
        public ICommand CloseChartDetailCommand { get; }

        public ICommand RefreshCommand { get; }

        public ICommand LogoutCommand { get; }

        public ICommand ViewInventoryCommand { get; }
        public ICommand ViewBillingCommand { get; }
        public ICommand ViewReportsCommand { get; }
        public ICommand ViewPatientVisitsReportCommand { get; }
        public ICommand ViewRevenueReportCommand { get; }
        public ICommand ViewActivityLogReportCommand { get; }

        public ICommand SelectDayCommand { get; }
        public ICommand PreviousWeekCommand { get; }
        public ICommand NextWeekCommand { get; }
        public ICommand SelectStaffCalendarDateCommand { get; }
        public ICommand PreviousStaffCalendarMonthCommand { get; }
        public ICommand NextStaffCalendarMonthCommand { get; }

        public ICommand ShowQueueTabCommand { get; }
        public ICommand ShowTransactionsTabCommand { get; }

        public ICommand SelectSearchResultCommand { get; }

        private void LoadDashboard()
        {
            DashboardSummary summary = dashboardRepository.GetDashboardSummary();

            TotalPatients = summary.TotalPatients;
            PendingPayments = summary.PendingPayments;
            TotalUnpaidBalance = summary.TotalUnpaidBalance;
            LowStockItemCount = summary.LowStockItemCount;

            // New Patients count follows the selected Month/Year period.
            RefreshNewPatientsForPeriod();

            LowStockItems.Clear();
            foreach (DashboardLowStockItem item in dashboardRepository.GetLowStockItems())
                LowStockItems.Add(item);

            RecentActivities.Clear();
            foreach (DashboardActivityItem item in dashboardRepository.GetRecentActivities())
                RecentActivities.Add(item);

            TodayQueue.Clear();
            foreach (DashboardQueueItem item in dashboardRepository.GetTodayQueue())
                TodayQueue.Add(item);

            LoadAppointmentsForSelectedDate();

            RecentTransactions.Clear();
            foreach (DashboardTransactionItem item in dashboardRepository.GetRecentPatientTransactions())
                RecentTransactions.Add(item);

            // Admin-only analytics: activity log table + charts for the current month.
            if (CanViewAdminDashboardAnalytics)
                LoadAdminAnalytics();
        }

        // Returns the inclusive yyyy-MM-dd date range for the selected period:
        // the current month, or the whole current year.
        private (string From, string To) GetSelectedPeriodRange()
        {
            DateTime now = DateTime.Now;

            DateTime fromDate;
            DateTime toDate;

            if (SelectedPeriod == "Year")
            {
                fromDate = new DateTime(now.Year, 1, 1);
                toDate = new DateTime(now.Year, 12, 31);
            }
            else
            {
                fromDate = new DateTime(now.Year, now.Month, 1);
                toDate = fromDate.AddMonths(1).AddDays(-1);
            }

            return (fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));
        }

        // Updates the "New Patients" card count and label for the selected period.
        private void RefreshNewPatientsForPeriod()
        {
            (string from, string to) = GetSelectedPeriodRange();

            NewPatientsThisMonth = dashboardRepository.GetNewPatientsCount(from, to);
            NewPatientsLabel = SelectedPeriod == "Year"
                ? "New Patients This Year"
                : "New Patients This Month";
        }

        // Loads the richer activity log and the chart data shown only to admins.
        private void LoadAdminAnalytics()
        {
            (string from, string to) = GetSelectedPeriodRange();

            RecentActivityLog.Clear();
            int shown = 0;
            foreach (ActivityLogReportItem item in reportsRepository.GetActivityLogs(from, to))
            {
                RecentActivityLog.Add(item);
                if (++shown >= 6) break;
            }

            PatientVisitTrend = reportsRepository.GetPatientVisitTrend(from, to);
            RevenueTrend = reportsRepository.GetRevenueTrend(from, to);

            PatientVisitInsight = ChartInsight.SummarizeDual(PatientVisitTrend, "scheduled", "walk-in");
            RevenueInsight = ChartInsight.Summarize(RevenueTrend, "days", "₱");

            ChartDataRefreshed?.Invoke();
        }

        // Selects a day from the Appointment List strip.
        private void SelectDay(DashboardDayItem? day)
        {
            if (day != null)
                SelectedCalendarDate = day.Date;
        }

        private void SelectStaffCalendarDate(AppointmentCalendarDay? day)
        {
            if (day == null)
                return;

            if (day.Date.Month != DisplayedStaffCalendarMonth.Month ||
                day.Date.Year != DisplayedStaffCalendarMonth.Year)
            {
                DisplayedStaffCalendarMonth = day.Date;
            }

            SelectedCalendarDate = day.Date;
        }

        // Rebuilds the 7-day strip (Saturday-first) around the selected date.
        private void BuildWeekStrip()
        {
            WeekDays.Clear();

            int offset = ((int)SelectedCalendarDate.DayOfWeek + 1) % 7; // days since the most recent Saturday
            DateTime start = SelectedCalendarDate.Date.AddDays(-offset);

            for (int i = 0; i < 7; i++)
            {
                DateTime day = start.AddDays(i);
                WeekDays.Add(new DashboardDayItem
                {
                    Date = day,
                    IsSelected = day == SelectedCalendarDate.Date,
                });
            }
        }

        private void BuildStaffCalendarDays()
        {
            StaffCalendarDays.Clear();

            DateTime firstDayOfMonth = new(
                DisplayedStaffCalendarMonth.Year,
                DisplayedStaffCalendarMonth.Month,
                1);

            int daysFromSunday = (int)firstDayOfMonth.DayOfWeek;
            DateTime calendarStartDate = firstDayOfMonth.AddDays(-daysFromSunday);

            Dictionary<DateTime, int> appointmentCounts =
                dashboardRepository.GetAppointmentCountsByDate(
                    DisplayedStaffCalendarMonth.Year,
                    DisplayedStaffCalendarMonth.Month);

            for (int i = 0; i < 42; i++)
            {
                DateTime date = calendarStartDate.AddDays(i).Date;
                bool isCurrentMonth =
                    date.Month == DisplayedStaffCalendarMonth.Month &&
                    date.Year == DisplayedStaffCalendarMonth.Year;

                int appointmentCount = 0;
                if (isCurrentMonth && appointmentCounts.TryGetValue(date, out int count))
                    appointmentCount = count;

                StaffCalendarDays.Add(new AppointmentCalendarDay
                {
                    Date = date,
                    AppointmentCount = appointmentCount,
                    IsCurrentMonth = isCurrentMonth,
                    IsToday = date == DateTime.Today,
                    IsSelected = date == SelectedCalendarDate.Date
                });
            }
        }

        // Loads appointments for the selected date in the calendar.
        private void LoadAppointmentsForSelectedDate()
        {
            SelectedDateAppointments.Clear();

            foreach (DashboardQueueItem item in dashboardRepository.GetAppointmentsByDate(SelectedCalendarDate))
            {
                SelectedDateAppointments.Add(item);
            }
        }

        // Runs the global search and fills the suggestion popup. Users are only
        // searched when the current account can open the Manage Users module.
        private void RunGlobalSearch()
        {
            SearchResults.Clear();

            string keyword = (SearchText ?? string.Empty).Trim();
            if (keyword.Length < 2)
            {
                IsSearchPopupOpen = false;
                return;
            }

            bool includeUsers = SessionService.CanAccessModule("ManageUsers");

            foreach (DashboardSearchResultItem item in dashboardRepository.SearchPatientsAndUsers(keyword, includeUsers))
                SearchResults.Add(item);

            IsSearchPopupOpen = SearchResults.Count > 0;
        }

        // Navigates to the patient/user record's module, pre-filling its search.
        private void SelectSearchResult(DashboardSearchResultItem? item)
        {
            if (item == null)
                return;

            IsSearchPopupOpen = false;
            SearchResults.Clear();

            if (!SessionService.CanAccessModule(item.TargetModule))
                return;

            NavigationWithSearchRequested?.Invoke(item.TargetModule, item.SearchKey);
        }

        private void NavigateTo(string moduleName)
        {
            if (!SessionService.CanAccessModule(moduleName))
                return;

            NavigationRequested?.Invoke(moduleName);
        }

        // Opens the right-side chart detail panel for the clicked chart.
        private void OpenChartDetail(string? key)
        {
            if (string.IsNullOrEmpty(key)) return;

            ChartDetailFigures.Clear();

            switch (key)
            {
                case "patientVisit":
                    ChartDetailTitle = "Patient Visits Trend";
                    ChartDetailInsight = PatientVisitInsight;
                    foreach (var f in ChartInsight.KeyFigures(PatientVisitTrend, "scheduled", "walk-in"))
                        ChartDetailFigures.Add(f);
                    break;
                case "revenue":
                    ChartDetailTitle = "Revenue Trend";
                    ChartDetailInsight = RevenueInsight;
                    foreach (var f in ChartInsight.KeyFigures(RevenueTrend, "₱"))
                        ChartDetailFigures.Add(f);
                    break;
                default:
                    return;
            }

            ChartDetailDescription = ChartInsight.Description(key);
            SelectedChartKey = key;
            IsChartDetailOpen = true;
            ChartDetailRequested?.Invoke();
        }

        // Opens the Reports module on a specific report tab.
        private void NavigateToReport(string reportKey)
        {
            if (!SessionService.CanAccessModule("Reports"))
                return;

            NavigationToReportRequested?.Invoke(reportKey);
        }

        private void Logout()
        {
            SessionService.Logout();
            LogoutRequested?.Invoke();
        }
    }
}
