using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    public class BillingViewModel : BaseViewModel
    {
        #region Dependencies and Backing Fields

        private readonly BillingRepository billingRepository;

        private AppointmentPaymentItem? selectedAppointmentPaymentItem;

        private string appointmentReceiptNumber = string.Empty;
        private string appointmentPatientCode = string.Empty;
        private string appointmentPatientName = string.Empty;
        private string appointmentCategory = string.Empty;
        private string appointmentDate = string.Empty;
        private string appointmentServiceName = string.Empty;

        private decimal appointmentTotalAmount;
        private string appointmentDiscountType = "None";
        private decimal appointmentDiscountAmount;
        private decimal appointmentPaymentAmount;
        private decimal appointmentBalance;

        private string appointmentPaymentMethod = "Cash";
        private string appointmentNotes = string.Empty;

        private string appointmentPaymentErrorMessage = string.Empty;
        private bool hasAppointmentPaymentError;
        private string selectedBillingModule = "Appointment Payment";
        private string searchText = string.Empty;
        private string errorMessage = string.Empty;
        private bool hasError;

        #endregion

        #region Collections

        public ObservableCollection<BillingRecordListItem> BillingRecords { get; }

        public ObservableCollection<AppointmentPaymentItem> AppointmentPaymentItems { get; }

        public ObservableCollection<BalancePaymentItem> BalancePaymentItems { get; }

        public ObservableCollection<string> DiscountTypeOptions { get; }


        #endregion

        #region Properties

        public string SelectedBillingModule
        {
            get => selectedBillingModule;
            set
            {
                if (SetProperty(ref selectedBillingModule, value))
                {
                    OnPropertyChanged(nameof(IsAppointmentPaymentSelected));
                    OnPropertyChanged(nameof(IsBalancePaymentSelected));
                    OnPropertyChanged(nameof(IsManualTransactionSelected));

                    OnPropertyChanged(nameof(AppointmentPaymentTabStyle));
                    OnPropertyChanged(nameof(BalancePaymentTabStyle));
                    OnPropertyChanged(nameof(ManualTransactionTabStyle));

                    OnPropertyChanged(nameof(SelectedModuleTitle));
                    OnPropertyChanged(nameof(SelectedModuleSubtitle));

                    RefreshSelectedModuleData();
                }
            }
        }

        public bool IsAppointmentPaymentSelected =>
            SelectedBillingModule == "Appointment Payment";

        public bool IsBalancePaymentSelected =>
            SelectedBillingModule == "Balance Payment";

        public bool IsManualTransactionSelected =>
            SelectedBillingModule == "Manual Transaction";

        public string AppointmentPaymentTabStyle =>
            IsAppointmentPaymentSelected
                ? "SelectedBillingTabButtonStyle"
                : "BillingTabButtonStyle";

        public string BalancePaymentTabStyle =>
            IsBalancePaymentSelected
                ? "SelectedBillingTabButtonStyle"
                : "BillingTabButtonStyle";

        public string ManualTransactionTabStyle =>
            IsManualTransactionSelected
                ? "SelectedBillingTabButtonStyle"
                : "BillingTabButtonStyle";

        public string SelectedModuleTitle =>
            SelectedBillingModule switch
            {
                "Appointment Payment" => "Appointment Payment",
                "Balance Payment" => "Balance Payment",
                "Manual Transaction" => "Manual Transaction",
                _ => "Billing"
            };

        public string SelectedModuleSubtitle =>
            SelectedBillingModule switch
            {
                "Appointment Payment" => "Process payments connected to completed appointments.",
                "Balance Payment" => "Record payments for existing unpaid or partial balances.",
                "Manual Transaction" => "Create staff-entered billing transactions not pulled from appointments.",
                _ => string.Empty
            };

        public string SearchText
        {
            get => searchText;
            set => SetProperty(ref searchText, value);
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

        public AppointmentPaymentItem? SelectedAppointmentPaymentItem
        {
            get => selectedAppointmentPaymentItem;
            set
            {
                if (SetProperty(ref selectedAppointmentPaymentItem, value))
                {
                    FillAppointmentPaymentForm(value);
                }
            }
        }

        public string AppointmentReceiptNumber
        {
            get => appointmentReceiptNumber;
            set => SetProperty(ref appointmentReceiptNumber, value);
        }

        public string AppointmentPatientCode
        {
            get => appointmentPatientCode;
            set => SetProperty(ref appointmentPatientCode, value);
        }

        public string AppointmentPatientName
        {
            get => appointmentPatientName;
            set => SetProperty(ref appointmentPatientName, value);
        }

        public string AppointmentCategory
        {
            get => appointmentCategory;
            set => SetProperty(ref appointmentCategory, value);
        }

        public string AppointmentDate
        {
            get => appointmentDate;
            set => SetProperty(ref appointmentDate, value);
        }

        public string AppointmentServiceName
        {
            get => appointmentServiceName;
            set => SetProperty(ref appointmentServiceName, value);
        }

        public decimal AppointmentTotalAmount
        {
            get => appointmentTotalAmount;
            set
            {
                if (SetProperty(ref appointmentTotalAmount, value))
                    RecalculateAppointmentPayment();
            }
        }

        public string AppointmentDiscountType
        {
            get => appointmentDiscountType;
            set
            {
                if (SetProperty(ref appointmentDiscountType, value))
                    ApplyAppointmentDiscountType();
            }
        }

        public decimal AppointmentDiscountAmount
        {
            get => appointmentDiscountAmount;
            set
            {
                if (SetProperty(ref appointmentDiscountAmount, value))
                    RecalculateAppointmentPayment();
            }
        }

        public decimal AppointmentPaymentAmount
        {
            get => appointmentPaymentAmount;
            set
            {
                if (SetProperty(ref appointmentPaymentAmount, value))
                    RecalculateAppointmentPayment();
            }
        }

        public decimal AppointmentBalance
        {
            get => appointmentBalance;
            set => SetProperty(ref appointmentBalance, value);
        }

        public string AppointmentPaymentMethod
        {
            get => appointmentPaymentMethod;
            set => SetProperty(ref appointmentPaymentMethod, value);
        }

        public string AppointmentNotes
        {
            get => appointmentNotes;
            set => SetProperty(ref appointmentNotes, value);
        }

        public string AppointmentSubtotalDisplay =>
            $"₱{Math.Max(AppointmentTotalAmount - AppointmentDiscountAmount, 0):N2}";

        public string AppointmentBalanceDisplay =>
            $"₱{AppointmentBalance:N2}";

        public string AppointmentPaymentErrorMessage
        {
            get => appointmentPaymentErrorMessage;
            set => SetProperty(ref appointmentPaymentErrorMessage, value);
        }

        public bool HasAppointmentPaymentError
        {
            get => hasAppointmentPaymentError;
            set => SetProperty(ref hasAppointmentPaymentError, value);
        }

        #endregion

        #region Commands

        public ICommand SelectBillingModuleCommand { get; }

        public ICommand RefreshBillingCommand { get; }

        public ICommand ViewReceiptCommand { get; }

        public ICommand PrintReceiptCommand { get; }

        public ICommand ProcessAppointmentPaymentCommand { get; }

        public ICommand ClearAppointmentPaymentFormCommand { get; }

        #endregion

        #region Constructor

        public BillingViewModel()
        {
            billingRepository = new BillingRepository();

            BillingRecords = new ObservableCollection<BillingRecordListItem>();
            AppointmentPaymentItems = new ObservableCollection<AppointmentPaymentItem>();
            BalancePaymentItems = new ObservableCollection<BalancePaymentItem>();

            SelectBillingModuleCommand = new RelayCommand<string>(SelectBillingModule);
            RefreshBillingCommand = new RelayCommand(LoadBillingData);

            ViewReceiptCommand = new RelayCommand<BillingRecordListItem>(ViewReceipt);
            PrintReceiptCommand = new RelayCommand<BillingRecordListItem>(PrintReceipt);

            ProcessAppointmentPaymentCommand = new RelayCommand(ProcessAppointmentPayment);
            ClearAppointmentPaymentFormCommand = new RelayCommand(ClearAppointmentPaymentForm);

            DiscountTypeOptions = new ObservableCollection<string>
            {
                "None",
                "PWD",
                "Senior",
                "Manual"
            };

            LoadBillingData();
        }

        #endregion

        #region Load Methods

        private void LoadBillingData()
        {
            try
            {
                ClearError();

                LoadBillingRecords();
                LoadAppointmentPaymentItems();
                LoadBalancePaymentItems();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load billing data: {ex.Message}");
            }
        }

        private void LoadBillingRecords()
        {
            BillingRecords.Clear();

            foreach (BillingRecordListItem item in billingRepository.GetBillingRecords())
                BillingRecords.Add(item);
        }

        private void LoadAppointmentPaymentItems()
        {
            AppointmentPaymentItems.Clear();

            foreach (AppointmentPaymentItem item in billingRepository.GetUnbilledCompletedTreatments())
                AppointmentPaymentItems.Add(item);
        }

        private void LoadBalancePaymentItems()
        {
            BalancePaymentItems.Clear();

            foreach (BalancePaymentItem item in billingRepository.GetBillingsWithBalance())
                BalancePaymentItems.Add(item);
        }

        private void RefreshSelectedModuleData()
        {
            try
            {
                ClearError();

                if (IsAppointmentPaymentSelected)
                    LoadAppointmentPaymentItems();

                if (IsBalancePaymentSelected)
                    LoadBalancePaymentItems();

                if (IsManualTransactionSelected)
                    LoadBillingRecords();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to refresh billing data: {ex.Message}");
            }
        }

        #endregion

        #region Command Methods

        private void SelectBillingModule(string? moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                return;

            SelectedBillingModule = moduleName;
        }

        private void ViewReceipt(BillingRecordListItem? billing)
        {
            if (billing == null)
                return;

            // Receipt modal will be implemented in Batch E.4.
        }

        private void PrintReceipt(BillingRecordListItem? billing)
        {
            if (billing == null)
                return;

            // Printing will be implemented after receipt modal.
        }

        #endregion

        #region Appointment Payment Methods

        private void FillAppointmentPaymentForm(AppointmentPaymentItem? item)
        {
            ClearAppointmentPaymentError();

            if (item == null)
            {
                ClearAppointmentPaymentFormFieldsOnly();
                return;
            }

            AppointmentReceiptNumber = billingRepository.GenerateReceiptNumber();
            AppointmentPatientCode = item.PatientCode;
            AppointmentPatientName = item.PatientName;
            AppointmentCategory = string.Empty;
            AppointmentDate = item.TreatmentDateDisplay;
            AppointmentServiceName = item.ServiceName;

            AppointmentTotalAmount = item.DefaultPrice;
            AppointmentDiscountType = "None";
            AppointmentDiscountAmount = 0;
            AppointmentPaymentAmount = 0;
            AppointmentPaymentMethod = "Cash";
            AppointmentNotes = string.Empty;

            RecalculateAppointmentPayment();
        }

        private void ApplyAppointmentDiscountType()
        {
            if (AppointmentDiscountType == "None")
            {
                AppointmentDiscountAmount = 0;
                return;
            }

            if (AppointmentDiscountType is "PWD" or "Senior")
            {
                AppointmentDiscountAmount = Math.Round(AppointmentTotalAmount * 0.20m, 2);
                return;
            }

            // Manual discount keeps whatever staff typed.
            RecalculateAppointmentPayment();
        }

        private void RecalculateAppointmentPayment()
        {
            if (AppointmentDiscountAmount < 0)
                AppointmentDiscountAmount = 0;

            if (AppointmentDiscountAmount > AppointmentTotalAmount)
                AppointmentDiscountAmount = AppointmentTotalAmount;

            if (AppointmentPaymentAmount < 0)
                AppointmentPaymentAmount = 0;

            decimal netAmount = Math.Max(AppointmentTotalAmount - AppointmentDiscountAmount, 0);

            if (AppointmentPaymentAmount > netAmount)
                AppointmentPaymentAmount = netAmount;

            AppointmentBalance = billingRepository.CalculateBalance(netAmount, AppointmentPaymentAmount);

            OnPropertyChanged(nameof(AppointmentSubtotalDisplay));
            OnPropertyChanged(nameof(AppointmentBalanceDisplay));
        }

        private void ProcessAppointmentPayment()
        {
            ClearAppointmentPaymentError();

            if (SelectedAppointmentPaymentItem == null)
            {
                ShowAppointmentPaymentError("Please select a completed appointment or treatment record to bill.");
                return;
            }

            if (AppointmentTotalAmount <= 0)
            {
                ShowAppointmentPaymentError("Please enter a valid total amount.");
                return;
            }

            if (AppointmentDiscountAmount < 0)
            {
                ShowAppointmentPaymentError("Discount cannot be negative.");
                return;
            }

            decimal netAmount = Math.Max(AppointmentTotalAmount - AppointmentDiscountAmount, 0);

            if (AppointmentPaymentAmount < 0)
            {
                ShowAppointmentPaymentError("Payment amount cannot be negative.");
                return;
            }

            if (AppointmentPaymentAmount > netAmount)
            {
                ShowAppointmentPaymentError("Payment amount cannot be greater than the subtotal after discount.");
                return;
            }

            try
            {
                BillingTransaction billing = new()
                {
                    PatientId = SelectedAppointmentPaymentItem.PatientId,
                    AppointmentId = SelectedAppointmentPaymentItem.AppointmentId,
                    TreatmentRecordId = SelectedAppointmentPaymentItem.TreatmentRecordId,
                    BillingSource = "Appointment",
                    ReceiptNumber = AppointmentReceiptNumber,
                    ServiceId = SelectedAppointmentPaymentItem.ServiceId,
                    ServiceName = SelectedAppointmentPaymentItem.ServiceName,
                    Description = $"Appointment payment for {SelectedAppointmentPaymentItem.ServiceName}",
                    TotalAmount = AppointmentTotalAmount,
                    DiscountType = AppointmentDiscountType,
                    DiscountAmount = AppointmentDiscountAmount,
                    SubtotalAfterDiscount = netAmount,
                    AmountPaid = 0,
                    RemainingBalance = netAmount,
                    PaymentStatus = "Unpaid",
                    TransactionDate = DateTime.Today,
                    CreatedByUserId = null,
                    Notes = AppointmentNotes
                };

                int billingId = billingRepository.CreateBillingTransaction(billing);

                if (AppointmentPaymentAmount > 0)
                {
                    PaymentRecord payment = new()
                    {
                        BillingId = billingId,
                        PatientId = SelectedAppointmentPaymentItem.PatientId,
                        AmountPaid = AppointmentPaymentAmount,
                        PaymentMethod = AppointmentPaymentMethod,
                        PaymentDate = DateTime.Today,
                        ReceivedByUserId = null,
                        Notes = AppointmentNotes
                    };

                    billingRepository.AddPaymentRecord(payment);
                }

                LoadBillingData();
                ClearAppointmentPaymentForm();

                SelectedBillingModule = "Appointment Payment";
            }
            catch (Exception ex)
            {
                ShowAppointmentPaymentError($"Failed to process appointment payment: {ex.Message}");
            }
        }

        private void ClearAppointmentPaymentForm()
        {
            SelectedAppointmentPaymentItem = null;
            ClearAppointmentPaymentFormFieldsOnly();
            ClearAppointmentPaymentError();
        }

        private void ClearAppointmentPaymentFormFieldsOnly()
        {
            AppointmentReceiptNumber = string.Empty;
            AppointmentPatientCode = string.Empty;
            AppointmentPatientName = string.Empty;
            AppointmentCategory = string.Empty;
            AppointmentDate = string.Empty;
            AppointmentServiceName = string.Empty;

            AppointmentTotalAmount = 0;
            AppointmentDiscountType = "None";
            AppointmentDiscountAmount = 0;
            AppointmentPaymentAmount = 0;
            AppointmentBalance = 0;
            AppointmentPaymentMethod = "Cash";
            AppointmentNotes = string.Empty;

            OnPropertyChanged(nameof(AppointmentSubtotalDisplay));
            OnPropertyChanged(nameof(AppointmentBalanceDisplay));
        }

        private void ShowAppointmentPaymentError(string message)
        {
            AppointmentPaymentErrorMessage = message;
            HasAppointmentPaymentError = true;
        }

        private void ClearAppointmentPaymentError()
        {
            AppointmentPaymentErrorMessage = string.Empty;
            HasAppointmentPaymentError = false;
        }

        #endregion


        #region Error Helpers

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