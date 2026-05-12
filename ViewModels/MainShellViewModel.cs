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
                "ManageUsers" => CreatePlaceholderView("Manage Users Screen Next"),
                "Patients" => CreatePlaceholderView("Patients Screen Next"),
                "Appointment" => CreatePlaceholderView("Appointment Screen Next"),
                "Billing" => CreatePlaceholderView("Billing Screen Next"),
                "Inventory" => CreatePlaceholderView("Inventory Screen Next"),
                "Maintenance" => CreatePlaceholderView("Maintenance Screen Next"),
                "Reports" => CreatePlaceholderView("Reports Screen Next"),
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
            // Because it uses NavigateTo(), the sidebar selected item also updates.
            dashboardViewModel.NavigationRequested += NavigateTo;

            DashboardView dashboardView = new DashboardView
            {
                DataContext = dashboardViewModel
            };

            return dashboardView;
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
    }
}