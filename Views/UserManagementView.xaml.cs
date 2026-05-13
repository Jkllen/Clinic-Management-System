using CruzNeryClinic.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    // UserManagementView contains the User Management content.
    // The sidebar is handled by MainShellView.
    public partial class UserManagementView : UserControl
    {
        private UserManagementViewModel? _currentViewModel;

        public UserManagementView()
        {
            InitializeComponent();

            // Listen when DataContext changes so we can safely subscribe
            // to ViewModel property changes.
            DataContextChanged += UserManagementView_DataContextChanged;
        }

        private void UserManagementView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from the old ViewModel to avoid duplicate event handlers.
            if (_currentViewModel != null)
            {
                _currentViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _currentViewModel = e.NewValue as UserManagementViewModel;

            // Subscribe to the new ViewModel.
            if (_currentViewModel != null)
            {
                _currentViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_currentViewModel == null)
                return;

            // When the overlay is closed, clear the PasswordBox controls visually.
            if (e.PropertyName == nameof(UserManagementViewModel.IsAddUserOverlayOpen) &&
                !_currentViewModel.IsAddUserOverlayOpen)
            {
                ClearAddUserPasswordBoxes();
            }

            // When success prompt closes or Register Again resets the form,
            // clear the PasswordBox controls too.
            if (e.PropertyName == nameof(UserManagementViewModel.IsSuccessPromptVisible) &&
                !_currentViewModel.IsSuccessPromptVisible)
            {
                ClearAddUserPasswordBoxes();
            }
        }

        private void AddUserPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel &&
                sender is PasswordBox passwordBox)
            {
                viewModel.NewUserPassword = passwordBox.Password;
            }
        }

        private void AddUserConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel &&
                sender is PasswordBox passwordBox)
            {
                viewModel.NewUserConfirmPassword = passwordBox.Password;
            }
        }

        private void ClearAddUserPasswordBoxes()
        {
            AddUserPasswordBox.Password = string.Empty;
            AddUserConfirmPasswordBox.Password = string.Empty;
        }
    }
}