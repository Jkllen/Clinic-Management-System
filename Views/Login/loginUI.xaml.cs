using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Clinic_Management_System.Models.ViewModels;
using Clinic_Management_System.Services;

namespace Clinic_Management_System.Views.Login
{
    public partial class LoginUI : Window
    {
        private readonly LoginViewModel _vm;
        private bool _showPassword = false;

        public LoginUI()
        {
            InitializeComponent();

            // Ensure database exists + seed default admin
            DatabaseService.InitializeDatabase();

            _vm = new LoginViewModel();
            DataContext = _vm;
        }

        // Make window draggable
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Forgot Password clicked");
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UserIdBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Error: Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = DatabaseService.AuthenticateUser(username, password);
            if (success)
            {
                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Open main dashboard window here
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserIdBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UserPlaceholder.Visibility = string.IsNullOrEmpty(UserIdBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _showPassword = !_showPassword;

            if (_showPassword)
            {
                VisiblePasswordBox.Text = PasswordBox.Password;
                VisiblePasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                ShowPassBtn.Content = "🚫";
            }
            else
            {
                PasswordBox.Password = VisiblePasswordBox.Text;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                ShowPassBtn.Content = "👁";
            }

            UpdatePasswordPlaceholder();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_showPassword)
                UpdatePasswordPlaceholder();
        }

        private void VisiblePasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_showPassword)
                UpdatePasswordPlaceholder();
        }

        private void UpdatePasswordPlaceholder()
        {
            bool isEmpty = _showPassword
                ? string.IsNullOrEmpty(VisiblePasswordBox.Text)
                : string.IsNullOrEmpty(PasswordBox.Password);

            PassPlaceholder.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }


    }
}
