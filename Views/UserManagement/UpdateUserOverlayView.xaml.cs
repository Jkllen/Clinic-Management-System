using CruzNeryClinic.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views.UserManagement
{
    // UpdateUserOverlayView contains the Update User modal.
    // PasswordBox values are handled here because PasswordBox.Password
    // cannot be bound directly in WPF.
    public partial class UpdateUserOverlayView : UserControl
    {
        private UserManagementViewModel? currentViewModel;

        private bool isPasswordSyncing;

        public UpdateUserOverlayView()
        {
            InitializeComponent();

            DataContextChanged += UpdateUserOverlayView_DataContextChanged;
        }

        private void UpdateUserOverlayView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (currentViewModel != null)
            {
                currentViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            currentViewModel = e.NewValue as UserManagementViewModel;

            if (currentViewModel != null)
            {
                currentViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (currentViewModel == null)
                return;

            if (e.PropertyName == nameof(UserManagementViewModel.IsUpdateUserOverlayOpen) &&
                !currentViewModel.IsUpdateUserOverlayOpen)
            {
                ClearUpdatePasswordBoxes();
            }

            if (e.PropertyName == nameof(UserManagementViewModel.IsUpdateSuccessPromptVisible) &&
                !currentViewModel.IsUpdateSuccessPromptVisible)
            {
                ClearUpdatePasswordBoxes();
            }
        }

        private void UpdateOldPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isPasswordSyncing)
                return;

            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.UpdateOldPassword = UpdateOldPasswordBox.Password;
            }
        }

        private void UpdateNewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isPasswordSyncing)
                return;

            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.UpdateNewPassword = UpdateNewPasswordBox.Password;
            }
        }

        private void UpdateConfirmNewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isPasswordSyncing)
                return;

            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.UpdateConfirmNewPassword = UpdateConfirmNewPasswordBox.Password;
            }
        }

        private void UpdateOldPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPasswordSyncing)
                return;

            if (sender is TextBox textBox)
            {
                SyncPasswordBox(UpdateOldPasswordBox, textBox.Text);
            }
        }

        private void UpdateNewPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPasswordSyncing)
                return;

            if (sender is TextBox textBox)
            {
                SyncPasswordBox(UpdateNewPasswordBox, textBox.Text);
            }
        }

        private void UpdateConfirmNewPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isPasswordSyncing)
                return;

            if (sender is TextBox textBox)
            {
                SyncPasswordBox(UpdateConfirmNewPasswordBox, textBox.Text);
            }
        }

        private void SyncPasswordBox(PasswordBox passwordBox, string value)
        {
            isPasswordSyncing = true;

            try
            {
                if (passwordBox.Password != value)
                {
                    passwordBox.Password = value ?? string.Empty;
                }
            }
            finally
            {
                isPasswordSyncing = false;
            }
        }

        private void ClearUpdatePasswordBoxes()
        {
            isPasswordSyncing = true;

            try
            {
                UpdateOldPasswordBox.Password = string.Empty;
                UpdateNewPasswordBox.Password = string.Empty;
                UpdateConfirmNewPasswordBox.Password = string.Empty;
            }
            finally
            {
                isPasswordSyncing = false;
            }
        }
    }
}