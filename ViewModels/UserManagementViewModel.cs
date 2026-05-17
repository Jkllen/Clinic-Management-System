using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Models.UserManagement;
using CruzNeryClinic.Repositories;
using CruzNeryClinic.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;
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
        #region Dependencies and Backing Fields

        // Repository used by this ViewModel.
        private readonly UserRepository userRepository;


        // Summary card values.
        private int totalUsers;
        private int activeAccounts;
        private int administratorCount;
        private int staffCount;


        // Main table state.
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


        // Filter and sorting state.
        private readonly List<UserListItem> allUserItems = new();

        private string selectedFilterOption = "All Active Users";
        private string selectedSortOption = "User ID Ascending";


        // Update User overlay state.
        private bool isUpdateUserOverlayOpen;
        private int updateUserCurrentTab = 1;
        private bool isUpdateSuccessPromptVisible;

        private UserListItem? userBeingUpdated;

        // Update account information fields.
        private string updateUserFirstName = string.Empty;
        private string updateUserMiddleName = string.Empty;
        private string updateUserLastName = string.Empty;
        private string updateUserRole = string.Empty;
        private string updateUserContactNumber = string.Empty;

        // Update password fields.
        private string updateOldPassword = string.Empty;
        private string updateNewPassword = string.Empty;
        private string updateConfirmNewPassword = string.Empty;

        private bool isUpdateOldPasswordVisible;
        private bool isUpdateNewPasswordVisible;
        private bool isUpdateConfirmNewPasswordVisible;

        // Update security question fields.
        private SecurityQuestion? updateSelectedSecurityQuestion1;
        private SecurityQuestion? updateSelectedSecurityQuestion2;
        private SecurityQuestion? updateSelectedSecurityQuestion3;

        private string updateSecurityAnswer1 = string.Empty;
        private string updateSecurityAnswer2 = string.Empty;
        private string updateSecurityAnswer3 = string.Empty;

        private string updateUserErrorMessage = string.Empty;
        private bool hasUpdateUserError;

        private string updateSuccessTitle = string.Empty;
        private string updateSuccessMessage = string.Empty;

        #endregion

        #region Constructor

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

            FilterOptions = new ObservableCollection<string>
            {
                "All Active Users",
                "All Users",
                "Archived Users",
                "Administrators",
                "Dentists",
                "Secretaries",
                "Dental Assistants",
                "Staff Only"
            };

            SortOptions = new ObservableCollection<string>
            {
                "User ID Ascending",
                "User ID Descending",
                "Last Name A-Z",
                "Last Name Z-A",
                "First Name A-Z",
                "First Name Z-A",
                "Role A-Z",
                "Role Z-A"
            };

            LoadUsersCommand = new RelayCommand(LoadUsers);
            AddNewUserCommand = new RelayCommand(OpenAddUserOverlay);
            UpdateUserCommand = new RelayCommand<UserListItem>(OpenUpdateUser);
            ArchiveUserCommand = new RelayCommand<UserListItem>(ArchiveUser);
            RestoreUserCommand = new RelayCommand<UserListItem>(RestoreUser);
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

            CloseUpdateUserOverlayCommand = new RelayCommand(CloseUpdateUserOverlay);

            ShowUpdateAccountInfoTabCommand = new RelayCommand(() => UpdateUserCurrentTab = 1);
            ShowUpdatePasswordTabCommand = new RelayCommand(() => UpdateUserCurrentTab = 2);
            ShowUpdateSecurityTabCommand = new RelayCommand(() => UpdateUserCurrentTab = 3);

            SaveUpdateAccountInfoCommand = new RelayCommand(SaveUpdateAccountInfo);
            SaveUpdatePasswordCommand = new RelayCommand(SaveUpdatePassword);
            SaveUpdateSecurityCommand = new RelayCommand(SaveUpdateSecurity);

            CloseUpdateSuccessPromptCommand = new RelayCommand(() =>
            {
                IsUpdateSuccessPromptVisible = false;
            });

            ToggleUpdateOldPasswordVisibilityCommand = new RelayCommand(() =>
            {
                IsUpdateOldPasswordVisible = !IsUpdateOldPasswordVisible;
            });

            ToggleUpdateNewPasswordVisibilityCommand = new RelayCommand(() =>
            {
                IsUpdateNewPasswordVisible = !IsUpdateNewPasswordVisible;
            });

            ToggleUpdateConfirmNewPasswordVisibilityCommand = new RelayCommand(() =>
            {
                IsUpdateConfirmNewPasswordVisible = !IsUpdateConfirmNewPasswordVisible;
            });

            LoadUsers();
            LoadSecurityQuestions();
        }

        #endregion

        #region Public Collections

        public ObservableCollection<UserListItem> Users { get; }

        public ObservableCollection<SecurityQuestion> SecurityQuestions { get; }

        public ObservableCollection<string> Roles { get; }

        #endregion

        #region Summary Card Properties

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

        #endregion

        #region Search, Filter, and Sort Properties

        public string SearchText
        {
            get => searchText;
            set
            {
                SetProperty(ref searchText, value);
                RefreshUsersView();
            }
        }

        public ObservableCollection<string> FilterOptions { get; }

        public ObservableCollection<string> SortOptions { get; }

        public string SelectedFilterOption
        {
            get => selectedFilterOption;
            set
            {
                SetProperty(ref selectedFilterOption, value);
                RefreshUsersView();
            }
        }

        public string SelectedSortOption
        {
            get => selectedSortOption;
            set
            {
                SetProperty(ref selectedSortOption, value);
                RefreshUsersView();
            }
        }

        #endregion

        #region Selection and Screen Error Properties

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

        #endregion

        #region Add and Update Overlay Properties

        public bool IsAddUserOverlayOpen
        {
            get => isAddUserOverlayOpen;
            set => SetProperty(ref isAddUserOverlayOpen, value);
        }

        public bool IsUpdateUserOverlayOpen
        {
            get => isUpdateUserOverlayOpen;
            set => SetProperty(ref isUpdateUserOverlayOpen, value);
        }

        public int UpdateUserCurrentTab
        {
            get => updateUserCurrentTab;
            set
            {
                SetProperty(ref updateUserCurrentTab, value);

                OnPropertyChanged(nameof(IsUpdateAccountInfoTabVisible));
                OnPropertyChanged(nameof(IsUpdatePasswordTabVisible));
                OnPropertyChanged(nameof(IsUpdateSecurityTabVisible));

                OnPropertyChanged(nameof(UpdateAccountTabBrush));
                OnPropertyChanged(nameof(UpdatePasswordTabBrush));
                OnPropertyChanged(nameof(UpdateSecurityTabBrush));

                OnPropertyChanged(nameof(UpdateAccountTabForeground));
                OnPropertyChanged(nameof(UpdatePasswordTabForeground));
                OnPropertyChanged(nameof(UpdateSecurityTabForeground));
            }
        }

        public bool IsUpdateAccountInfoTabVisible => UpdateUserCurrentTab == 1;

        public bool IsUpdatePasswordTabVisible => UpdateUserCurrentTab == 2;

        public bool IsUpdateSecurityTabVisible => UpdateUserCurrentTab == 3;

        public string UpdateAccountTabBrush => UpdateUserCurrentTab == 1 ? "#2E86DE" : "Transparent";
        public string UpdatePasswordTabBrush => UpdateUserCurrentTab == 2 ? "#2E86DE" : "Transparent";
        public string UpdateSecurityTabBrush => UpdateUserCurrentTab == 3 ? "#2E86DE" : "Transparent";

        public string UpdateAccountTabForeground => UpdateUserCurrentTab == 1 ? "White" : "Black";
        public string UpdatePasswordTabForeground => UpdateUserCurrentTab == 2 ? "White" : "Black";
        public string UpdateSecurityTabForeground => UpdateUserCurrentTab == 3 ? "White" : "Black";

        public bool IsUpdateSuccessPromptVisible
        {
            get => isUpdateSuccessPromptVisible;
            set => SetProperty(ref isUpdateSuccessPromptVisible, value);
        }

        public string UpdateUserFirstName
        {
            get => updateUserFirstName;
            set
            {
                SetProperty(ref updateUserFirstName, value);
                ClearUpdateUserError();
            }
        }

        public string UpdateUserMiddleName
        {
            get => updateUserMiddleName;
            set => SetProperty(ref updateUserMiddleName, value);
        }

        public string UpdateUserLastName
        {
            get => updateUserLastName;
            set
            {
                SetProperty(ref updateUserLastName, value);
                ClearUpdateUserError();
            }
        }

        public string UpdateUserRole
        {
            get => updateUserRole;
            set
            {
                SetProperty(ref updateUserRole, value);
                ClearUpdateUserError();
            }
        }

        public string UpdateUserContactNumber
        {
            get => updateUserContactNumber;
            set
            {
                SetProperty(ref updateUserContactNumber, value);
                ClearUpdateUserError();
            }
        }

        public string UpdateOldPassword
        {
            get => updateOldPassword;
            set
            {
                SetProperty(ref updateOldPassword, value);
                ClearUpdateUserError();
                OnPropertyChanged(nameof(IsUpdateOldPasswordEmpty));
            }
        }

        public string UpdateNewPassword
        {
            get => updateNewPassword;
            set
            {
                SetProperty(ref updateNewPassword, value);
                ClearUpdateUserError();
                OnPropertyChanged(nameof(IsUpdateNewPasswordEmpty));

                // Refresh live password rule colors.
                OnPropertyChanged(nameof(UpdateMinLengthRuleBrush));
                OnPropertyChanged(nameof(UpdateUppercaseRuleBrush));
                OnPropertyChanged(nameof(UpdateLowercaseRuleBrush));
                OnPropertyChanged(nameof(UpdateNumberRuleBrush));
                OnPropertyChanged(nameof(UpdateSpecialCharRuleBrush));
            }
        }

        public string UpdateConfirmNewPassword
        {
            get => updateConfirmNewPassword;
            set
            {
                SetProperty(ref updateConfirmNewPassword, value);
                ClearUpdateUserError();
                OnPropertyChanged(nameof(IsUpdateConfirmNewPasswordEmpty));
            }
        }

        public bool IsUpdateOldPasswordEmpty => string.IsNullOrEmpty(UpdateOldPassword);
        public bool IsUpdateNewPasswordEmpty => string.IsNullOrEmpty(UpdateNewPassword);
        public bool IsUpdateConfirmNewPasswordEmpty => string.IsNullOrEmpty(UpdateConfirmNewPassword);

        public bool IsUpdateOldPasswordVisible
        {
            get => isUpdateOldPasswordVisible;
            set
            {
                SetProperty(ref isUpdateOldPasswordVisible, value);
                OnPropertyChanged(nameof(IsUpdateOldPasswordHidden));
                OnPropertyChanged(nameof(UpdateOldPasswordEyeIcon));
            }
        }

        public bool IsUpdateOldPasswordHidden => !IsUpdateOldPasswordVisible;

        public bool IsUpdateNewPasswordVisible
        {
            get => isUpdateNewPasswordVisible;
            set
            {
                SetProperty(ref isUpdateNewPasswordVisible, value);
                OnPropertyChanged(nameof(IsUpdateNewPasswordHidden));
                OnPropertyChanged(nameof(UpdateNewPasswordEyeIcon));
            }
        }

        public bool IsUpdateNewPasswordHidden => !IsUpdateNewPasswordVisible;

        public bool IsUpdateConfirmNewPasswordVisible
        {
            get => isUpdateConfirmNewPasswordVisible;
            set
            {
                SetProperty(ref isUpdateConfirmNewPasswordVisible, value);
                
                OnPropertyChanged(nameof(IsUpdateConfirmNewPasswordHidden));
                OnPropertyChanged(nameof(UpdateConfirmNewPasswordEyeIcon));
            
                // Refresh compatibility aliases.
                OnPropertyChanged(nameof(IsUpdateConfirmPasswordVisible));
                OnPropertyChanged(nameof(IsUpdateConfirmPasswordHidden));
                OnPropertyChanged(nameof(UpdateConfirmPasswordEyeIcon));
            }
        }

        public bool IsUpdateConfirmNewPasswordHidden => !IsUpdateConfirmNewPasswordVisible;

        public string UpdateOldPasswordEyeIcon => IsUpdateOldPasswordVisible ? "Eye" : "EyeSlash";
        public string UpdateNewPasswordEyeIcon => IsUpdateNewPasswordVisible ? "Eye" : "EyeSlash";
        public string UpdateConfirmNewPasswordEyeIcon => IsUpdateConfirmNewPasswordVisible ? "Eye" : "EyeSlash";

        // Compatibility aliases for the Update Password XAML.
        // These exist because some XAML bindings use the shorter "ConfirmPassword" name.
        public bool IsUpdateConfirmPasswordVisible => IsUpdateConfirmNewPasswordVisible;

        public bool IsUpdateConfirmPasswordHidden => IsUpdateConfirmNewPasswordHidden;

        public string UpdateConfirmPasswordEyeIcon => UpdateConfirmNewPasswordEyeIcon;

        // Live password requirement colors for the Change Password tab.
        // Red means the new password has not met that requirement yet.
        // Gray means the requirement is already satisfied.
        public Brush UpdateMinLengthRuleBrush =>
            !HasMinimumPasswordLength(UpdateNewPassword) ? Brushes.Red : Brushes.Gray;

        public Brush UpdateUppercaseRuleBrush =>
            !HasUppercaseCharacter(UpdateNewPassword) ? Brushes.Red : Brushes.Gray;

        public Brush UpdateLowercaseRuleBrush =>
            !HasLowercaseCharacter(UpdateNewPassword) ? Brushes.Red : Brushes.Gray;

        public Brush UpdateNumberRuleBrush =>
            !HasNumberCharacter(UpdateNewPassword) ? Brushes.Red : Brushes.Gray;

        public Brush UpdateSpecialCharRuleBrush =>
            !HasSpecialCharacter(UpdateNewPassword) ? Brushes.Red : Brushes.Gray;

        public SecurityQuestion? UpdateSelectedSecurityQuestion1
        {
            get => updateSelectedSecurityQuestion1;
            set
            {
                SetProperty(ref updateSelectedSecurityQuestion1, value);
                ClearUpdateUserError();
            }
        }

        public SecurityQuestion? UpdateSelectedSecurityQuestion2
        {
            get => updateSelectedSecurityQuestion2;
            set
            {
                SetProperty(ref updateSelectedSecurityQuestion2, value);
                ClearUpdateUserError();
            }
        }

        public SecurityQuestion? UpdateSelectedSecurityQuestion3
        {
            get => updateSelectedSecurityQuestion3;
            set
            {
                SetProperty(ref updateSelectedSecurityQuestion3, value);
                ClearUpdateUserError();
            }
        }

        public string UpdateSecurityAnswer1
        {
            get => updateSecurityAnswer1;
            set
            {
                SetProperty(ref updateSecurityAnswer1, value);
                ClearUpdateUserError();
            }
        }

        public string UpdateSecurityAnswer2
        {
            get => updateSecurityAnswer2;
            set
            {
                SetProperty(ref updateSecurityAnswer2, value);
                ClearUpdateUserError();
            }
        }

        public string UpdateSecurityAnswer3
        {
            get => updateSecurityAnswer3;
            set
            {
                SetProperty(ref updateSecurityAnswer3, value);
                ClearUpdateUserError();
            }
        }

        public string UpdateUserErrorMessage
        {
            get => updateUserErrorMessage;
            set => SetProperty(ref updateUserErrorMessage, value);
        }

        public bool HasUpdateUserError
        {
            get => hasUpdateUserError;
            set => SetProperty(ref hasUpdateUserError, value);
        }

        public string UpdateSuccessTitle
        {
            get => updateSuccessTitle;
            set => SetProperty(ref updateSuccessTitle, value);
        }

        public string UpdateSuccessMessage
        {
            get => updateSuccessMessage;
            set => SetProperty(ref updateSuccessMessage, value);
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

        #endregion

        #region Commands

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

        public ICommand RestoreUserCommand { get; }

        public ICommand CloseUpdateUserOverlayCommand { get; }

        public ICommand ShowUpdateAccountInfoTabCommand { get; }

        public ICommand ShowUpdatePasswordTabCommand { get; }

        public ICommand ShowUpdateSecurityTabCommand { get; }

        public ICommand SaveUpdateAccountInfoCommand { get; }

        public ICommand SaveUpdatePasswordCommand { get; }

        public ICommand SaveUpdateSecurityCommand { get; }

        public ICommand CloseUpdateSuccessPromptCommand { get; }

        public ICommand ToggleUpdateOldPasswordVisibilityCommand { get; }

        public ICommand ToggleUpdateNewPasswordVisibilityCommand { get; }
        public ICommand ToggleUpdateConfirmNewPasswordVisibilityCommand { get; }
        // Compatibility alias for XAML that uses the shorter command name.
        public ICommand ToggleUpdateConfirmPasswordVisibilityCommand =>
            ToggleUpdateConfirmNewPasswordVisibilityCommand;

        #endregion

        #region Main User List Methods

        private void LoadUsers()
        {
            try
            {
                ClearError();

                TotalUsers = userRepository.GetAllUsers(includeArchived: true).Count;
                ActiveAccounts = userRepository.CountActiveUsers();
                AdministratorCount = userRepository.CountAdmins();
                StaffCount = userRepository.CountStaff();

                allUserItems.Clear();

                foreach (User user in userRepository.GetAllUsers(includeArchived: true))
                {
                    allUserItems.Add(ConvertToUserListItem(user));
                }

                RefreshUsersView();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load users: {ex.Message}");
            }
        }

        private void RefreshUsersView()
        {
            IEnumerable<UserListItem> query = allUserItems;

            query = SelectedFilterOption switch
            {
                "All Active Users" => query.Where(user => user.IsActive),
                "All Users" => query,
                "Archived Users" => query.Where(user => !user.IsActive),
                "Administrators" => query.Where(user => user.IsActive && user.Role == "Admin"),
                "Dentists" => query.Where(user => user.IsActive && user.Role == "Dentist"),
                "Secretaries" => query.Where(user => user.IsActive && user.Role == "Secretary"),
                "Dental Assistants" => query.Where(user => user.IsActive && user.Role == "Dental Assistant"),
                "Staff Only" => query.Where(user => user.IsActive && user.Role != "Admin"),
                _ => query.Where(user => user.IsActive)
            };

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.Trim().ToLower();

                query = query.Where(user =>
                    user.UserCode.ToLower().Contains(keyword) ||
                    user.FirstName.ToLower().Contains(keyword) ||
                    user.LastName.ToLower().Contains(keyword) ||
                    user.ContactNumber.ToLower().Contains(keyword) ||
                    user.Role.ToLower().Contains(keyword) ||
                    user.AccountStatus.ToLower().Contains(keyword));
            }

            query = SelectedSortOption switch
            {
                "User ID Ascending" => query
                    .OrderBy(user => GetUserCodePrefixRank(user.UserCode))
                    .ThenBy(user => GetUserCodeNumber(user.UserCode)),

                "User ID Descending" => query
                    .OrderByDescending(user => GetUserCodePrefixRank(user.UserCode))
                    .ThenByDescending(user => GetUserCodeNumber(user.UserCode)),

                "Last Name A-Z" => query.OrderBy(user => user.LastName).ThenBy(user => user.FirstName),

                "Last Name Z-A" => query.OrderByDescending(user => user.LastName).ThenByDescending(user => user.FirstName),

                "First Name A-Z" => query.OrderBy(user => user.FirstName).ThenBy(user => user.LastName),

                "First Name Z-A" => query.OrderByDescending(user => user.FirstName).ThenByDescending(user => user.LastName),

                "Role A-Z" => query.OrderBy(user => user.Role).ThenBy(user => user.LastName),

                "Role Z-A" => query.OrderByDescending(user => user.Role).ThenBy(user => user.LastName),

                _ => query
                    .OrderBy(user => GetUserCodePrefixRank(user.UserCode))
                    .ThenBy(user => GetUserCodeNumber(user.UserCode))
            };

            Users.Clear();

            foreach (UserListItem user in query)
            {
                Users.Add(user);
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

        #endregion

        #region Add User Methods

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

        #endregion

        #region Update User Methods

        private void OpenUpdateUser(UserListItem? user)
        {
            if (user == null)
            {
                ShowError("Please select a user to update.");
                return;
            }

            userBeingUpdated = user;

            UpdateUserFirstName = user.FirstName;
            UpdateUserMiddleName = user.MiddleName;
            UpdateUserLastName = user.LastName;
            UpdateUserRole = user.Role;
            UpdateUserContactNumber = user.ContactNumber;

            ResetUpdatePasswordFields();
            ResetUpdateSecurityFields();

            UpdateUserCurrentTab = 1;
            IsUpdateSuccessPromptVisible = false;
            IsUpdateUserOverlayOpen = true;
            ClearUpdateUserError();
        }

        private void CloseUpdateUserOverlay()
        {
            IsUpdateUserOverlayOpen = false;
            IsUpdateSuccessPromptVisible = false;
            userBeingUpdated = null;
            ClearUpdateUserError();
        }

        private void SaveUpdateAccountInfo()
        {
            if (userBeingUpdated == null)
            {
                ShowUpdateUserError("No user was selected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(UpdateUserFirstName))
            {
                ShowUpdateUserError("First name is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(UpdateUserLastName))
            {
                ShowUpdateUserError("Last name is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(UpdateUserRole))
            {
                ShowUpdateUserError("Role is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(UpdateUserContactNumber))
            {
                ShowUpdateUserError("Contact number is required.");
                return;
            }

            if (!IsValidContactNumber(UpdateUserContactNumber))
            {
                ShowUpdateUserError("Contact number must be a valid Philippine mobile number, such as 09123456789 or +639123456789.");
                return;
            }

            if (SessionService.CurrentUser == null)
            {
                ShowUpdateUserError("No logged-in user was found.");
                return;
            }

            bool confirmed = ConfirmUpdateAction(
                "Confirm Update",
                $"Are you sure you want to update {UpdateUserFirstName} {UpdateUserLastName}'s account information?"
            );

            if (!confirmed)
                return;

            try
            {
                userRepository.UpdateUserAccountInfo(
                    userBeingUpdated.UserId,
                    UpdateUserFirstName,
                    UpdateUserMiddleName,
                    UpdateUserLastName,
                    UpdateUserRole,
                    UpdateUserContactNumber,
                    SessionService.CurrentUser.UserId,
                    SessionService.CurrentUser.UserCode,
                    SessionService.CurrentUser.Username
                );

                UpdateSuccessTitle = "User info updated successfully";
                UpdateSuccessMessage = $"{UpdateUserFirstName} {UpdateUserLastName}'s information has been updated.";
                IsUpdateSuccessPromptVisible = true;

                LoadUsers();
            }
            catch (Exception ex)
            {
                ShowUpdateUserError($"Failed to update user information: {ex.Message}");
            }
        }

        private void SaveUpdatePassword()
        {
            if (userBeingUpdated == null)
            {
                ShowUpdateUserError("No user was selected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(UpdateOldPassword))
            {
                ShowUpdateUserError("Old password is required.");
                return;
            }

            if (!IsStrongPassword(UpdateNewPassword))
            {
                ShowUpdateUserError("Password must contain at least 12 characters, uppercase, lowercase, number, and special character.");
                return;
            }

            if (UpdateNewPassword != UpdateConfirmNewPassword)
            {
                ShowUpdateUserError("New password and confirm password must match.");
                return;
            }

            if (SessionService.CurrentUser == null)
            {
                ShowUpdateUserError("No logged-in user was found.");
                return;
            }

            bool confirmed = ConfirmUpdateAction(
                "Confirm Password Change",
                $"Are you sure you want to change {userBeingUpdated.FirstName} {userBeingUpdated.LastName}'s password?"
            );

            if (!confirmed)
                return;

            try
            {
                userRepository.ChangeUserPasswordFromManagement(
                    userBeingUpdated.UserId,
                    UpdateOldPassword,
                    UpdateNewPassword,
                    SessionService.CurrentUser.UserId,
                    SessionService.CurrentUser.UserCode,
                    SessionService.CurrentUser.Username
                );

                UpdateSuccessTitle = "User info updated successfully";
                UpdateSuccessMessage = $"{userBeingUpdated.FirstName} {userBeingUpdated.LastName}'s password has been updated.";
                IsUpdateSuccessPromptVisible = true;

                ResetUpdatePasswordFields();
            }
            catch (Exception ex)
            {
                ShowUpdateUserError($"Failed to change password: {ex.Message}");
            }
        }

        private void SaveUpdateSecurity()
        {
            if (userBeingUpdated == null)
            {
                ShowUpdateUserError("No user was selected.");
                return;
            }

            if (UpdateSelectedSecurityQuestion1 == null ||
                UpdateSelectedSecurityQuestion2 == null ||
                UpdateSelectedSecurityQuestion3 == null)
            {
                ShowUpdateUserError("Please select all three security questions.");
                return;
            }

            if (UpdateSelectedSecurityQuestion1.SecurityQuestionId == UpdateSelectedSecurityQuestion2.SecurityQuestionId ||
                UpdateSelectedSecurityQuestion1.SecurityQuestionId == UpdateSelectedSecurityQuestion3.SecurityQuestionId ||
                UpdateSelectedSecurityQuestion2.SecurityQuestionId == UpdateSelectedSecurityQuestion3.SecurityQuestionId)
            {
                ShowUpdateUserError("Please select three different security questions.");
                return;
            }

            if (string.IsNullOrWhiteSpace(UpdateSecurityAnswer1) ||
                string.IsNullOrWhiteSpace(UpdateSecurityAnswer2) ||
                string.IsNullOrWhiteSpace(UpdateSecurityAnswer3))
            {
                ShowUpdateUserError("Please answer all security questions.");
                return;
            }

            if (SessionService.CurrentUser == null)
            {
                ShowUpdateUserError("No logged-in user was found.");
                return;
            }

            bool confirmed = ConfirmUpdateAction(
                "Confirm Security Update",
                $"Are you sure you want to update {userBeingUpdated.FirstName} {userBeingUpdated.LastName}'s security questions?"
            );

            if (!confirmed)
                return;

            try
            {
                userRepository.UpdateUserSecurityQuestions(
                    userBeingUpdated.UserId,
                    UpdateSelectedSecurityQuestion1.SecurityQuestionId,
                    UpdateSecurityAnswer1,
                    UpdateSelectedSecurityQuestion2.SecurityQuestionId,
                    UpdateSecurityAnswer2,
                    UpdateSelectedSecurityQuestion3.SecurityQuestionId,
                    UpdateSecurityAnswer3,
                    SessionService.CurrentUser.UserId,
                    SessionService.CurrentUser.UserCode,
                    SessionService.CurrentUser.Username
                );

                UpdateSuccessTitle = "User info updated successfully";
                UpdateSuccessMessage = $"{userBeingUpdated.FirstName} {userBeingUpdated.LastName}'s security questions have been updated.";
                IsUpdateSuccessPromptVisible = true;

                ResetUpdateSecurityFields();
            }
            catch (Exception ex)
            {
                ShowUpdateUserError($"Failed to update security questions: {ex.Message}");
            }
        }

        #endregion

        #region Archive and Restore Methods

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

            if (!user.IsActive)
            {
                ShowError("This user is already archived.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to archive {user.FirstName} {user.LastName}?\n\nThis user will no longer appear in the active user list.",
                "Confirm Archive",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

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

        private void RestoreUser(UserListItem? user)
        {
            if (user == null)
            {
                ShowError("Please select a user to restore.");
                return;
            }

            if (SessionService.CurrentUser == null)
            {
                ShowError("No logged-in user was found.");
                return;
            }

            if (user.IsActive)
            {
                ShowError("This user is already active.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to restore {user.FirstName} {user.LastName}?\n\nThis user will become active again.",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                userRepository.RestoreUser(
                    user.UserId,
                    SessionService.CurrentUser.UserId,
                    SessionService.CurrentUser.UserCode,
                    SessionService.CurrentUser.Username
                );

                LoadUsers();
            }
            catch (Exception ex)
            {
                ShowError($"Restore failed: {ex.Message}");
            }
        }

        private int GetUserCodePrefixRank(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                return 99;

            // Normal clinic user IDs should appear before dev accounts.
            if (userCode.StartsWith("2026-", StringComparison.OrdinalIgnoreCase))
                return 1;

            if (userCode.StartsWith("DEV-", StringComparison.OrdinalIgnoreCase))
                return 2;

            return 99;
        }

        private int GetUserCodeNumber(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                return 0;

            string[] parts = userCode.Split('-');

            if (parts.Length < 2)
                return 0;

            return int.TryParse(parts[1], out int number) ? number : 0;
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            RefreshUsersView();
        }

        #endregion

        #region Validation Helpers

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

        private bool HasMinimumPasswordLength(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 12;
        }

        private bool HasUppercaseCharacter(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, @"[A-Z]");
        }

        private bool HasLowercaseCharacter(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, @"[a-z]");
        }

        private bool HasNumberCharacter(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, @"[0-9]");
        }

        private bool HasSpecialCharacter(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, @"[^a-zA-Z0-9]");
        }

        private bool IsStrongPassword(string password)
        {
            return HasMinimumPasswordLength(password) &&
                HasUppercaseCharacter(password) &&
                HasLowercaseCharacter(password) &&
                HasNumberCharacter(password) &&
                HasSpecialCharacter(password);
        }

        private bool ConfirmUpdateAction(string title, string message)
        {
            MessageBoxResult result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            return result == MessageBoxResult.Yes;
        }

        #endregion

        #region Utility, Reset, and Error Helpers

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
                MiddleName = user.MiddleName,
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

        private void ResetUpdatePasswordFields()
        {
            UpdateOldPassword = string.Empty;
            UpdateNewPassword = string.Empty;
            UpdateConfirmNewPassword = string.Empty;

            IsUpdateOldPasswordVisible = false;
            IsUpdateNewPasswordVisible = false;
            IsUpdateConfirmNewPasswordVisible = false;
        }

        private void ResetUpdateSecurityFields()
        {
            UpdateSelectedSecurityQuestion1 = null;
            UpdateSelectedSecurityQuestion2 = null;
            UpdateSelectedSecurityQuestion3 = null;

            UpdateSecurityAnswer1 = string.Empty;
            UpdateSecurityAnswer2 = string.Empty;
            UpdateSecurityAnswer3 = string.Empty;
        }

        private void ShowUpdateUserError(string message)
        {
            UpdateUserErrorMessage = message;
            HasUpdateUserError = true;
        }

        private void ClearUpdateUserError()
        {
            UpdateUserErrorMessage = string.Empty;
            HasUpdateUserError = false;
        }

        #endregion


    }
}