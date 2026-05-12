using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Repositories;
using CruzNeryClinic.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    // LoginViewModel controls the logic of the Login screen.
    // It receives the user ID/username and password, validates them,
    // then calls UserRepository to check the database.
    public class LoginViewModel : BaseViewModel
    {
        private readonly UserRepository userRepository;

        private string loginInput = string.Empty;
        private string password = string.Empty;
        private string errorMessage = string.Empty;
        private bool hasError;

        // This event tells the View that login succeeded.
        // The View will decide what screen/window to open next.
        public event Action? LoginSucceeded;

        // This event tells the View that the user clicked Forgot Password.
        public event Action? ForgotPasswordRequested;

        public LoginViewModel()
        {
            userRepository = new UserRepository();

            // Commands are connected to buttons in the XAML.
            LoginCommand = new RelayCommand(Login);
            ForgotPasswordCommand = new RelayCommand(OpenForgotPassword);
        }

        // This is bound to the User ID textbox.
        // It can accept either visible UserCode such as 2026-001,
        // or username such as admin01.
        public string LoginInput
        {
            get => loginInput;
            set
            {
                SetProperty(ref loginInput, value);
                ClearError();
            }
        }

        // This is bound manually from the PasswordBox in the View.
        // PasswordBox cannot directly bind its Password property safely in WPF.
        public string Password
        {
            get => password;
            set
            {
                SetProperty(ref password, value);
                ClearError();
            }
        }

        // Error message displayed under the login form.
        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        // Controls whether the error message is visible.
        public bool HasError
        {
            get => hasError;
            set => SetProperty(ref hasError, value);
        }

        // Command for the Login button.
        public ICommand LoginCommand { get; }

        // Command for the Forgot Password text/button.
        public ICommand ForgotPasswordCommand { get; }

        // Main login logic.
        private void Login()
        {
            try
            {
                // Validate empty fields first before checking the database.
                if (string.IsNullOrWhiteSpace(LoginInput))
                {
                    ShowError("Please enter your user ID.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    ShowError("Please enter your password.");
                    return;
                }

                // Ask the repository to verify the account.
                User? user = userRepository.Login(LoginInput, Password);

                if (user == null)
                {
                    ShowError("Invalid user ID or password.");
                    return;
                }

                // Save the logged-in user into the session.
                SessionService.Login(user);

                // Notify the View that login succeeded.
                LoginSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                // This catches unexpected errors, such as database connection issues.
                ShowError($"Login failed: {ex.Message}");
            }
        }

        // Opens the forgot password flow.
        private void OpenForgotPassword()
        {
            ForgotPasswordRequested?.Invoke();
        }

        // Shows an error message in the UI.
        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }

        // Clears the error message when the user types again.
        private void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }
    }
}