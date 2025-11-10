using ClinicManagementSystem.Models.ViewModels;
using System.Windows;

namespace ClinicManagementSystem.Views.Login
{
    public partial class LoginUI : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginUI()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Username = UserIdBox.Text;
            _viewModel.Password = PasswordBox.Password;

            bool success = _viewModel.AuthenticateUser();
            if (success)
            {
                // TODO: Navigate to Dashboard
                // new DashboardUI().Show();
                this.Close();
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Forgot password feature not yet implemented.");
        }
    }
}
