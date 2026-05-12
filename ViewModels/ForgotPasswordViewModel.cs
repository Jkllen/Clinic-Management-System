using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Repositories;
using System;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    // ForgotPasswordViewModel controls the Forgot Password screen.
    // Its job is to check if the entered User ID belongs to an active account.
    public class ForgotPasswordViewModel : BaseViewModel
    {
        private readonly UserRepository userRepository;

        private string userCode = string.Empty;
        private string errorMessage = string.Empty;
        private bool hasError;

        // This event is triggered when the entered user ID is valid.
        // MainWindow will use this to open the Security Questions screen.
        public event Action<User>? UserFound;

        // This event is triggered when the user wants to return to Login.
        public event Action? BackToLoginRequested;

        public ForgotPasswordViewModel()
        {
            userRepository = new UserRepository();

            ContinueCommand = new RelayCommand(Continue);
            BackToLoginCommand = new RelayCommand(BackToLogin);
        }

        // Bound to the User ID textbox.
        public string UserCode
        {
            get => userCode;
            set
            {
                SetProperty(ref userCode, value);
                ClearError();
            }
        }

        // Error text shown under the input field.
        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        // Controls the visibility of the error message.
        public bool HasError
        {
            get => hasError;
            set => SetProperty(ref hasError, value);
        }

        // Command for the Enter button.
        public ICommand ContinueCommand { get; }

        // Command for the Back button.
        public ICommand BackToLoginCommand { get; }

        private void Continue()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UserCode))
                {
                    ShowError("Please enter your user ID.");
                    return;
                }

                // We use GetActiveUserByUserCode because this screen specifically asks for User ID.
                User? user = userRepository.GetActiveUserByUserCode(UserCode);

                if (user == null)
                {
                    ShowError("User ID was not found.");
                    return;
                }

                // If the user exists, move to the Security Questions screen.
                UserFound?.Invoke(user);
            }
            catch (Exception ex)
            {
                ShowError($"Unable to continue: {ex.Message}");
            }
        }

        private void BackToLogin()
        {
            BackToLoginRequested?.Invoke();
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