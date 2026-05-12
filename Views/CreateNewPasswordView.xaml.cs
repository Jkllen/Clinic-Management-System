using CruzNeryClinic.ViewModels;
using FontAwesome.Sharp;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    // Code-behind for CreateNewPasswordView.
    // PasswordBox.Password cannot be bound directly in WPF,
    // so this file syncs the PasswordBox values to the ViewModel manually.
    public partial class CreateNewPasswordView : UserControl
    {
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        private bool _isUpdatingNewPassword = false;
        private bool _isUpdatingConfirmPassword = false;

        public CreateNewPasswordView()
        {
            InitializeComponent();
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingNewPassword)
                return;

            UpdateViewModelNewPassword(NewPasswordBox.Password);

            // Keep visible textbox synchronized with hidden password box.
            _isUpdatingNewPassword = true;
            VisibleNewPasswordBox.Text = NewPasswordBox.Password;
            _isUpdatingNewPassword = false;
        }

        private void VisibleNewPasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingNewPassword)
                return;

            UpdateViewModelNewPassword(VisibleNewPasswordBox.Text);

            // Keep hidden password box synchronized with visible textbox.
            _isUpdatingNewPassword = true;
            NewPasswordBox.Password = VisibleNewPasswordBox.Text;
            _isUpdatingNewPassword = false;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingConfirmPassword)
                return;

            UpdateViewModelConfirmPassword(ConfirmPasswordBox.Password);

            // Keep visible textbox synchronized with hidden password box.
            _isUpdatingConfirmPassword = true;
            VisibleConfirmPasswordBox.Text = ConfirmPasswordBox.Password;
            _isUpdatingConfirmPassword = false;
        }

        private void VisibleConfirmPasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingConfirmPassword)
                return;

            UpdateViewModelConfirmPassword(VisibleConfirmPasswordBox.Text);

            // Keep hidden password box synchronized with visible textbox.
            _isUpdatingConfirmPassword = true;
            ConfirmPasswordBox.Password = VisibleConfirmPasswordBox.Text;
            _isUpdatingConfirmPassword = false;
        }

        private void ToggleNewPassword_Click(object sender, RoutedEventArgs e)
        {
            _isNewPasswordVisible = !_isNewPasswordVisible;

            if (_isNewPasswordVisible)
            {
                VisibleNewPasswordBox.Visibility = Visibility.Visible;
                NewPasswordBox.Visibility = Visibility.Collapsed;

                NewPasswordEyeIcon.Icon = IconChar.Eye;

                VisibleNewPasswordBox.Focus();
                VisibleNewPasswordBox.CaretIndex = VisibleNewPasswordBox.Text.Length;
            }
            else
            {
                VisibleNewPasswordBox.Visibility = Visibility.Collapsed;
                NewPasswordBox.Visibility = Visibility.Visible;

                NewPasswordEyeIcon.Icon = IconChar.EyeSlash;

                NewPasswordBox.Focus();
            }
        }

        private void ToggleConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;

            if (_isConfirmPasswordVisible)
            {
                VisibleConfirmPasswordBox.Visibility = Visibility.Visible;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;

                ConfirmPasswordEyeIcon.Icon = IconChar.Eye;

                VisibleConfirmPasswordBox.Focus();
                VisibleConfirmPasswordBox.CaretIndex = VisibleConfirmPasswordBox.Text.Length;
            }
            else
            {
                VisibleConfirmPasswordBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = Visibility.Visible;

                ConfirmPasswordEyeIcon.Icon = IconChar.EyeSlash;

                ConfirmPasswordBox.Focus();
            }
        }

        private void UpdateViewModelNewPassword(string password)
        {
            if (DataContext is CreateNewPasswordViewModel viewModel)
            {
                viewModel.NewPassword = password;
            }
        }

        private void UpdateViewModelConfirmPassword(string password)
        {
            if (DataContext is CreateNewPasswordViewModel viewModel)
            {
                viewModel.ConfirmPassword = password;
            }
        }

        private void NewPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NewPasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void NewPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateNewPasswordPlaceholder();
        }

        private void VisibleNewPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NewPasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void VisibleNewPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateNewPasswordPlaceholder();
        }

        private void ConfirmPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void ConfirmPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateConfirmPasswordPlaceholder();
        }

        private void VisibleConfirmPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void VisibleConfirmPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateConfirmPasswordPlaceholder();
        }

        private void UpdateNewPasswordPlaceholder()
        {
            string currentPassword = _isNewPasswordVisible
                ? VisibleNewPasswordBox.Text
                : NewPasswordBox.Password;

            bool isFocused = _isNewPasswordVisible
                ? VisibleNewPasswordBox.IsKeyboardFocused
                : NewPasswordBox.IsKeyboardFocused;

            NewPasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(currentPassword) && !isFocused
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void UpdateConfirmPasswordPlaceholder()
        {
            string currentPassword = _isConfirmPasswordVisible
                ? VisibleConfirmPasswordBox.Text
                : ConfirmPasswordBox.Password;

            bool isFocused = _isConfirmPasswordVisible
                ? VisibleConfirmPasswordBox.IsKeyboardFocused
                : ConfirmPasswordBox.IsKeyboardFocused;

            ConfirmPasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(currentPassword) && !isFocused
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

    }
}