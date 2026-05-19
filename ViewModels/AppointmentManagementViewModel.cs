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
    public class AppointmentManagementViewModel : BaseViewModel
    {
        #region Dependencies and Backing Fields

        private readonly AppointmentRepository appointmentRepository;
        private readonly List<AppointmentListItem> allAppointments = new();

        private int todaysAppointments;
        private int completedToday;
        private int cancelledToday;

        private string searchText = string.Empty;
        private string selectedFilterOption = "Today's Appointments";
        private string selectedSortOption = "Queue Priority";

        private bool isWalkInOverlayOpen;
        private bool isScheduledOverlayOpen;
        private bool isPatientSearchPopupOpen;

        public bool IsWalkInPatientSearchPopupOpen =>
            IsPatientSearchPopupOpen && IsWalkInOverlayOpen;

        public bool IsScheduledPatientSearchPopupOpen =>
            IsPatientSearchPopupOpen && IsScheduledOverlayOpen;

        private string formTitle = string.Empty;
        private string formPatientSearchText = string.Empty;
        private AppointmentPatientSearchItem? selectedPatient;
        private AppointmentServiceOption? selectedService;
        private AppointmentDentistOption? selectedDentist;

        private DateTime? formAppointmentDate = DateTime.Today;
        private string formAppointmentTimeText = string.Empty;
        private string formNotes = string.Empty;

        private string formErrorMessage = string.Empty;
        private bool hasFormError;

        private string errorMessage = string.Empty;
        private bool hasError;

        #endregion

        #region Constructor

        public AppointmentManagementViewModel()
        {
            appointmentRepository = new AppointmentRepository();

            Appointments = new ObservableCollection<AppointmentListItem>();
            PatientSearchResults = new ObservableCollection<AppointmentPatientSearchItem>();
            ServiceOptions = new ObservableCollection<AppointmentServiceOption>();
            DentistOptions = new ObservableCollection<AppointmentDentistOption>();

            FilterOptions = new ObservableCollection<string>
            {
                "Today's Appointments",
                "All Appointments",
                "Upcoming Scheduled",
                "Walk-in Queue",
                "Completed",
                "Cancelled"
            };

            SortOptions = new ObservableCollection<string>
            {
                "Queue Priority",
                "Appointment Date/Time Ascending",
                "Appointment Date/Time Descending",
                "Patient Name A-Z",
                "Status"
            };

            OpenWalkInOverlayCommand = new RelayCommand(OpenWalkInOverlay);
            OpenScheduledOverlayCommand = new RelayCommand(OpenScheduledOverlay);

            SaveWalkInCommand = new RelayCommand(SaveWalkInAppointment);
            SaveScheduledCommand = new RelayCommand(SaveScheduledAppointment);

            CancelWalkInCommand = new RelayCommand(CloseWalkInOverlay);
            CancelScheduledCommand = new RelayCommand(CloseScheduledOverlay);

            MarkArrivedCommand = new RelayCommand<AppointmentListItem>(MarkArrived);
            StartTreatmentCommand = new RelayCommand<AppointmentListItem>(StartTreatment);
            CompleteAppointmentCommand = new RelayCommand<AppointmentListItem>(CompleteAppointment);
            CancelAppointmentCommand = new RelayCommand<AppointmentListItem>(CancelAppointment);
            ToggleUrgentCommand = new RelayCommand<AppointmentListItem>(ToggleUrgent);

            LoadDropdownOptions();
            LoadAppointments();
        }

        #endregion

        #region Collections

        public ObservableCollection<AppointmentListItem> Appointments { get; }

        public ObservableCollection<string> FilterOptions { get; }

        public ObservableCollection<string> SortOptions { get; }

        public ObservableCollection<AppointmentPatientSearchItem> PatientSearchResults { get; }

        public ObservableCollection<AppointmentServiceOption> ServiceOptions { get; }

        public ObservableCollection<AppointmentDentistOption> DentistOptions { get; }

        #endregion

        #region Summary Properties

        public int TodaysAppointments
        {
            get => todaysAppointments;
            set => SetProperty(ref todaysAppointments, value);
        }

        public int CompletedToday
        {
            get => completedToday;
            set => SetProperty(ref completedToday, value);
        }

        public int CancelledToday
        {
            get => cancelledToday;
            set => SetProperty(ref cancelledToday, value);
        }

        #endregion

        #region Search, Filter, Sort

        public string SearchText
        {
            get => searchText;
            set
            {
                SetProperty(ref searchText, value);
                RefreshAppointmentsView();
            }
        }

        public string SelectedFilterOption
        {
            get => selectedFilterOption;
            set
            {
                SetProperty(ref selectedFilterOption, value);
                RefreshAppointmentsView();
            }
        }

        public string SelectedSortOption
        {
            get => selectedSortOption;
            set
            {
                SetProperty(ref selectedSortOption, value);
                RefreshAppointmentsView();
            }
        }

        #endregion

        #region Overlay Properties

        public bool IsWalkInOverlayOpen
        {
            get => isWalkInOverlayOpen;
            set
            {
                if (SetProperty(ref isWalkInOverlayOpen, value))
                {
                    OnPropertyChanged(nameof(IsWalkInPatientSearchPopupOpen));
                }
            }
        }

        public bool IsScheduledOverlayOpen
        {
            get => isScheduledOverlayOpen;
            set
            {
                if (SetProperty(ref isScheduledOverlayOpen, value))
                {
                    OnPropertyChanged(nameof(IsScheduledPatientSearchPopupOpen));
                }
            }
        }

        #endregion

        #region Form Properties

        public string FormTitle
        {
            get => formTitle;
            set => SetProperty(ref formTitle, value);
        }

        public string FormPatientSearchText
        {
            get => formPatientSearchText;
            set
            {
                SetProperty(ref formPatientSearchText, value);
                SearchPatientsForForm();
            }
        }

        public AppointmentPatientSearchItem? SelectedPatient
        {
            get => selectedPatient;
            set
            {
                if (SetProperty(ref selectedPatient, value))
                {
                    OnPropertyChanged(nameof(FormPatientCode));
                    OnPropertyChanged(nameof(FormPatientFirstName));
                    OnPropertyChanged(nameof(FormPatientLastName));
                    OnPropertyChanged(nameof(FormPatientCategory));
                }
            }
        }

        public string FormPatientCode => SelectedPatient?.PatientCode ?? string.Empty;

        public string FormPatientFirstName => SelectedPatient?.FirstName ?? string.Empty;

        public string FormPatientLastName => SelectedPatient?.LastName ?? string.Empty;

        public string FormPatientCategory => SelectedPatient?.Category ?? string.Empty;

        public bool IsPatientSearchPopupOpen
        {
            get => isPatientSearchPopupOpen;
            set
            {
                if (SetProperty(ref isPatientSearchPopupOpen, value))
                {
                    OnPropertyChanged(nameof(IsWalkInPatientSearchPopupOpen));
                    OnPropertyChanged(nameof(IsScheduledPatientSearchPopupOpen));
                }
            }
        }

        public AppointmentServiceOption? SelectedService
        {
            get => selectedService;
            set => SetProperty(ref selectedService, value);
        }

        public AppointmentDentistOption? SelectedDentist
        {
            get => selectedDentist;
            set => SetProperty(ref selectedDentist, value);
        }

        public DateTime? FormAppointmentDate
        {
            get => formAppointmentDate;
            set => SetProperty(ref formAppointmentDate, value);
        }

        public string FormAppointmentTimeText
        {
            get => formAppointmentTimeText;
            set => SetProperty(ref formAppointmentTimeText, value);
        }

        public string FormNotes
        {
            get => formNotes;
            set => SetProperty(ref formNotes, value);
        }

        public string FormErrorMessage
        {
            get => formErrorMessage;
            set => SetProperty(ref formErrorMessage, value);
        }

        public bool HasFormError
        {
            get => hasFormError;
            set => SetProperty(ref hasFormError, value);
        }

        #endregion

        #region Error Properties

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

        #region Commands

        public ICommand OpenWalkInOverlayCommand { get; }

        public ICommand OpenScheduledOverlayCommand { get; }

        public ICommand SaveWalkInCommand { get; }

        public ICommand SaveScheduledCommand { get; }

        public ICommand CancelWalkInCommand { get; }

        public ICommand CancelScheduledCommand { get; }

        public ICommand MarkArrivedCommand { get; }

        public ICommand StartTreatmentCommand { get; }

        public ICommand CompleteAppointmentCommand { get; }

        public ICommand CancelAppointmentCommand { get; }

        public ICommand ToggleUrgentCommand { get; }

        #endregion

        #region Load Methods

        private void LoadDropdownOptions()
        {
            LoadServices();
            LoadDentists();
        }

        private void LoadServices()
        {
            ServiceOptions.Clear();

            List<AppointmentServiceOption> services = appointmentRepository.GetActiveServices();

            foreach (AppointmentServiceOption service in services)
                ServiceOptions.Add(service);
        }

        private void LoadDentists()
        {
            DentistOptions.Clear();

            DentistOptions.Add(new AppointmentDentistOption
            {
                DentistUserId = null,
                DentistName = "Unassigned"
            });

            List<AppointmentDentistOption> dentists = appointmentRepository.GetActiveDentists();

            foreach (AppointmentDentistOption dentist in dentists)
                DentistOptions.Add(dentist);

            SelectedDentist = DentistOptions.FirstOrDefault();
        }

        private void LoadAppointments()
        {
            try
            {
                ClearError();

                allAppointments.Clear();

                List<AppointmentListItem> appointmentsFromDatabase = appointmentRepository.GetAppointmentListItems();

                foreach (AppointmentListItem appointment in appointmentsFromDatabase)
                    allAppointments.Add(appointment);

                RefreshSummaryCards();
                RefreshAppointmentsView();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load appointments: {ex.Message}");
            }
        }

        private void RefreshSummaryCards()
        {
            TodaysAppointments = appointmentRepository.GetTodayAppointmentCount();
            CompletedToday = appointmentRepository.GetCompletedTodayCount();
            CancelledToday = appointmentRepository.GetCancelledTodayCount();
        }

        #endregion

        #region Refresh View

        private void RefreshAppointmentsView()
        {
            IEnumerable<AppointmentListItem> query = allAppointments;
            DateTime today = DateTime.Today;

            query = SelectedFilterOption switch
            {
                "Today's Appointments" => query.Where(a => a.AppointmentDate.Date == today),
                "All Appointments" => query,
                "Upcoming Scheduled" => query.Where(a =>
                    a.AppointmentType == "Scheduled" &&
                    a.Status == "Scheduled" &&
                    a.AppointmentDate.Date >= today),
                "Walk-in Queue" => query.Where(a =>
                    a.AppointmentType == "Walk-In" &&
                    a.AppointmentDate.Date == today &&
                    a.Status is "Waiting" or "In Treatment"),
                "Completed" => query.Where(a => a.Status == "Completed"),
                "Cancelled" => query.Where(a => a.Status == "Cancelled"),
                _ => query.Where(a => a.AppointmentDate.Date == today)
            };

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.Trim().ToLower();

                query = query.Where(a =>
                    a.PatientCode.ToLower().Contains(keyword) ||
                    a.PatientName.ToLower().Contains(keyword) ||
                    a.AppointmentType.ToLower().Contains(keyword) ||
                    a.Category.ToLower().Contains(keyword) ||
                    a.ServiceName.ToLower().Contains(keyword) ||
                    a.DentistName.ToLower().Contains(keyword) ||
                    a.Status.ToLower().Contains(keyword));
            }

            query = SelectedSortOption switch
            {
                "Queue Priority" => query
                    .OrderBy(a => GetQueueRank(a))
                    .ThenBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .ThenBy(a => a.AppointmentId),

                "Appointment Date/Time Ascending" => query
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime),

                "Appointment Date/Time Descending" => query
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime),

                "Patient Name A-Z" => query
                    .OrderBy(a => a.PatientName),

                "Status" => query
                    .OrderBy(a => a.Status)
                    .ThenBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime),

                _ => query
                    .OrderBy(a => GetQueueRank(a))
                    .ThenBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
            };

            List<AppointmentListItem> displayList = query.ToList();

            ApplyQueueNumbers(displayList);

            Appointments.Clear();

            foreach (AppointmentListItem appointment in displayList)
                Appointments.Add(appointment);
        }

        private int GetQueueRank(AppointmentListItem appointment)
        {
            if (appointment.Status == "In Treatment")
                return 0;

            if (appointment.IsUrgent && appointment.Status == "Waiting")
                return 1;

            if (appointment.AppointmentType == "Scheduled" &&
                appointment.Status is "Scheduled" or "Waiting")
                return 2;

            if (appointment.AppointmentType == "Walk-In" &&
                appointment.Status == "Waiting")
                return 3;

            if (appointment.Status == "Completed")
                return 4;

            if (appointment.Status == "Cancelled")
                return 5;

            return 6;
        }

        private void ApplyQueueNumbers(List<AppointmentListItem> displayList)
        {
            int queueNumber = 1;

            foreach (AppointmentListItem appointment in displayList)
            {
                bool isTodayActiveQueue =
                    appointment.AppointmentDate.Date == DateTime.Today &&
                    appointment.Status is "Scheduled" or "Waiting" or "In Treatment";

                appointment.QueueNumber = isTodayActiveQueue ? queueNumber++ : null;
            }
        }

        #endregion

        #region Open and Close Overlay Methods

        private void OpenWalkInOverlay()
        {
            ClearForm();
            ClearFormError();

            FormTitle = "Add Walk-in Visit";
            FormAppointmentDate = DateTime.Today;
            FormAppointmentTimeText = DateTime.Now.ToString("hh:mm tt");
            SelectedDentist = DentistOptions.FirstOrDefault();

            IsWalkInOverlayOpen = true;
        }

        private void CloseWalkInOverlay()
        {
            IsPatientSearchPopupOpen = false;
            IsWalkInOverlayOpen = false;
            ClearFormError();
        }

        private void OpenScheduledOverlay()
        {
            ClearForm();
            ClearFormError();

            FormTitle = "Add Scheduled Appointment";
            FormAppointmentDate = DateTime.Today;
            FormAppointmentTimeText = DateTime.Now.AddHours(1).ToString("hh:mm tt");
            SelectedDentist = DentistOptions.FirstOrDefault();

            IsScheduledOverlayOpen = true;
        }

        private void CloseScheduledOverlay()
        {
            IsPatientSearchPopupOpen = false;
            IsScheduledOverlayOpen = false;
            ClearFormError();
        }

        #endregion

        #region Patient Search

        private void SearchPatientsForForm()
        {
            PatientSearchResults.Clear();

            if (string.IsNullOrWhiteSpace(FormPatientSearchText) ||
                FormPatientSearchText.Trim().Length < 2)
            {
                IsPatientSearchPopupOpen = false;
                return;
            }

            if (SelectedPatient != null &&
                FormPatientSearchText == SelectedPatient.DisplayText)
            {
                IsPatientSearchPopupOpen = false;
                return;
            }

            SelectedPatient = null;

            List<AppointmentPatientSearchItem> results =
                appointmentRepository.SearchActivePatients(FormPatientSearchText);

            foreach (AppointmentPatientSearchItem patient in results)
                PatientSearchResults.Add(patient);

            IsPatientSearchPopupOpen = PatientSearchResults.Count > 0;
        }

        #endregion

        #region Save Appointment Methods

        private void SaveWalkInAppointment()
        {
            try
            {
                ClearFormError();

                if (!ValidateAppointmentForm(isScheduled: false))
                    return;

                TimeSpan appointmentTime = ParseFormTime();

                Appointment appointment = new()
                {
                    PatientId = SelectedPatient!.PatientId,
                    AppointmentType = "Walk-In",
                    Category = SelectedPatient.Category,
                    ServiceId = SelectedService!.ServiceId,
                    ServiceName = SelectedService.ServiceName,
                    DentistUserId = SelectedDentist!.DentistUserId,
                    DentistName = SelectedDentist.DentistName,
                    AppointmentDate = DateTime.Today,
                    AppointmentTime = appointmentTime,
                    ArrivalTime = appointmentTime,
                    IsUrgent = false,
                    Priority = "Normal",
                    Status = "Waiting",
                    Notes = FormNotes.Trim()
                };

                appointmentRepository.AddAppointment(appointment);

                IsWalkInOverlayOpen = false;

                MessageBox.Show(
                    "Walk-in visit was added to the queue.",
                    "Walk-in Added",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadAppointments();
            }
            catch (Exception ex)
            {
                ShowFormError($"Failed to save walk-in visit: {ex.Message}");
            }
        }

        private void SaveScheduledAppointment()
        {
            try
            {
                ClearFormError();

                if (!ValidateAppointmentForm(isScheduled: true))
                    return;

                TimeSpan appointmentTime = ParseFormTime();

                Appointment appointment = new()
                {
                    PatientId = SelectedPatient!.PatientId,
                    AppointmentType = "Scheduled",
                    Category = SelectedPatient.Category,
                    ServiceId = SelectedService!.ServiceId,
                    ServiceName = SelectedService.ServiceName,
                    DentistUserId = SelectedDentist!.DentistUserId,
                    DentistName = SelectedDentist.DentistName,
                    AppointmentDate = FormAppointmentDate!.Value.Date,
                    AppointmentTime = appointmentTime,
                    ArrivalTime = null,
                    IsUrgent = false,
                    Priority = "Scheduled",
                    Status = "Scheduled",
                    Notes = FormNotes.Trim()
                };

                appointmentRepository.AddAppointment(appointment);

                IsScheduledOverlayOpen = false;

                MessageBox.Show(
                    "Scheduled appointment was saved successfully.",
                    "Appointment Scheduled",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                LoadAppointments();
            }
            catch (Exception ex)
            {
                ShowFormError($"Failed to save scheduled appointment: {ex.Message}");
            }
        }

        #endregion

        #region Status Action Methods

        private void MarkArrived(AppointmentListItem? appointment)
        {
            if (appointment == null)
                return;

            MessageBoxResult result = MessageBox.Show(
                $"Mark {appointment.PatientName} as arrived and add to queue?",
                "Confirm Arrival",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                appointmentRepository.MarkArrived(appointment.AppointmentId);
                LoadAppointments();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to mark patient as arrived: {ex.Message}");
            }
        }

        private void StartTreatment(AppointmentListItem? appointment)
        {
            if (appointment == null)
                return;

            MessageBoxResult result = MessageBox.Show(
                $"Start treatment for {appointment.PatientName}?",
                "Start Treatment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                bool started = appointmentRepository.StartTreatment(appointment.AppointmentId);

                if (!started)
                {
                    MessageBox.Show(
                        "Another patient is currently in treatment. Complete or cancel the current treatment before starting another.",
                        "Treatment Already In Progress",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                LoadAppointments();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to start treatment: {ex.Message}");
            }
        }

        private void CompleteAppointment(AppointmentListItem? appointment)
        {
            if (appointment == null)
                return;

            MessageBoxResult result = MessageBox.Show(
                $"Mark {appointment.PatientName}'s appointment as completed?",
                "Complete Appointment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                appointmentRepository.CompleteAppointment(appointment.AppointmentId);
                LoadAppointments();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to complete appointment: {ex.Message}");
            }
        }

        private void CancelAppointment(AppointmentListItem? appointment)
        {
            if (appointment == null)
                return;

            MessageBoxResult result = MessageBox.Show(
                $"Cancel appointment for {appointment.PatientName}?",
                "Cancel Appointment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                appointmentRepository.CancelAppointment(appointment.AppointmentId);
                LoadAppointments();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to cancel appointment: {ex.Message}");
            }
        }

        private void ToggleUrgent(AppointmentListItem? appointment)
        {
            if (appointment == null)
                return;

            bool newUrgentValue = !appointment.IsUrgent;

            string actionText = newUrgentValue ? "mark as urgent" : "remove urgent status from";

            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to {actionText} {appointment.PatientName}?",
                "Confirm Priority Change",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                appointmentRepository.ToggleUrgent(appointment.AppointmentId, newUrgentValue);
                LoadAppointments();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update urgent priority: {ex.Message}");
            }
        }

        #endregion

        #region Validation and Form Helpers

        private bool ValidateAppointmentForm(bool isScheduled)
        {
            if (SelectedPatient == null)
            {
                ShowFormError("Please search and select an existing patient.");
                return false;
            }

            if (SelectedService == null)
            {
                ShowFormError("Please select a service or treatment.");
                return false;
            }

            if (SelectedDentist == null)
            {
                ShowFormError("Please select a dentist or choose Unassigned.");
                return false;
            }

            if (isScheduled)
            {
                if (!FormAppointmentDate.HasValue)
                {
                    ShowFormError("Please select an appointment date.");
                    return false;
                }

                if (FormAppointmentDate.Value.Date < DateTime.Today)
                {
                    ShowFormError("Scheduled appointment date cannot be in the past.");
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(FormAppointmentTimeText))
            {
                ShowFormError("Please enter appointment time.");
                return false;
            }

            if (!TryParseTime(FormAppointmentTimeText, out _))
            {
                ShowFormError("Please enter a valid time. Example: 02:30 PM or 14:30.");
                return false;
            }

            return true;
        }

        private TimeSpan ParseFormTime()
        {
            TryParseTime(FormAppointmentTimeText, out TimeSpan time);
            return time;
        }

        private bool TryParseTime(string value, out TimeSpan time)
        {
            time = TimeSpan.Zero;

            if (TimeSpan.TryParse(value, out time))
                return true;

            if (DateTime.TryParse(value, out DateTime dateTime))
            {
                time = dateTime.TimeOfDay;
                return true;
            }

            return false;
        }

        private void ClearForm()
        {
            FormPatientSearchText = string.Empty;
            SelectedPatient = null;
            PatientSearchResults.Clear();

            SelectedService = null;
            SelectedDentist = DentistOptions.FirstOrDefault();

            FormAppointmentDate = DateTime.Today;
            FormAppointmentTimeText = string.Empty;
            FormNotes = string.Empty;
        }

        private void ShowFormError(string message)
        {
            FormErrorMessage = message;
            HasFormError = true;
        }

        private void ClearFormError()
        {
            FormErrorMessage = string.Empty;
            HasFormError = false;
        }

        #endregion

        #region General Helpers

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