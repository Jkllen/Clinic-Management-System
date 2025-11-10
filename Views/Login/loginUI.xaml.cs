using System.Windows;
using System.Windows.Input;
using Clinic_Management_System.Models.ViewModels;
using Clinic_Management_System.Services;

namespace Clinic_Management_System.Views.Login
{
    public partial class LoginUI : Window
    {
        private readonly LoginViewModel _vm;

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
    }
}
