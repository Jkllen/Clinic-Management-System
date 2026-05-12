using CruzNeryClinic.ViewModels;
using FontAwesome.Sharp;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    // LoginView is the code-behind for the login screen.
    // PasswordBox.Password cannot be directly bound in WPF,
    // so we manually pass the password to LoginViewModel.
    public partial class LoginView : UserControl
    {
        private bool isPasswordVisible = false;
        private bool isUpdatingPassword = false;

        public LoginView()
        {
            InitializeComponent();
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isUpdatingPassword)
                return;

            // Send hidden PasswordBox value to ViewModel.
            UpdateViewModelPassword(PasswordInput.Password);

            // Keep visible TextBox synced.
            isUpdatingPassword = true;
            VisiblePasswordInput.Text = PasswordInput.Password;
            isUpdatingPassword = false;

            UpdatePasswordPlaceholder();
        }

        private void VisiblePasswordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingPassword)
                return;

            // Send visible TextBox value to ViewModel.
            UpdateViewModelPassword(VisiblePasswordInput.Text);

            // Keep hidden PasswordBox synced.
            isUpdatingPassword = true;
            PasswordInput.Password = VisiblePasswordInput.Text;
            isUpdatingPassword = false;

            UpdatePasswordPlaceholder();
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                // Show password as plain text.
                VisiblePasswordInput.Visibility = Visibility.Visible;
                PasswordInput.Visibility = Visibility.Collapsed;
                PasswordEyeIcon.Icon = IconChar.Eye;
                VisiblePasswordInput.Focus();
                VisiblePasswordInput.CaretIndex = VisiblePasswordInput.Text.Length;
            }
            else
            {
                // Hide password again.
                VisiblePasswordInput.Visibility = Visibility.Collapsed;
                PasswordInput.Visibility = Visibility.Visible;
                PasswordEyeIcon.Icon = IconChar.EyeSlash;
                PasswordInput.Focus();
            }

            UpdatePasswordPlaceholder();
        }

        private void UpdateViewModelPassword(string password)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = password;
            }
        }

        private void UpdatePasswordPlaceholder()
        {
            string currentPassword = isPasswordVisible
                ? VisiblePasswordInput.Text
                : PasswordInput.Password;

            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(currentPassword)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}