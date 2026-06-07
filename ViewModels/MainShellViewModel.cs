using CruzNeryClinic.Services;
using CruzNeryClinic.Views;
using System;
using System.Windows.Controls;

namespace CruzNeryClinic.ViewModels
{
    // MainShellViewModel controls the logged-in shell layout.
    // It keeps the sidebar visible while changing only the right-side module content.
    public class MainShellViewModel : BaseViewModel
    {
        private UserControl _currentModuleView;
        private string _selectedModule = "Dashboard";

        public event Action? LogoutRequested;

        public MainShellViewModel()
        {
            // Default module after login.
            _currentModuleView = CreateDashboardView();
        }

        public UserControl CurrentModuleView
        {
            get => _currentModuleView;
            set => SetProperty(ref _currentModuleView, value);
        }

        public string SelectedModule
        {
            get => _selectedModule;
            set => SetProperty(ref _selectedModule, value);
        }

        public void NavigateTo(string moduleName)
        {
            if (!SessionService.CanAccessModule(moduleName))
                return;

            SelectedModule = moduleName;

            CurrentModuleView = moduleName switch
            {
                "Dashboard" => CreateDashboardView(),
                "ManageUsers" => CreateUserManagementView(),
                "Patients" => CreatePatientManagementView(),
                "Appointment" => CreateAppointmentManagementView(),
                "Billing" => CreateBillingView(),
                "Inventory" => CreateInventoryView(),
                "Maintenance" => CreateMaintenanceView(),
                "Reports" => CreateReportsView(),
                "Help" => CreateHelpView(),
                _ => CreateDashboardView()
            };
        }

        public void Logout()
        {
            SessionService.Logout();
            LogoutRequested?.Invoke();
        }

        private DashboardView CreateDashboardView()
        {
            DashboardViewModel dashboardViewModel = new DashboardViewModel();

            // This makes Dashboard "View All" buttons navigate through the shell.
            // It uses NavigateTo(), the sidebar selected item also updates.
            dashboardViewModel.NavigationRequested += NavigateTo;

            // Lets the Dashboard global search jump to a patient/user record.
            dashboardViewModel.NavigationWithSearchRequested += NavigateToModuleWithSearch;

            DashboardView dashboardView = new DashboardView
            {
                DataContext = dashboardViewModel
            };

            return dashboardView;
        }

        private UserManagementView CreateUserManagementView()
        {
            UserManagementViewModel userManagementViewModel = new UserManagementViewModel();

            UserManagementView userManagementView = new UserManagementView
            {
                DataContext = userManagementViewModel
            };

            return userManagementView;
        }

        private InventoryView CreateInventoryView()
        {
            InventoryViewModel inventoryViewModel = new InventoryViewModel();

            InventoryView inventoryView = new InventoryView
            {
                DataContext = inventoryViewModel
            };

            return inventoryView;
        }

        private PatientManagementView CreatePatientManagementView()
        {
            return new PatientManagementView
            {
                DataContext = new PatientManagementViewModel()
            };
        }

        private AppointmentManagementView CreateAppointmentManagementView()
        {
            AppointmentManagementViewModel appointmentViewModel = new AppointmentManagementViewModel();

            appointmentViewModel.AddPatientRequested += NavigateToPatientsAndOpenAddPatient;

            return new AppointmentManagementView
            {
                DataContext = appointmentViewModel
            };
        }

        private BillingView CreateBillingView()
        {
            return new BillingView
            {
                DataContext = new BillingViewModel()
            };
        }

        private ReportsView CreateReportsView()
        {
            return new ReportsView
            {
                DataContext = new ReportsViewModel()
            };
        }

        private HelpView CreateHelpView()
        {
            return new HelpView
            {
                DataContext = new HelpViewModel()
            };
        }

        // Navigates to the Patients or Manage Users module and pre-fills its
        // search box so the chosen record is shown immediately.
        private void NavigateToModuleWithSearch(string moduleName, string searchKey)
        {
            if (!SessionService.CanAccessModule(moduleName))
                return;

            SelectedModule = moduleName;

            switch (moduleName)
            {
                case "Patients":
                    PatientManagementViewModel patientViewModel = new();
                    patientViewModel.OpenPatientHistoryFromSearchKey(searchKey);
                    CurrentModuleView = new PatientManagementView { DataContext = patientViewModel };
                    break;

                case "ManageUsers":
                    UserManagementViewModel userViewModel = new();
                    userViewModel.OpenUserDetailsFromSearchKey(searchKey);
                    CurrentModuleView = new UserManagementView { DataContext = userViewModel };
                    break;

                default:
                    NavigateTo(moduleName);
                    break;
            }
        }

        private void NavigateToPatientsAndOpenAddPatient()
        {
            SelectedModule = "Patients";

            PatientManagementViewModel patientViewModel = new PatientManagementViewModel();

            PatientManagementView patientManagementView = new PatientManagementView
            {
                DataContext = patientViewModel
            };

            CurrentModuleView = patientManagementView;

            patientViewModel.OpenAddPatientOverlayFromNavigation();
        }

        private UserControl CreatePlaceholderView(string text)
        {
            return new UserControl
            {
                Content = new TextBlock
                {
                    Text = text,
                    FontSize = 40,
                    FontWeight = System.Windows.FontWeights.Bold,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                }
            };
        }

        private MaintenanceView CreateMaintenanceView()
        {
            return new MaintenanceView
            {
                DataContext = new MaintenanceViewModel()
            };
        }
    }
}
