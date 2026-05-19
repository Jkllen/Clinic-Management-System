using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    public class PatientManagementViewModel : BaseViewModel
    {
        #region Dependencies and Backing Fields

        private readonly PatientRepository patientRepository;
        private readonly List<PatientListItem> allPatientItems = new();

        private int totalPatients;
        private int newPatientsThisMonth;
        private int pwdSeniorPatients;
        private int calculatedAge;
        private int patientsWithBalance;

        private string searchText = string.Empty;
        private string selectedFilterOption = "All Active Patients";
        private string selectedSortOption = "Patient ID Ascending";

        private PatientListItem? selectedPatient;

        private string errorMessage = string.Empty;
        private bool hasError;

        private bool isAddPatientOverlayOpen;
        private bool isUpdatePatientOverlayOpen;
        private bool isPatientHistoryOverlayOpen;

        private PatientListItem? patientBeingUpdated;
        private Patient? selectedPatientDetails;

        private string patientFormTitle = "Add New Patient";

        private string formFirstName = string.Empty;
        private string formMiddleName = string.Empty;
        private string formLastName = string.Empty;
        private string formPhoneNumber = string.Empty;
        private DateTime? formDateOfBirth = DateTime.Today;
        private string formGender = string.Empty;
        private bool formIsPwd;
        private bool formIsSeniorCitizen;

        private bool formHasMedicalCondition;
        private string formMedicalConditionNotes = string.Empty;
        private string formAllergyNotes = string.Empty;
        private string formCurrentMedication = string.Empty;
        private bool formRequiresMedicalClearance;
        private string formClearanceNotes = string.Empty;
        private string formInitialTreatmentNotes = string.Empty;

        private string formInitialTreatment = string.Empty;
        private string formAddress = string.Empty;

        private string patientFormErrorMessage = string.Empty;
        private bool hasPatientFormError;

        private string historyPatientName = string.Empty;
        private string historyMedicalBackground = string.Empty;
        private string historyInitialVisit = string.Empty;
        private string historyTreatmentHistory = string.Empty;
        private ServiceItem? selectedService;
        private string otherServiceName = string.Empty;
        private bool isOtherServiceSelected;

        #endregion

        #region Constructor

        public PatientManagementViewModel()
        {
            patientRepository = new PatientRepository();

            Patients = new ObservableCollection<PatientListItem>();

            FilterOptions = new ObservableCollection<string>
            {
                "All Active Patients",
                "All Patients",
                "PWD/Senior Patients",
                "Patients With Balance",
                "Archived Patients"
            };

            SortOptions = new ObservableCollection<string>
            {
                "Patient ID Ascending",
                "Patient ID Descending",
                "Last Name A-Z",
                "Last Name Z-A",
                "First Name A-Z",
                "First Name Z-A"
            };

            GenderOptions = new ObservableCollection<string>
            {
                "Male",
                "Female",
                "Other"
            };

            LoadServiceOptions();

            AddNewPatientCommand = new RelayCommand(OpenAddPatientOverlay);
            SaveNewPatientCommand = new RelayCommand(SaveNewPatient);
            CancelAddPatientCommand = new RelayCommand(CloseAddPatientOverlay);

            UpdatePatientCommand = new RelayCommand<PatientListItem>(OpenUpdatePatientOverlay);
            SaveUpdatedPatientCommand = new RelayCommand(SaveUpdatedPatient);
            CancelUpdatePatientCommand = new RelayCommand(CloseUpdatePatientOverlay);

            ArchivePatientCommand = new RelayCommand<PatientListItem>(ArchiveOrRestorePatient);

            ViewPatientHistoryCommand = new RelayCommand<PatientListItem>(OpenPatientHistoryOverlay);
            ClosePatientHistoryCommand = new RelayCommand(ClosePatientHistoryOverlay);

            LoadPatients();
        }

        #endregion

        #region Collections

        public ObservableCollection<PatientListItem> Patients { get; }

        public ObservableCollection<string> FilterOptions { get; }

        public ObservableCollection<string> SortOptions { get; }

        public ObservableCollection<string> GenderOptions { get; }

        public ObservableCollection<ServiceItem> ServiceOptions { get; } = new();

        #endregion

        #region Summary Card Properties

        public int TotalPatients
        {
            get => totalPatients;
            set => SetProperty(ref totalPatients, value);
        }

        public int NewPatientsThisMonth
        {
            get => newPatientsThisMonth;
            set => SetProperty(ref newPatientsThisMonth, value);
        }

        public int PwdSeniorPatients
        {
            get => pwdSeniorPatients;
            set => SetProperty(ref pwdSeniorPatients, value);
        }

        public int PatientsWithBalance
        {
            get => patientsWithBalance;
            set => SetProperty(ref patientsWithBalance, value);
        }

        #endregion

        #region Search, Filter, Sort Properties

        public string SearchText
        {
            get => searchText;
            set
            {
                SetProperty(ref searchText, value);
                RefreshPatientsView();
            }
        }

        public string SelectedFilterOption
        {
            get => selectedFilterOption;
            set
            {
                SetProperty(ref selectedFilterOption, value);
                RefreshPatientsView();
            }
        }

        public string SelectedSortOption
        {
            get => selectedSortOption;
            set
            {
                SetProperty(ref selectedSortOption, value);
                RefreshPatientsView();
            }
        }

        #endregion

        #region Selection and Error Properties

        public PatientListItem? SelectedPatient
        {
            get => selectedPatient;
            set => SetProperty(ref selectedPatient, value);
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

        #region Overlay Visibility Properties

        public bool IsAddPatientOverlayOpen
        {
            get => isAddPatientOverlayOpen;
            set => SetProperty(ref isAddPatientOverlayOpen, value);
        }

        public bool IsUpdatePatientOverlayOpen
        {
            get => isUpdatePatientOverlayOpen;
            set => SetProperty(ref isUpdatePatientOverlayOpen, value);
        }

        public bool IsPatientHistoryOverlayOpen
        {
            get => isPatientHistoryOverlayOpen;
            set => SetProperty(ref isPatientHistoryOverlayOpen, value);
        }

        #endregion

        #region Patient Form Properties

        public string PatientFormTitle
        {
            get => patientFormTitle;
            set => SetProperty(ref patientFormTitle, value);
        }

        public string FormFirstName
        {
            get => formFirstName;
            set => SetProperty(ref formFirstName, value);
        }

        public string FormMiddleName
        {
            get => formMiddleName;
            set => SetProperty(ref formMiddleName, value);
        }

        public string FormLastName
        {
            get => formLastName;
            set => SetProperty(ref formLastName, value);
        }

        public string FormPhoneNumber
        {
            get => formPhoneNumber;
            set => SetProperty(ref formPhoneNumber, value);
        }

        public DateTime? FormDateOfBirth
        {
            get => formDateOfBirth;
            set
            {
                if (SetProperty(ref formDateOfBirth, value))
                {
                    UpdateCalculatedAgeAndSeniorStatus();
                }
            }
        }

        public string FormGender
        {
            get => formGender;
            set => SetProperty(ref formGender, value);
        }

        public bool FormIsPwd
        {
            get => formIsPwd;
            set => SetProperty(ref formIsPwd, value);
        }

        public int CalculatedAge
        {
            get => calculatedAge;
            set => SetProperty(ref calculatedAge, value);
        }

        public string CalculatedAgeDisplay =>
            FormDateOfBirth.HasValue ? $"{CalculatedAge} years old" : "Select date of birth";

        public bool IsSeniorCitizenAutoChecked =>
            CalculatedAge >= 60;

        public bool FormIsSeniorCitizen
        {
            get => formIsSeniorCitizen;
            set => SetProperty(ref formIsSeniorCitizen, value);
        }


        public bool FormHasMedicalCondition
        {
            get => formHasMedicalCondition;
            set
            {
                if (SetProperty(ref formHasMedicalCondition, value))
                {
                    OnPropertyChanged(nameof(MedicalConditionNotesVisibility));

                    if (!value)
                        FormMedicalConditionNotes = string.Empty;
                }
            }
        }
        public Visibility MedicalConditionNotesVisibility =>
            FormHasMedicalCondition ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ClearanceNotesVisibility =>
            FormRequiresMedicalClearance ? Visibility.Visible : Visibility.Collapsed;

        public string FormMedicalConditionNotes
        {
            get => formMedicalConditionNotes;
            set => SetProperty(ref formMedicalConditionNotes, value);
        }

        public string FormAllergyNotes
        {
            get => formAllergyNotes;
            set => SetProperty(ref formAllergyNotes, value);
        }

        public string FormCurrentMedication
        {
            get => formCurrentMedication;
            set => SetProperty(ref formCurrentMedication, value);
        }

        public bool FormRequiresMedicalClearance
        {
            get => formRequiresMedicalClearance;
            set
            {
                if (SetProperty(ref formRequiresMedicalClearance, value))
                {
                    OnPropertyChanged(nameof(ClearanceNotesVisibility));

                    if (!value)
                        FormClearanceNotes = string.Empty;
                }
            }
        }

        public string FormClearanceNotes
        {
            get => formClearanceNotes;
            set => SetProperty(ref formClearanceNotes, value);
        }

        public string FormInitialTreatmentNotes
        {
            get => formInitialTreatmentNotes;
            set => SetProperty(ref formInitialTreatmentNotes, value);
        }

        public string FormInitialTreatment
        {
            get => formInitialTreatment;
            set => SetProperty(ref formInitialTreatment, value);
        }

        public ServiceItem? SelectedService
        {
            get => selectedService;
            set
            {
                SetProperty(ref selectedService, value);

                IsOtherServiceSelected = selectedService?.ServiceName == "Other";

                if (!IsOtherServiceSelected)
                    OtherServiceName = string.Empty;
            }
        }

        public string OtherServiceName
        {
            get => otherServiceName;
            set => SetProperty(ref otherServiceName, value);
        }

        public bool IsOtherServiceSelected
        {
            get => isOtherServiceSelected;
            set => SetProperty(ref isOtherServiceSelected, value);
        }

        public string FormAddress
        {
            get => formAddress;
            set => SetProperty(ref formAddress, value);
        }

        public string PatientFormErrorMessage
        {
            get => patientFormErrorMessage;
            set => SetProperty(ref patientFormErrorMessage, value);
        }

        public bool HasPatientFormError
        {
            get => hasPatientFormError;
            set => SetProperty(ref hasPatientFormError, value);
        }

        #endregion

        #region Patient History Properties

        public string HistoryPatientName
        {
            get => historyPatientName;
            set => SetProperty(ref historyPatientName, value);
        }

        public string HistoryMedicalBackground
        {
            get => historyMedicalBackground;
            set => SetProperty(ref historyMedicalBackground, value);
        }

        public string HistoryInitialVisit
        {
            get => historyInitialVisit;
            set => SetProperty(ref historyInitialVisit, value);
        }

        public string HistoryTreatmentHistory
        {
            get => historyTreatmentHistory;
            set => SetProperty(ref historyTreatmentHistory, value);
        }

        #endregion

        #region Commands

        public ICommand AddNewPatientCommand { get; }

        public ICommand SaveNewPatientCommand { get; }

        public ICommand CancelAddPatientCommand { get; }

        public ICommand UpdatePatientCommand { get; }

        public ICommand SaveUpdatedPatientCommand { get; }

        public ICommand CancelUpdatePatientCommand { get; }

        public ICommand ArchivePatientCommand { get; }

        public ICommand ViewPatientHistoryCommand { get; }

        public ICommand ClosePatientHistoryCommand { get; }

        #endregion

        #region Load and Refresh Methods

        private void LoadPatients()
        {
            try
            {
                ClearError();

                allPatientItems.Clear();

                List<PatientListItem> patientsFromDatabase = patientRepository.GetPatientListItems();

                foreach (PatientListItem patient in patientsFromDatabase)
                    allPatientItems.Add(patient);

                RefreshSummaryCards();
                RefreshPatientsView();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load patients: {ex.Message}");
            }
        }

        private void LoadServiceOptions()
        {
            try
            {
                ServiceOptions.Clear();

                List<ServiceItem> servicesFromDatabase = patientRepository.GetActiveServices();

                foreach (ServiceItem service in servicesFromDatabase)
                    ServiceOptions.Add(service);

                ServiceOptions.Add(new ServiceItem
                {
                    ServiceId = 0,
                    ServiceName = "Other",
                    DefaultPrice = 0,
                    IsActive = true
                });
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load services: {ex.Message}");
            }
        }

        private void RefreshSummaryCards()
        {
            TotalPatients = allPatientItems.Count(patient => patient.IsActive);
            NewPatientsThisMonth = patientRepository.GetNewPatientsThisMonthCount();
            PwdSeniorPatients = allPatientItems.Count(patient => patient.IsActive && (patient.IsPwd || patient.IsSenior));
            PatientsWithBalance = allPatientItems.Count(patient => patient.IsActive && patient.HasBalance);
        }

        private void RefreshPatientsView()
        {
            IEnumerable<PatientListItem> query = allPatientItems;

            query = SelectedFilterOption switch
            {
                "All Active Patients" => query.Where(patient => patient.IsActive),
                "All Patients" => query,
                "PWD/Senior Patients" => query.Where(patient => patient.IsActive && (patient.IsPwd || patient.IsSenior)),
                "Patients With Balance" => query.Where(patient => patient.IsActive && patient.HasBalance),
                "Archived Patients" => query.Where(patient => !patient.IsActive),
                _ => query.Where(patient => patient.IsActive)
            };

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.Trim().ToLower();

                query = query.Where(patient =>
                    patient.PatientCode.ToLower().Contains(keyword) ||
                    patient.LastName.ToLower().Contains(keyword) ||
                    patient.FirstName.ToLower().Contains(keyword) ||
                    patient.MiddleName.ToLower().Contains(keyword) ||
                    patient.PhoneNumber.ToLower().Contains(keyword) ||
                    patient.Gender.ToLower().Contains(keyword) ||
                    patient.Treatment.ToLower().Contains(keyword));
            }

            query = SelectedSortOption switch
            {
                "Patient ID Ascending" => query.OrderBy(patient => GetPatientCodeNumber(patient.PatientCode)),
                "Patient ID Descending" => query.OrderByDescending(patient => GetPatientCodeNumber(patient.PatientCode)),
                "Last Name A-Z" => query.OrderBy(patient => patient.LastName).ThenBy(patient => patient.FirstName),
                "Last Name Z-A" => query.OrderByDescending(patient => patient.LastName).ThenByDescending(patient => patient.FirstName),
                "First Name A-Z" => query.OrderBy(patient => patient.FirstName).ThenBy(patient => patient.LastName),
                "First Name Z-A" => query.OrderByDescending(patient => patient.FirstName).ThenByDescending(patient => patient.LastName),
                _ => query.OrderBy(patient => GetPatientCodeNumber(patient.PatientCode))
            };

            Patients.Clear();

            foreach (PatientListItem patient in query)
                Patients.Add(patient);
        }

        #endregion

        #region Add Patient Methods

        private void OpenAddPatientOverlay()
        {
            ClearError();
            ClearPatientFormError();
            ClearPatientForm();

            PatientFormTitle = "Add New Patient";
            IsAddPatientOverlayOpen = true;
        }
        public void OpenAddPatientOverlayFromNavigation()
        {
            OpenAddPatientOverlay();
        }

        private void SaveNewPatient()
        {
            try
            {
                ClearPatientFormError();

                if (!ValidatePatientForm(requireInitialTreatment: true))
                    return;

                PatientListItem? duplicateByIdentity = patientRepository.FindDuplicatePatientByIdentity(
                    FormFirstName,
                    FormMiddleName,
                    FormLastName,
                    FormDateOfBirth!.Value
                );

                if (duplicateByIdentity != null)
                {
                    ShowPatientFormError(
                        $"Duplicate patient found: {duplicateByIdentity.PatientCode} - " +
                        $"{duplicateByIdentity.FirstName} {duplicateByIdentity.LastName} has the same full name and date of birth. " +
                        "Saving was stopped to prevent duplicate records."
                    );
                    return;
                }

                Patient patient = BuildPatientFromForm();

                patientRepository.AddPatient(patient);

                IsAddPatientOverlayOpen = false;

                MessageBox.Show(
                    "Patient was added successfully.",
                    "Patient Added",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadPatients();
            }
            catch (Exception ex)
            {
                ShowPatientFormError($"Failed to add patient: {ex.Message}");
            }
        }

        private void CloseAddPatientOverlay()
        {
            IsAddPatientOverlayOpen = false;
            ClearPatientFormError();
        }

        #endregion

        #region Update Patient Methods

        private void OpenUpdatePatientOverlay(PatientListItem? patient)
        {
            if (patient == null)
            {
                ShowError("Please select a patient to update.");
                return;
            }

            try
            {
                ClearError();
                ClearPatientFormError();

                patientBeingUpdated = patient;

                Patient? patientDetails = patientRepository.GetPatientById(patient.PatientId);

                if (patientDetails == null)
                {
                    ShowError("Selected patient record was not found.");
                    return;
                }

                selectedPatientDetails = patientDetails;

                PatientFormTitle = $"Update Patient - {patientDetails.PatientCode}";
                FillFormFromPatient(patientDetails);

                IsUpdatePatientOverlayOpen = true;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open update patient form: {ex.Message}");
            }
        }

        private void SaveUpdatedPatient()
        {
            try
            {
                ClearPatientFormError();

                if (selectedPatientDetails == null)
                {
                    ShowPatientFormError("No patient is currently selected for update.");
                    return;
                }

                if (!ValidatePatientForm(requireInitialTreatment: false))
                    return;

                PatientListItem? duplicateByIdentity = patientRepository.FindDuplicatePatientByIdentity(
                    FormFirstName,
                    FormMiddleName,
                    FormLastName,
                    FormDateOfBirth!.Value,
                    selectedPatientDetails.PatientId
                );

                if (duplicateByIdentity != null)
                {
                    ShowPatientFormError(
                        $"Duplicate patient found: {duplicateByIdentity.PatientCode} - " +
                        $"{duplicateByIdentity.FirstName} {duplicateByIdentity.LastName} has the same full name and date of birth. " +
                        "Updating was stopped to prevent duplicate records."
                    );
                    return;
                }

                Patient updatedPatient = BuildPatientFromForm();
                updatedPatient.PatientId = selectedPatientDetails.PatientId;
                updatedPatient.PatientCode = selectedPatientDetails.PatientCode;

                // Preserve initial visit details.
                // Update Patient should not overwrite treatment data.
                // New treatments will later be recorded through Appointment/Treatment Records.
                updatedPatient.InitialTreatment = selectedPatientDetails.InitialTreatment;
                updatedPatient.InitialTreatmentNotes = selectedPatientDetails.InitialTreatmentNotes;

                patientRepository.UpdatePatient(updatedPatient);
                IsUpdatePatientOverlayOpen = false;

                MessageBox.Show(
                    "Patient was updated successfully.",
                    "Patient Updated",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadPatients();
            }
            catch (Exception ex)
            {
                ShowPatientFormError($"Failed to update patient: {ex.Message}");
            }
        }

        private void CloseUpdatePatientOverlay()
        {
            IsUpdatePatientOverlayOpen = false;
            ClearPatientFormError();
        }

        #endregion

        #region Archive and Restore Methods

        private void ArchiveOrRestorePatient(PatientListItem? patient)
        {
            if (patient == null)
            {
                ShowError("Please select a patient.");
                return;
            }

            string action = patient.IsActive ? "archive" : "restore";

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to {action} {patient.FirstName} {patient.LastName}?",
                patient.IsActive ? "Confirm Archive" : "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                patientRepository.SetPatientActiveStatus(patient.PatientId, !patient.IsActive);
                LoadPatients();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to {action} patient: {ex.Message}");
            }
        }

        #endregion

        #region Patient History Methods

        private void OpenPatientHistoryOverlay(PatientListItem? patient)
        {
            if (patient == null)
            {
                ShowError("Please select a patient first.");
                return;
            }

            try
            {
                ClearError();

                Patient? patientDetails = patientRepository.GetPatientById(patient.PatientId);

                if (patientDetails == null)
                {
                    ShowError("Selected patient record was not found.");
                    return;
                }

                HistoryPatientName = $"{patientDetails.PatientCode} - {patientDetails.FullName}";
                HistoryMedicalBackground = BuildMedicalBackgroundSummary(patientDetails);
                HistoryInitialVisit = BuildInitialVisitSummary(patientDetails);

                // Real dental/treatment history should come from completed appointments or TreatmentRecords later.
                HistoryTreatmentHistory = "No treatment history recorded yet. Treatment records will be added after completed appointments or future treatment entries.";

                IsPatientHistoryOverlayOpen = true;
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open patient history: {ex.Message}");
            }
        }

        private string BuildMedicalBackgroundSummary(Patient patient)
        {
            List<string> lines = new();

            lines.Add($"Has Medical Condition: {(patient.HasMedicalCondition ? "Yes" : "No")}");

            if (!string.IsNullOrWhiteSpace(patient.MedicalConditionNotes))
                lines.Add($"Medical Conditions / Health Notes: {patient.MedicalConditionNotes}");

            if (!string.IsNullOrWhiteSpace(patient.AllergyNotes))
                lines.Add($"Allergy Notes: {patient.AllergyNotes}");

            if (!string.IsNullOrWhiteSpace(patient.CurrentMedication))
                lines.Add($"Current Medication: {patient.CurrentMedication}");

            lines.Add($"Requires Medical Clearance: {(patient.RequiresMedicalClearance ? "Yes" : "No")}");

            if (!string.IsNullOrWhiteSpace(patient.ClearanceNotes))
                lines.Add($"Clearance Notes: {patient.ClearanceNotes}");

            if (lines.Count == 2 &&
                !patient.HasMedicalCondition &&
                !patient.RequiresMedicalClearance &&
                string.IsNullOrWhiteSpace(patient.MedicalConditionNotes) &&
                string.IsNullOrWhiteSpace(patient.AllergyNotes) &&
                string.IsNullOrWhiteSpace(patient.CurrentMedication) &&
                string.IsNullOrWhiteSpace(patient.ClearanceNotes))
            {
                return "No medical background notes recorded.";
            }

            return string.Join(Environment.NewLine, lines);
        }

        private string BuildInitialVisitSummary(Patient patient)
        {
            List<string> lines = new();

            if (!string.IsNullOrWhiteSpace(patient.InitialTreatment))
                lines.Add($"Initial Treatment / Service: {patient.InitialTreatment}");

            if (!string.IsNullOrWhiteSpace(patient.InitialTreatmentNotes))
                lines.Add($"Initial Treatment Notes: {patient.InitialTreatmentNotes}");

            if (lines.Count == 0)
                return "No initial visit notes recorded.";

            return string.Join(Environment.NewLine, lines);
        }
        private void ClosePatientHistoryOverlay()
        {
            IsPatientHistoryOverlayOpen = false;
        }



        #endregion

        #region Form Helpers

        private bool ValidatePatientForm(bool requireInitialTreatment)
        {
            if (string.IsNullOrWhiteSpace(FormFirstName))
            {
                ShowPatientFormError("First name is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(FormLastName))
            {
                ShowPatientFormError("Last name is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(FormPhoneNumber))
            {
                ShowPatientFormError("Contact number is required.");
                return false;
            }

            if (!FormDateOfBirth.HasValue)
            {
                ShowPatientFormError("Date of birth is required.");
                return false;
            }

            if (FormDateOfBirth.Value.Date > DateTime.Today)
            {
                ShowPatientFormError("Date of birth cannot be in the future.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(FormGender))
            {
                ShowPatientFormError("Gender is required.");
                return false;
            }

            if (FormHasMedicalCondition && string.IsNullOrWhiteSpace(FormMedicalConditionNotes))
            {
                ShowPatientFormError("Please enter the patient's medical condition notes.");
                return false;
            }

            if (FormRequiresMedicalClearance && string.IsNullOrWhiteSpace(FormClearanceNotes))
            {
                ShowPatientFormError("Please enter the medical clearance notes.");
                return false;
            }

            if (requireInitialTreatment)
            {
                if (SelectedService == null)
                {
                    ShowPatientFormError("Initial treatment/service is required.");
                    return false;
                }

                if (IsOtherServiceSelected && string.IsNullOrWhiteSpace(OtherServiceName))
                {
                    ShowPatientFormError("Please type the other treatment/service.");
                    return false;
                }
            }

            return true;
        }
        private Patient BuildPatientFromForm()
        {
            return new Patient
            {
                FirstName = FormFirstName.Trim(),
                MiddleName = FormMiddleName.Trim(),
                LastName = FormLastName.Trim(),
                PhoneNumber = FormPhoneNumber.Trim(),
                BirthDate = FormDateOfBirth!.Value.Date,
                Gender = FormGender.Trim(),
                Address = FormAddress.Trim(),
                IsPwd = FormIsPwd,
                IsSeniorCitizen = FormIsSeniorCitizen,

                HasMedicalCondition = FormHasMedicalCondition,
                MedicalConditionNotes = FormMedicalConditionNotes.Trim(),
                AllergyNotes = FormAllergyNotes.Trim(),
                CurrentMedication = FormCurrentMedication.Trim(),
                RequiresMedicalClearance = FormRequiresMedicalClearance,
                ClearanceNotes = FormClearanceNotes.Trim(),

                InitialTreatmentNotes = FormInitialTreatmentNotes.Trim(),
                InitialTreatment = GetSelectedServiceName()
            };
        }

        private void FillFormFromPatient(Patient patient)
        {
            FormFirstName = patient.FirstName;
            FormMiddleName = patient.MiddleName;
            FormLastName = patient.LastName;
            FormPhoneNumber = patient.PhoneNumber;
            FormDateOfBirth = patient.BirthDate;
            FormGender = patient.Gender;
            FormIsPwd = patient.IsPwd;
            FormIsSeniorCitizen = patient.IsSeniorCitizen;
            FormAddress = patient.Address;

            // Medical Background
            FormHasMedicalCondition = patient.HasMedicalCondition;
            FormMedicalConditionNotes = patient.MedicalConditionNotes;
            FormAllergyNotes = patient.AllergyNotes;
            FormCurrentMedication = patient.CurrentMedication;
            FormRequiresMedicalClearance = patient.RequiresMedicalClearance;
            FormClearanceNotes = patient.ClearanceNotes;

            // Initial Visit
            FormInitialTreatment = patient.InitialTreatment;
            FormInitialTreatmentNotes = patient.InitialTreatmentNotes;

            // These are only used by Add Patient now.
            // Clear them so Update Patient does not depend on service dropdown state.
            SelectedService = null;
            OtherServiceName = string.Empty;
            IsOtherServiceSelected = false;
        }

        private string GetSelectedServiceName()
        {
            if (SelectedService == null)
                return string.Empty;

            if (SelectedService.ServiceName == "Other")
                return OtherServiceName.Trim();

            return SelectedService.ServiceName.Trim();
        }

        private void UpdateCalculatedAgeAndSeniorStatus()
        {
            if (!FormDateOfBirth.HasValue)
            {
                CalculatedAge = 0;
                FormIsSeniorCitizen = false;
                OnPropertyChanged(nameof(CalculatedAgeDisplay));
                OnPropertyChanged(nameof(IsSeniorCitizenAutoChecked));
                return;
            }

            DateTime birthDate = FormDateOfBirth.Value.Date;
            DateTime today = DateTime.Today;

            if (birthDate > today)
            {
                CalculatedAge = 0;
                FormIsSeniorCitizen = false;
                OnPropertyChanged(nameof(CalculatedAgeDisplay));
                OnPropertyChanged(nameof(IsSeniorCitizenAutoChecked));
                return;
            }

            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            CalculatedAge = age;
            FormIsSeniorCitizen = age >= 60;

            OnPropertyChanged(nameof(CalculatedAgeDisplay));
            OnPropertyChanged(nameof(IsSeniorCitizenAutoChecked));
        }
        private void ClearPatientForm()
        {
            FormFirstName = string.Empty;
            FormMiddleName = string.Empty;
            FormLastName = string.Empty;
            FormPhoneNumber = string.Empty;
            FormDateOfBirth = DateTime.Today;
            CalculatedAge = 0;
            OnPropertyChanged(nameof(CalculatedAgeDisplay));
            OnPropertyChanged(nameof(IsSeniorCitizenAutoChecked));
            FormGender = string.Empty;
            FormIsPwd = false;
            FormIsSeniorCitizen = false;
            FormHasMedicalCondition = false;
            FormMedicalConditionNotes = string.Empty;
            FormAllergyNotes = string.Empty;
            FormCurrentMedication = string.Empty;
            FormRequiresMedicalClearance = false;
            FormClearanceNotes = string.Empty;
            FormInitialTreatmentNotes = string.Empty;
            FormInitialTreatment = string.Empty;
            SelectedService = null;
            OtherServiceName = string.Empty;
            IsOtherServiceSelected = false;
            FormAddress = string.Empty;
        }

        private void ApplySelectedServiceFromExistingTreatment(string treatment)
        {
            if (string.IsNullOrWhiteSpace(treatment))
            {
                SelectedService = null;
                OtherServiceName = string.Empty;
                IsOtherServiceSelected = false;
                return;
            }

            ServiceItem? matchedService = ServiceOptions
                .FirstOrDefault(service =>
                    service.ServiceName.Equals(treatment.Trim(), StringComparison.OrdinalIgnoreCase));

            if (matchedService != null)
            {
                SelectedService = matchedService;
                OtherServiceName = string.Empty;
                IsOtherServiceSelected = false;
                return;
            }

            ServiceItem? otherService = ServiceOptions
                .FirstOrDefault(service => service.ServiceName == "Other");

            SelectedService = otherService;
            OtherServiceName = treatment.Trim();
            IsOtherServiceSelected = true;
        }

        private void ShowPatientFormError(string message)
        {
            PatientFormErrorMessage = message;
            HasPatientFormError = true;
        }

        private void ClearPatientFormError()
        {
            PatientFormErrorMessage = string.Empty;
            HasPatientFormError = false;
        }

        #endregion

        #region General Helpers

        private int GetPatientCodeNumber(string patientCode)
        {
            if (string.IsNullOrWhiteSpace(patientCode))
                return 0;

            string digits = new(patientCode.Where(char.IsDigit).ToArray());

            return int.TryParse(digits, out int number) ? number : 0;
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

        #endregion
    }
}