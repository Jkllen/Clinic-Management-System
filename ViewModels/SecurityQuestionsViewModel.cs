using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Repositories;
using System;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    // SecurityQuestionsViewModel controls the Security Questions screen.
    // It displays the user's saved questions and verifies the entered answers.
    public class SecurityQuestionsViewModel : BaseViewModel
    {
        private readonly UserRepository _userRepository;
        private readonly User _user;

        private string _answer1 = string.Empty;
        private string _answer2 = string.Empty;
        private string _answer3 = string.Empty;

        private string _errorMessage = string.Empty;
        private bool _hasError;

        // Triggered when all answers are correct.
        // The user will then proceed to Create New Password screen.
        public event Action<User>? SecurityPassed;

        // Triggered when the user clicks Back.
        public event Action? BackToForgotPasswordRequested;

        public SecurityQuestionsViewModel(User user)
        {
            _user = user;
            _userRepository = new UserRepository();

            ContinueCommand = new RelayCommand(Continue);
            BackCommand = new RelayCommand(GoBack);
        }

        // Visible user ID shown on the screen.
        public string UserCode => _user.UserCode;

        // Full name can be shown for confirmation if needed.
        public string FullName => _user.FullName;

        // These are loaded from the database.
        public string SecurityQuestion1 => _user.SecurityQuestion1;
        public string SecurityQuestion2 => _user.SecurityQuestion2;
        public string SecurityQuestion3 => _user.SecurityQuestion3;

        // Bound to Answer 1 textbox.
        public string Answer1
        {
            get => _answer1;
            set
            {
                SetProperty(ref _answer1, value);
                ClearError();
            }
        }

        // Bound to Answer 2 textbox.
        public string Answer2
        {
            get => _answer2;
            set
            {
                SetProperty(ref _answer2, value);
                ClearError();
            }
        }

        // Bound to Answer 3 textbox.
        public string Answer3
        {
            get => _answer3;
            set
            {
                SetProperty(ref _answer3, value);
                ClearError();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public ICommand ContinueCommand { get; }

        public ICommand BackCommand { get; }

        private void Continue()
        {
            if (string.IsNullOrWhiteSpace(Answer1) ||
                string.IsNullOrWhiteSpace(Answer2) ||
                string.IsNullOrWhiteSpace(Answer3))
            {
                ShowError("Please answer all security questions.");
                return;
            }

            bool isCorrect = _userRepository.VerifySecurityAnswers(
                _user,
                Answer1,
                Answer2,
                Answer3
            );

            if (!isCorrect)
            {
                ShowError("One or more answers are incorrect.");
                return;
            }

            // Security questions passed.
            SecurityPassed?.Invoke(_user);
        }

        private void GoBack()
        {
            BackToForgotPasswordRequested?.Invoke();
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