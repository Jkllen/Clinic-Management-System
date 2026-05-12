using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Repositories;
using System;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    // This ViewModel controls the Create New Password screen.
    // It validates the password requirements, checks if both passwords match,
    // and then updates the user's password in the database.
    public class CreateNewPasswordViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly User _user;

        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError;

        // This event is triggered after the password is successfully reset.
        // MainWindow will listen to this and return the user to the Login screen.
        public event Action? PasswordResetSucceeded;

        // This event is triggered when the user clicks Back.
        // It returns the user to the Security Questions screen.
        public event Action<User>? BackToSecurityQuestionsRequested;

        public CreateNewPasswordViewModel(User user)
        {
            _user = user;
            _userRepository = new UserRepository();

            ResetPasswordCommand = new RelayCommand(ResetPassword);
            BackCommand = new RelayCommand(GoBack);
        }

        // This receives the first PasswordBox value from the View code-behind.
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                SetProperty(ref _newPassword, value);
                ClearError();
            }
        }

        // This receives the Confirm PasswordBox value from the View code-behind.
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                ClearError();
            }
        }

        // Error text shown under the password fields.
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Controls error message visibility.
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public ICommand ResetPasswordCommand { get; }

        public ICommand BackCommand { get; }

        private void ResetPassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ShowError("Please fill in both password fields.");
                return;
            }

            if (!IsPasswordValid(NewPassword))
            {
                ShowError("Password must be at least 12 characters and include uppercase, lowercase, number, and symbol.");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ShowError("Password must be identical.");
                return;
            }

            try
            {
                // Updates the user's stored PasswordHash and PasswordSalt.
                // The real password is never stored in the database.
                _userRepository.ResetPassword(_user.UserId, NewPassword);

                // Adds a log so the reset action is recorded.
                _userRepository.AddActivityLog(
                    _user.UserId,
                    _user.UserCode,
                    _user.Username,
                    "Reset Password",
                    "Security",
                    $"{_user.FullName} reset their password."
                );

                PasswordResetSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError($"Password reset failed: {ex.Message}");
            }
        }

        // Checks if the password follows the required password policy.
        // Rule: at least 12 characters, one uppercase, one lowercase, one number, and one symbol.
        private bool IsPasswordValid(string password)
        {
            if (password.Length < 12)
                return false;

            bool hasUppercase = Regex.IsMatch(password, "[A-Z]");
            bool hasLowercase = Regex.IsMatch(password, "[a-z]");
            bool hasNumber = Regex.IsMatch(password, "[0-9]");
            bool hasSymbol = Regex.IsMatch(password, "[^a-zA-Z0-9]");

            return hasUppercase && hasLowercase && hasNumber && hasSymbol;
        }
        
        private void GoBack()
        {
            BackToSecurityQuestionsRequested?.Invoke(_user);
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }
    }
}