using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models.Dashboard;
using CruzNeryClinic.Repositories;
using CruzNeryClinic.Services;
using System;
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

        private int totalPatients;
        private int newPatientsThisMonth;
        private int pendingPayments;
        private decimal totalUnpaidBalance;
        private int lowStockItemCount;
        private string searchText = string.Empty;

        private DateTime _selectedCalendarDate = DateTime.Today;
        private string _selectedAppointmentHeader = DateTime.Today.ToString("dddd | MMMM d, yyyy").ToUpper();

        public event Action? LogoutRequested;

        public DashboardViewModel()
        {
            dashboardRepository = new DashboardRepository();

            LowStockItems = new ObservableCollection<DashboardLowStockItem>();
            RecentActivities = new ObservableCollection<DashboardActivityItem>();

            TodayQueue = new ObservableCollection<DashboardQueueItem>();
            SelectedDateAppointments = new ObservableCollection<DashboardQueueItem>();

            RecentTransactions = new ObservableCollection<DashboardTransactionItem>();

            RefreshCommand = new RelayCommand(LoadDashboard);
            LogoutCommand = new RelayCommand(Logout);

            LoadDashboard();
        }

        public string CurrentUserName => SessionService.GetCurrentUserFullName();

        public string CurrentUserCode => SessionService.GetCurrentUserCode();

        public string CurrentUserRole => SessionService.GetCurrentUserRole();

        public string SelectedAppointmentHeader{
            get => _selectedAppointmentHeader;
            set => SetProperty(ref _selectedAppointmentHeader, value);
        }

        public string CurrentMonth => SelectedCalendarDate.ToString("MMMM").ToUpper();

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
            set => SetProperty(ref searchText, value);
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
                    OnPropertyChanged(nameof(CurrentMonth));
                }
            }
        }

        public ObservableCollection<DashboardLowStockItem> LowStockItems { get; }

        public ObservableCollection<DashboardActivityItem> RecentActivities { get; }

        public ObservableCollection<DashboardQueueItem> TodayQueue { get; }

        public ObservableCollection<DashboardQueueItem> SelectedDateAppointments { get; }

        public ObservableCollection<DashboardTransactionItem> RecentTransactions { get; }



        public ICommand RefreshCommand { get; }

        public ICommand LogoutCommand { get; }

        private void LoadDashboard()
        {
            DashboardSummary summary = dashboardRepository.GetDashboardSummary();

            TotalPatients = summary.TotalPatients;
            NewPatientsThisMonth = summary.NewPatientsThisMonth;
            PendingPayments = summary.PendingPayments;
            TotalUnpaidBalance = summary.TotalUnpaidBalance;
            LowStockItemCount = summary.LowStockItemCount;

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

        private void Logout()
        {
            SessionService.Logout();
            LogoutRequested?.Invoke();
        }
    }
}