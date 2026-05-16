using CruzNeryClinic.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views.UserManagement
{
    // AddUserOverlayView contains the Add New User modal.
    // PasswordBox values are handled here because PasswordBox.Password
    // cannot be bound directly in WPF.
    public partial class AddUserOverlayView : UserControl
    {
        private UserManagementViewModel? _currentViewModel;

        public AddUserOverlayView()
        {
            InitializeComponent();

            // Listen when DataContext changes so this overlay can safely
            // subscribe to the UserManagementViewModel.
            DataContextChanged += AddUserOverlayView_DataContextChanged;
        }

        private void AddUserOverlayView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from the old ViewModel to avoid duplicate handlers.
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

            // When the Add User overlay closes, clear the PasswordBox controls visually.
            if (e.PropertyName == nameof(UserManagementViewModel.IsAddUserOverlayOpen) &&
                !_currentViewModel.IsAddUserOverlayOpen)
            {
                ClearAddUserPasswordBoxes();
            }

            // When the success prompt closes or Register Again resets the form,
            // clear the PasswordBox controls visually too.
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