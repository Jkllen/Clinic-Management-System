using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Models.UserManagement;
using CruzNeryClinic.Repositories;
using CruzNeryClinic.Services;
using System;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    // UserManagementViewModel controls the User Management screen.
    // It loads user summary cards, user table rows, search, archive,
    // and the Add New User overlay form.
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly UserRepository userRepository;

        private int totalUsers;
        private int activeAccounts;
        private int administratorCount;
        private int staffCount;

        private string searchText = string.Empty;
        private UserListItem? selectedUser;
        private string errorMessage = string.Empty;
        private bool hasError;

        // Add User overlay state.
        private bool isAddUserOverlayOpen;
        private bool isSuccessPromptVisible;
        private int currentStep = 1;

        // Add User basic fields.
        private string newUserFirstName = string.Empty;
        private string newUserMiddleName = string.Empty;
        private string newUserLastName = string.Empty;
        private string newUserRole = string.Empty;
        private string newUserContactNumber = string.Empty;
        private string newUserPassword = string.Empty;
        private string newUserConfirmPassword = string.Empty;
        private bool isAddUserPasswordVisible;
        private bool isAddUserConfirmPasswordVisible;
        private bool isConfirmationPasswordVisible;

        // Add User security question fields.
        private SecurityQuestion? selectedSecurityQuestion1;
        private SecurityQuestion? selectedSecurityQuestion2;
        private SecurityQuestion? selectedSecurityQuestion3;

        private string securityAnswer1 = string.Empty;
        private string securityAnswer2 = string.Empty;
        private string securityAnswer3 = string.Empty;

        private string addUserErrorMessage = string.Empty;
        private bool hasAddUserError;

        private string createdUserDisplayName = string.Empty;

        public UserManagementViewModel()
        {
            userRepository = new UserRepository();

            Users = new ObservableCollection<UserListItem>();
            SecurityQuestions = new ObservableCollection<SecurityQuestion>();

            Roles = new ObservableCollection<string>
            {
                "Admin",
                "Dentist",
                "Secretary",
                "Dental Assistant"
            };

            LoadUsersCommand = new RelayCommand(LoadUsers);
            AddNewUserCommand = new RelayCommand(OpenAddUserOverlay);
            UpdateUserCommand = new RelayCommand<UserListItem>(OpenUpdateUser);
            ArchiveUserCommand = new RelayCommand<UserListItem>(ArchiveUser);
            ClearSearchCommand = new RelayCommand(ClearSearch);

            CloseAddUserOverlayCommand = new RelayCommand(CloseAddUserOverlay);
            NextStepCommand = new RelayCommand(GoToNextStep);
            PreviousStepCommand = new RelayCommand(GoToPreviousStep);
            SaveNewUserCommand = new RelayCommand(SaveNewUser);
            RegisterAgainCommand = new RelayCommand(RegisterAgain);
            CloseSuccessPromptCommand = new RelayCommand(CloseAddUserOverlay);

            ToggleAddUserPasswordVisibilityCommand = new RelayCommand(() =>
            {
                IsAddUserPasswordVisible = !IsAddUserPasswordVisible;
            });

            ToggleAddUserConfirmPasswordVisibilityCommand = new RelayCommand(() =>
            {
                IsAddUserConfirmPasswordVisible = !IsAddUserConfirmPasswordVisible;
            });

            ToggleConfirmationPasswordVisibilityCommand = new RelayCommand(() =>
            {
                IsConfirmationPasswordVisible = !IsConfirmationPasswordVisible;
            });

            LoadUsers();
            LoadSecurityQuestions();
        }

        public ObservableCollection<UserListItem> Users { get; }

        public ObservableCollection<SecurityQuestion> SecurityQuestions { get; }

        public ObservableCollection<string> Roles { get; }

        public int TotalUsers
        {
            get => totalUsers;
            set => SetProperty(ref totalUsers, value);
        }

        public int ActiveAccounts
        {
            get => activeAccounts;
            set => SetProperty(ref activeAccounts, value);
        }

        public int AdministratorCount
        {
            get => administratorCount;
            set => SetProperty(ref administratorCount, value);
        }

        public int StaffCount
        {
            get => staffCount;
            set => SetProperty(ref staffCount, value);
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                SetProperty(ref searchText, value);
                SearchUsers();
            }
        }

        public UserListItem? SelectedUser
        {
            get => selectedUser;
            set => SetProperty(ref selectedUser, value);
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        public bool HasError
        {
            get => hasError;
            set => SetProperty(ref hasError, value);
        }

        public bool IsAddUserOverlayOpen
        {
            get => isAddUserOverlayOpen;
            set => SetProperty(ref isAddUserOverlayOpen, value);
        }

        public bool IsSuccessPromptVisible
        {
            get => isSuccessPromptVisible;
            set => SetProperty(ref isSuccessPromptVisible, value);
        }

        public string AddUserPasswordEyeIcon =>
            IsAddUserPasswordVisible ? "Eye" : "EyeSlash";

        public string AddUserConfirmPasswordEyeIcon =>
            IsAddUserConfirmPasswordVisible ? "Eye" : "EyeSlash";

        public string ConfirmationPasswordEyeIcon =>
            IsConfirmationPasswordVisible ? "Eye" : "EyeSlash";

        public bool IsAddUserPasswordVisible
        {
            get => isAddUserPasswordVisible;
            set
            {
                SetProperty(ref isAddUserPasswordVisible, value);
                OnPropertyChanged(nameof(IsAddUserPasswordHidden));
                OnPropertyChanged(nameof(AddUserPasswordEyeIcon));
            }
        }

        public ICommand ToggleAddUserPasswordVisibilityCommand { get; }

        public ICommand ToggleAddUserConfirmPasswordVisibilityCommand { get; }

        public ICommand ToggleConfirmationPasswordVisibilityCommand { get; }

        public bool IsAddUserPasswordHidden => !IsAddUserPasswordVisible;

        public bool IsAddUserConfirmPasswordVisible
        {
            get => isAddUserConfirmPasswordVisible;
            set
            {
                SetProperty(ref isAddUserConfirmPasswordVisible, value);
                OnPropertyChanged(nameof(IsAddUserConfirmPasswordHidden));
                OnPropertyChanged(nameof(AddUserConfirmPasswordEyeIcon));
            }
        }

        public bool IsAddUserConfirmPasswordHidden => !IsAddUserConfirmPasswordVisible;

        public bool IsConfirmationPasswordVisible
        {
            get => isConfirmationPasswordVisible;
            set
            {
                SetProperty(ref isConfirmationPasswordVisible, value);
                OnPropertyChanged(nameof(ConfirmationPasswordDisplay));
                OnPropertyChanged(nameof(ConfirmationPasswordEyeIcon));
            }
        }

        public string ConfirmationPasswordDisplay =>
            IsConfirmationPasswordVisible ? NewUserPassword : "••••••••••••";

        public int CurrentStep
        {
            get => currentStep;
            set
            {
                SetProperty(ref currentStep, value);

                // Refresh step content visibility.
                OnPropertyChanged(nameof(IsBasicsStepVisible));
                OnPropertyChanged(nameof(IsSecurityStepVisible));
                OnPropertyChanged(nameof(IsConfirmationStepVisible));

                // Refresh bottom button visibility.
                OnPropertyChanged(nameof(IsBackButtonVisible));
                OnPropertyChanged(nameof(IsNextButtonVisible));
                OnPropertyChanged(nameof(IsAddUserButtonVisible));

                // Refresh progress bar colors.
                OnPropertyChanged(nameof(Step1SegmentBrush));
                OnPropertyChanged(nameof(Step2SegmentBrush));
                OnPropertyChanged(nameof(Step3SegmentBrush));

                OnPropertyChanged(nameof(Step1TextBrush));
                OnPropertyChanged(nameof(Step2TextBrush));
                OnPropertyChanged(nameof(Step3TextBrush));
            }
        }

        public bool IsBasicsStepVisible => CurrentStep == 1;

        public bool IsSecurityStepVisible => CurrentStep == 2;

        public bool IsConfirmationStepVisible => CurrentStep == 3;

        // Button visibility helpers for the Add User overlay.
        // These keep the XAML simple and avoid needing an inverse converter.
        public bool IsBackButtonVisible => CurrentStep > 1;

        public bool IsNextButtonVisible => CurrentStep < 3;

        public bool IsAddUserButtonVisible => CurrentStep == 3;

        public string Step1SegmentBrush => CurrentStep >= 1 ? "#2E86DE" : "#F0F0F0";

        public string Step2SegmentBrush => CurrentStep >= 2 ? "#2E86DE" : "#F0F0F0";

        public string Step3SegmentBrush => CurrentStep >= 3 ? "#2E86DE" : "#F0F0F0";

        public string Step1TextBrush => CurrentStep == 1 ? "#0A7BE8" : "#9A9A9A";

        public string Step2TextBrush => CurrentStep == 2 ? "#0A7BE8" : "#9A9A9A";

        public string Step3TextBrush => CurrentStep == 3 ? "#0A7BE8" : "#9A9A9A";

        public string NewUserFirstName
        {
            get => newUserFirstName;
            set
            {
                SetProperty(ref newUserFirstName, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public string NewUserMiddleName
        {
            get => newUserMiddleName;
            set
            {
                SetProperty(ref newUserMiddleName, value);
                RefreshConfirmationBindings();
            }
        }

        public string NewUserLastName
        {
            get => newUserLastName;
            set
            {
                SetProperty(ref newUserLastName, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public string NewUserRole
        {
            get => newUserRole;
            set
            {
                SetProperty(ref newUserRole, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public string NewUserContactNumber
        {
            get => newUserContactNumber;
            set
            {
                SetProperty(ref newUserContactNumber, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public string NewUserPassword
        {
            get => newUserPassword;
            set
            {
                SetProperty(ref newUserPassword, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
                OnPropertyChanged(nameof(IsNewUserPasswordEmpty));
                OnPropertyChanged(nameof(ConfirmationPasswordDisplay));
            }
        }

        public string NewUserConfirmPassword
        {
            get => newUserConfirmPassword;
            set
            {
                SetProperty(ref newUserConfirmPassword, value);
                ClearAddUserError();
                OnPropertyChanged(nameof(IsNewUserConfirmPasswordEmpty));
            }
        }
        public bool IsNewUserPasswordEmpty => string.IsNullOrEmpty(NewUserPassword);

        public bool IsNewUserConfirmPasswordEmpty => string.IsNullOrEmpty(NewUserConfirmPassword);

        public SecurityQuestion? SelectedSecurityQuestion1
        {
            get => selectedSecurityQuestion1;
            set
            {
                SetProperty(ref selectedSecurityQuestion1, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public SecurityQuestion? SelectedSecurityQuestion2
        {
            get => selectedSecurityQuestion2;
            set
            {
                SetProperty(ref selectedSecurityQuestion2, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public SecurityQuestion? SelectedSecurityQuestion3
        {
            get => selectedSecurityQuestion3;
            set
            {
                SetProperty(ref selectedSecurityQuestion3, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public string SecurityAnswer1
        {
            get => securityAnswer1;
            set
            {
                SetProperty(ref securityAnswer1, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public string SecurityAnswer2
        {
            get => securityAnswer2;
            set
            {
                SetProperty(ref securityAnswer2, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public string SecurityAnswer3
        {
            get => securityAnswer3;
            set
            {
                SetProperty(ref securityAnswer3, value);
                ClearAddUserError();
                RefreshConfirmationBindings();
            }
        }

        public string AddUserErrorMessage
        {
            get => addUserErrorMessage;
            set => SetProperty(ref addUserErrorMessage, value);
        }

        public bool HasAddUserError
        {
            get => hasAddUserError;
            set => SetProperty(ref hasAddUserError, value);
        }

        public string CreatedUserDisplayName
        {
            get => createdUserDisplayName;
            set => SetProperty(ref createdUserDisplayName, value);
        }

        public string GeneratedUsername
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NewUserFirstName) ||
                    string.IsNullOrWhiteSpace(NewUserLastName))
                {
                    return "Will be generated after saving";
                }

                return $"{NewUserFirstName.Trim().ToLower()}.{NewUserLastName.Trim().ToLower()}";
            }
        }

        public string ConfirmationFullName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NewUserMiddleName))
                    return $"{NewUserFirstName} {NewUserLastName}".Trim();

                return $"{NewUserFirstName} {NewUserMiddleName} {NewUserLastName}".Trim();
            }
        }

        public string SelectedQuestionText1 => SelectedSecurityQuestion1?.QuestionText ?? string.Empty;

        public string SelectedQuestionText2 => SelectedSecurityQuestion2?.QuestionText ?? string.Empty;

        public string SelectedQuestionText3 => SelectedSecurityQuestion3?.QuestionText ?? string.Empty;

        public ICommand LoadUsersCommand { get; }

        public ICommand AddNewUserCommand { get; }

        public ICommand UpdateUserCommand { get; }

        public ICommand ArchiveUserCommand { get; }

        public ICommand ClearSearchCommand { get; }

        public ICommand CloseAddUserOverlayCommand { get; }

        public ICommand NextStepCommand { get; }

        public ICommand PreviousStepCommand { get; }

        public ICommand SaveNewUserCommand { get; }

        public ICommand RegisterAgainCommand { get; }

        public ICommand CloseSuccessPromptCommand { get; }

        private void LoadUsers()
        {
            try
            {
                ClearError();

                TotalUsers = userRepository.GetAllUsers(includeArchived: true).Count;
                ActiveAccounts = userRepository.CountActiveUsers();
                AdministratorCount = userRepository.CountAdmins();
                StaffCount = userRepository.CountStaff();

                Users.Clear();

                foreach (User user in userRepository.GetAllUsers(includeArchived: false))
                {
                    Users.Add(ConvertToUserListItem(user));
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load users: {ex.Message}");
            }
        }

        private void SearchUsers()
        {
            try
            {
                ClearError();

                Users.Clear();

                var results = string.IsNullOrWhiteSpace(SearchText)
                    ? userRepository.GetAllUsers(includeArchived: false)
                    : userRepository.SearchUsers(SearchText);

                foreach (User user in results)
                {
                    Users.Add(ConvertToUserListItem(user));
                }
            }
            catch (Exception ex)
            {
                ShowError($"Search failed: {ex.Message}");
            }
        }

        private void LoadSecurityQuestions()
        {
            try
            {
                SecurityQuestions.Clear();

                foreach (SecurityQuestion question in userRepository.GetActiveSecurityQuestions())
                {
                    SecurityQuestions.Add(question);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load security questions: {ex.Message}");
            }
        }

        private void OpenAddUserOverlay()
        {
            ResetAddUserForm();

            IsAddUserOverlayOpen = true;
            IsSuccessPromptVisible = false;
            CurrentStep = 1;
        }

        private void CloseAddUserOverlay()
        {
            IsAddUserOverlayOpen = false;
            IsSuccessPromptVisible = false;
            ResetAddUserForm();
        }

        private void GoToNextStep()
        {
            if (CurrentStep == 1)
            {
                if (!ValidateBasicStep())
                    return;

                CurrentStep = 2;
                return;
            }

            if (CurrentStep == 2)
            {
                if (!ValidateSecurityStep())
                    return;

                RefreshConfirmationBindings();
                CurrentStep = 3;
            }
        }

        private void GoToPreviousStep()
        {
            ClearAddUserError();

            if (CurrentStep > 1)
                CurrentStep--;
        }

        private void SaveNewUser()
        {
            if (!ValidateBasicStep())
                return;

            if (!ValidateSecurityStep())
                return;

            if (SessionService.CurrentUser == null)
            {
                ShowAddUserError("No logged-in user was found.");
                return;
            }

            try
            {
                string userCode = userRepository.GenerateNextUserCode();
                string username = GenerateUniqueUsername();

                userRepository.AddUser(
                    userCode,
                    NewUserFirstName,
                    NewUserMiddleName,
                    NewUserLastName,
                    NewUserContactNumber,
                    username,
                    NewUserPassword,
                    NewUserRole,
                    SelectedSecurityQuestion1!.SecurityQuestionId,
                    SecurityAnswer1,
                    SelectedSecurityQuestion2!.SecurityQuestionId,
                    SecurityAnswer2,
                    SelectedSecurityQuestion3!.SecurityQuestionId,
                    SecurityAnswer3,
                    SessionService.CurrentUser.UserId,
                    SessionService.CurrentUser.UserCode,
                    SessionService.CurrentUser.Username
                );

                CreatedUserDisplayName = $"{NewUserFirstName} {NewUserLastName}";
                IsSuccessPromptVisible = true;

                LoadUsers();
            }
            catch (Exception ex)
            {
                ShowAddUserError($"Failed to add user: {ex.Message}");
            }
        }

        private void RegisterAgain()
        {
            ResetAddUserForm();
            IsSuccessPromptVisible = false;
            CurrentStep = 1;
        }

        private void OpenUpdateUser(UserListItem? user)
        {
            if (user == null)
            {
                ShowError("Please select a user to update.");
                return;
            }

            ShowError($"Update form for {user.UserCode} will be added next.");
        }

        private void ArchiveUser(UserListItem? user)
        {
            if (user == null)
            {
                ShowError("Please select a user to archive.");
                return;
            }

            if (SessionService.CurrentUser == null)
            {
                ShowError("No logged-in user was found.");
                return;
            }

            if (user.UserId == SessionService.CurrentUser.UserId)
            {
                ShowError("You cannot archive your own account while logged in.");
                return;
            }

            try
            {
                userRepository.ArchiveUser(
                    user.UserId,
                    SessionService.CurrentUser.UserId,
                    SessionService.CurrentUser.UserCode,
                    SessionService.CurrentUser.Username
                );

                LoadUsers();
            }
            catch (Exception ex)
            {
                ShowError($"Archive failed: {ex.Message}");
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            LoadUsers();
        }

        private bool ValidateBasicStep()
        {
            if (string.IsNullOrWhiteSpace(NewUserFirstName))
            {
                ShowAddUserError("First name is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewUserLastName))
            {
                ShowAddUserError("Last name is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewUserRole))
            {
                ShowAddUserError("Role is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewUserContactNumber))
            {
                ShowAddUserError("Contact number is required.");
                return false;
            }

            if (!IsValidContactNumber(NewUserContactNumber))
            {
                ShowAddUserError("Contact number must be a valid mobile number.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewUserPassword))
            {
                ShowAddUserError("Password is required.");
                return false;
            }

            if (NewUserPassword.Length < 12)
            {
                ShowAddUserError("Password must contain at least 12 characters.");
                return false;
            }

            if (!IsStrongPassword(NewUserPassword))
            {
                ShowAddUserError("Password must contain at least 12 characters, uppercase, lowercase, number, and special character.");
                return false;
            }

            ClearAddUserError();
            return true;
        }

        private bool ValidateSecurityStep()
        {
            if (SelectedSecurityQuestion1 == null ||
                SelectedSecurityQuestion2 == null ||
                SelectedSecurityQuestion3 == null)
            {
                ShowAddUserError("Please select all three security questions.");
                return false;
            }

            if (SelectedSecurityQuestion1.SecurityQuestionId == SelectedSecurityQuestion2.SecurityQuestionId ||
                SelectedSecurityQuestion1.SecurityQuestionId == SelectedSecurityQuestion3.SecurityQuestionId ||
                SelectedSecurityQuestion2.SecurityQuestionId == SelectedSecurityQuestion3.SecurityQuestionId)
            {
                ShowAddUserError("Please select three different security questions.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SecurityAnswer1) ||
                string.IsNullOrWhiteSpace(SecurityAnswer2) ||
                string.IsNullOrWhiteSpace(SecurityAnswer3))
            {
                ShowAddUserError("Please answer all security questions.");
                return false;
            }

            ClearAddUserError();
            return true;
        }

        private bool IsValidContactNumber(string contactNumber)
        {
            string cleanedNumber = contactNumber.Trim();

            // Allows:
            // 09123456789
            // +639123456789
            // 639123456789
            return Regex.IsMatch(cleanedNumber, @"^(09\d{9}|\+639\d{9}|639\d{9})$");
        }

        private bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            bool hasMinimumLength = password.Length >= 12;
            bool hasUppercase = Regex.IsMatch(password, @"[A-Z]");
            bool hasLowercase = Regex.IsMatch(password, @"[a-z]");
            bool hasNumber = Regex.IsMatch(password, @"[0-9]");
            bool hasSpecialCharacter = Regex.IsMatch(password, @"[^a-zA-Z0-9]");

            return hasMinimumLength &&
                hasUppercase &&
                hasLowercase &&
                hasNumber &&
                hasSpecialCharacter;
        }

        private string GenerateUniqueUsername()
        {
            string baseUsername = GeneratedUsername;

            if (baseUsername == "Will be generated after saving")
                baseUsername = $"user{DateTime.Now:yyyyMMddHHmmss}";

            string username = baseUsername;
            int suffix = 1;

            while (userRepository.UsernameExists(username))
            {
                username = $"{baseUsername}{suffix}";
                suffix++;
            }

            return username;
        }

        private void ResetAddUserForm()
        {
            NewUserFirstName = string.Empty;
            NewUserMiddleName = string.Empty;
            NewUserLastName = string.Empty;
            NewUserRole = string.Empty;
            NewUserContactNumber = string.Empty;
            NewUserPassword = string.Empty;
            NewUserConfirmPassword = string.Empty;

            SelectedSecurityQuestion1 = null;
            SelectedSecurityQuestion2 = null;
            SelectedSecurityQuestion3 = null;

            SecurityAnswer1 = string.Empty;
            SecurityAnswer2 = string.Empty;
            SecurityAnswer3 = string.Empty;

            CreatedUserDisplayName = string.Empty;

            ClearAddUserError();
            RefreshConfirmationBindings();
        }

        private void RefreshConfirmationBindings()
        {
            OnPropertyChanged(nameof(GeneratedUsername));
            OnPropertyChanged(nameof(ConfirmationFullName));
            OnPropertyChanged(nameof(SelectedQuestionText1));
            OnPropertyChanged(nameof(SelectedQuestionText2));
            OnPropertyChanged(nameof(SelectedQuestionText3));
        }

        private UserListItem ConvertToUserListItem(User user)
        {
            return new UserListItem
            {
                UserId = user.UserId,
                UserCode = user.UserCode,
                LastName = user.LastName,
                FirstName = user.FirstName,
                ContactNumber = user.ContactNumber,
                Role = user.Role,
                IsActive = user.IsActive
            };
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

        private void ShowAddUserError(string message)
        {
            AddUserErrorMessage = message;
            HasAddUserError = true;
        }

        private void ClearAddUserError()
        {
            AddUserErrorMessage = string.Empty;
            HasAddUserError = false;
        }
    }
}