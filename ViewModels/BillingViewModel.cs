using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Repositories;
using CruzNeryClinic.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Linq;
using System;

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

        private bool isTransactionHistoryOverlayOpen;
        private bool isReceiptOverlayOpen;
        private BillingReceiptDetail? selectedReceiptDetail;

        private bool isReceiptPrintPreviewOpen;
        private Uri? receiptPrintPreviewUri;

        private bool isPromptOpen;
        private string promptTitle = string.Empty;
        private string promptMessage = string.Empty;

        private string appointmentPaymentSearchText = string.Empty;
        private bool isAppointmentPaymentSearchPopupOpen;

        private string paymentHistorySearchText = string.Empty;
        private bool isPaymentHistorySearchPopupOpen;
        private BillingPatientLookupItem? selectedPaymentHistoryPatient;
        private string paymentHistoryPatientName = string.Empty;
        private bool hasSelectedPaymentHistoryPatient;


        private string errorMessage = string.Empty;
        private bool hasError;

        #endregion
        
        #region Backing Fields for Balance Payment Module

        private BalancePaymentItem? selectedBalancePaymentItem;

        private string balanceSearchText = string.Empty;

        private string balanceReceiptNumber = string.Empty;
        private string balancePatientName = string.Empty;
        private string balanceServiceName = string.Empty;

        private decimal balanceCurrentBalance;
        private decimal balancePaymentAmount;
        private decimal balanceRemainingAfterPayment;

        private string balancePaymentNotes = string.Empty;

        #endregion

        #region Collections

        public ObservableCollection<BillingRecordListItem> BillingRecords { get; }

        public ObservableCollection<AppointmentPaymentItem> AppointmentPaymentItems { get; }
        public ObservableCollection<AppointmentPaymentItem> FilteredAppointmentPaymentItems { get; }

        public ObservableCollection<BalancePaymentItem> BalancePaymentItems { get; }
        public ObservableCollection<BalancePaymentItem> FilteredBalancePaymentItems { get; }

        public ObservableCollection<BillingPatientLookupItem> PaymentHistoryPatientResults { get; }

        public ObservableCollection<BillingRecordListItem> SelectedPatientBillingRecords { get; }

        public ObservableCollection<BillingRecordListItem> LatestPatientBillingRecords { get; }

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

        public bool IsTransactionHistoryOverlayOpen
        {
            get => isTransactionHistoryOverlayOpen;
            set => SetProperty(ref isTransactionHistoryOverlayOpen, value);
        }

        public bool IsReceiptOverlayOpen
        {
            get => isReceiptOverlayOpen;
            set => SetProperty(ref isReceiptOverlayOpen, value);
        }

        public bool IsReceiptPrintPreviewOpen
        {
            get => isReceiptPrintPreviewOpen;
            set => SetProperty(ref isReceiptPrintPreviewOpen, value);
        }

        public Uri? ReceiptPrintPreviewUri
        {
            get => receiptPrintPreviewUri;
            set => SetProperty(ref receiptPrintPreviewUri, value);
        }

        public BillingReceiptDetail? SelectedReceiptDetail
        {
            get => selectedReceiptDetail;
            set => SetProperty(ref selectedReceiptDetail, value);
        }

        public bool HasMoreThanThreePatientBillingRecords =>
            SelectedPatientBillingRecords.Count > 3;

        public string PaymentHistorySearchText
        {
            get => paymentHistorySearchText;
            set
            {
                if (SetProperty(ref paymentHistorySearchText, value))
                {
                    SearchPatientsForPaymentHistory();

                    IsPaymentHistorySearchPopupOpen =
                        !string.IsNullOrWhiteSpace(paymentHistorySearchText)
                        && PaymentHistoryPatientResults.Count > 0;

                    System.Diagnostics.Debug.WriteLine(
                        $"Payment history search: '{paymentHistorySearchText}' | Results: {PaymentHistoryPatientResults.Count} | Popup: {IsPaymentHistorySearchPopupOpen}");
                }
            }
        }

        public bool IsPaymentHistorySearchPopupOpen
        {
            get => isPaymentHistorySearchPopupOpen;
            set => SetProperty(ref isPaymentHistorySearchPopupOpen, value);
        }

        public BillingPatientLookupItem? SelectedPaymentHistoryPatient
        {
            get => selectedPaymentHistoryPatient;
            set
            {
                if (SetProperty(ref selectedPaymentHistoryPatient, value))
                {
                    if (value == null)
                        return;

                    paymentHistorySearchText = $"{value.PatientCode} - {value.PatientName}";
                    OnPropertyChanged(nameof(PaymentHistorySearchText));

                    PaymentHistoryPatientName = value.PatientName;
                    HasSelectedPaymentHistoryPatient = true;
                    IsPaymentHistorySearchPopupOpen = false;

                    LoadSelectedPatientBillingRecords(value.PatientId);
                }
            }
        }

        public string PaymentHistoryPatientName
        {
            get => paymentHistoryPatientName;
            set => SetProperty(ref paymentHistoryPatientName, value);
        }

        public bool HasSelectedPaymentHistoryPatient
        {
            get => hasSelectedPaymentHistoryPatient;
            set => SetProperty(ref hasSelectedPaymentHistoryPatient, value);
        }

        public string SearchText
        {
            get => searchText;
            set => SetProperty(ref searchText, value);
        }

        public bool IsPromptOpen
        {
            get => isPromptOpen;
            set => SetProperty(ref isPromptOpen, value);
        }

        public string PromptTitle
        {
            get => promptTitle;
            set => SetProperty(ref promptTitle, value);
        }

        public string PromptMessage
        {
            get => promptMessage;
            set => SetProperty(ref promptMessage, value);
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
                    if (value != null)
                    {
                        appointmentPaymentSearchText =
                            $"{value.PatientCode} - {value.PatientName}";

                        OnPropertyChanged(nameof(AppointmentPaymentSearchText));
                        IsAppointmentPaymentSearchPopupOpen = false;
                    }

                    FillAppointmentPaymentForm(value);
                }
            }
        }

        public string AppointmentPaymentSearchText
        {
            get => appointmentPaymentSearchText;
            set
            {
                if (SetProperty(ref appointmentPaymentSearchText, value))
                {
                    FilterAppointmentPaymentItems();

                    IsAppointmentPaymentSearchPopupOpen =
                        !string.IsNullOrWhiteSpace(value)
                        && FilteredAppointmentPaymentItems.Count > 0;
                }
            }
        }

        public bool IsAppointmentPaymentSearchPopupOpen
        {
            get => isAppointmentPaymentSearchPopupOpen;
            set => SetProperty(ref isAppointmentPaymentSearchPopupOpen, value);
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
            set
            {
                if (SetProperty(ref appointmentCategory, value))
                {
                    RecalculateAppointmentPayment();
                }
            }
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
            set => SetProperty(ref appointmentDiscountType, value);
        }

        public decimal AppointmentDiscountAmount
        {
            get => appointmentDiscountAmount;
            set => SetProperty(ref appointmentDiscountAmount, value);
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

        public string AppointmentVatExemptSalesDisplay
        {
            get
            {
                if (!IsAppointmentPatientDiscountEligible())
                    return "N/A";

                decimal vatExemptSales = CalculateVatExemptSales(AppointmentTotalAmount);
                return $"₱{vatExemptSales:N2}";
            }
        }

        public string AppointmentDiscountLabel =>
            IsAppointmentPatientDiscountEligible()
                ? "20% Senior/PWD Discount:"
                : "Discount:";

        public string AppointmentBillableAmountDisplay =>
            $"₱{CalculateAppointmentBillableAmount():N2}";

        public string AppointmentSubtotalDisplay =>
            AppointmentBillableAmountDisplay;

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

        #region Balance Payment Properties

        public BalancePaymentItem? SelectedBalancePaymentItem
        {
            get => selectedBalancePaymentItem;
            set
            {
                if (SetProperty(ref selectedBalancePaymentItem, value))
                    FillBalancePaymentForm(value);
            }
        }

        public string BalanceSearchText
        {
            get => balanceSearchText;
            set
            {
                if (SetProperty(ref balanceSearchText, value))
                    FilterBalancePaymentItems();
            }
        }

        public string BalancePaymentDateDisplay =>
            DateTime.Today.ToString("MM/dd/yyyy");

        public string BalanceReceiptNumber
        {
            get => balanceReceiptNumber;
            set => SetProperty(ref balanceReceiptNumber, value);
        }

        public string BalancePatientName
        {
            get => balancePatientName;
            set => SetProperty(ref balancePatientName, value);
        }

        public string BalanceServiceName
        {
            get => balanceServiceName;
            set => SetProperty(ref balanceServiceName, value);
        }

        public decimal BalanceCurrentBalance
        {
            get => balanceCurrentBalance;
            set
            {
                if (SetProperty(ref balanceCurrentBalance, value))
                    OnPropertyChanged(nameof(BalanceCurrentBalanceDisplay));
            }
        }

        public decimal BalancePaymentAmount
        {
            get => balancePaymentAmount;
            set
            {
                if (SetProperty(ref balancePaymentAmount, value))
                    RecalculateBalancePayment();
            }
        }

        public decimal BalanceRemainingAfterPayment
        {
            get => balanceRemainingAfterPayment;
            set
            {
                if (SetProperty(ref balanceRemainingAfterPayment, value))
                    OnPropertyChanged(nameof(BalanceRemainingAfterPaymentDisplay));
            }
        }

        public string BalancePaymentNotes
        {
            get => balancePaymentNotes;
            set => SetProperty(ref balancePaymentNotes, value);
        }

        public string BalanceCurrentBalanceDisplay =>
            $"₱{BalanceCurrentBalance:N2}";

        public string BalancePaymentAmountDisplay =>
            $"₱{BalancePaymentAmount:N2}";

        public string BalanceRemainingAfterPaymentDisplay =>
            $"₱{BalanceRemainingAfterPayment:N2}";

        private bool showAllBalancePaymentItems;

        public bool ShowAllBalancePaymentItems
        {
            get => showAllBalancePaymentItems;
            set
            {
                if (SetProperty(ref showAllBalancePaymentItems, value))
                {
                    OnPropertyChanged(nameof(HasMoreBalancePaymentItems));
                    OnPropertyChanged(nameof(ViewMoreBalancePaymentText));
                }
            }
        }

        public ObservableCollection<BalancePaymentItem> VisibleBalancePaymentItems { get; }

        public bool HasMoreBalancePaymentItems =>
            FilteredBalancePaymentItems.Count > 3;

        public string ViewMoreBalancePaymentText =>
            ShowAllBalancePaymentItems ? "Show Less" : "View More";
        #endregion

        #region Commands

        public ICommand SelectBillingModuleCommand { get; }

        public ICommand RefreshBillingCommand { get; }

        public ICommand ViewReceiptCommand { get; }
        public ICommand PrintReceiptCommand { get; }

        public ICommand PrintReceiptPdfCommand { get; }
        public ICommand SaveReceiptPdfCommand { get; }

        public ICommand CloseReceiptPrintPreviewCommand { get; }

        public ICommand ProcessAppointmentPaymentCommand { get; }

        public ICommand ClearAppointmentPaymentFormCommand { get; }

        public ICommand ClosePromptCommand { get; }

        public ICommand OpenTransactionHistoryOverlayCommand { get; }

        public ICommand CloseTransactionHistoryOverlayCommand { get; }

        public ICommand ExpandReceiptCommand { get; }

        public ICommand CloseReceiptOverlayCommand { get; }

        // Balance Payment Commands
        public ICommand SelectBalancePaymentCommand { get; }

        public ICommand ProcessBalancePaymentCommand { get; }

        public ICommand ClearBalancePaymentFormCommand { get; }

        public ICommand ToggleBalancePaymentItemsCommand { get; }
        #endregion

        #region Constructor

        public BillingViewModel()
        {
            billingRepository = new BillingRepository();

            BillingRecords = new ObservableCollection<BillingRecordListItem>();
            AppointmentPaymentItems = new ObservableCollection<AppointmentPaymentItem>();
            FilteredAppointmentPaymentItems = new ObservableCollection<AppointmentPaymentItem>();
            VisibleBalancePaymentItems = new ObservableCollection<BalancePaymentItem>();

            ToggleBalancePaymentItemsCommand = new RelayCommand(() =>
            {
                ShowAllBalancePaymentItems = !ShowAllBalancePaymentItems;
                RefreshVisibleBalancePaymentItems();
            });

            BalancePaymentItems = new ObservableCollection<BalancePaymentItem>();
            FilteredBalancePaymentItems = new ObservableCollection<BalancePaymentItem>();

            SelectBillingModuleCommand = new RelayCommand<string>(SelectBillingModule);
            RefreshBillingCommand = new RelayCommand(LoadBillingData);

            ViewReceiptCommand = new RelayCommand<BillingRecordListItem>(ViewReceipt);
            SaveReceiptPdfCommand = new RelayCommand<BillingReceiptDetail>(SaveReceiptPdf);
            PrintReceiptPdfCommand = new RelayCommand<BillingReceiptDetail>(PrintReceiptPdf);
            PrintReceiptCommand = new RelayCommand<BillingRecordListItem>(PrintReceipt);


            LatestPatientBillingRecords = new ObservableCollection<BillingRecordListItem>();

            PaymentHistoryPatientResults = new ObservableCollection<BillingPatientLookupItem>();
            SelectedPatientBillingRecords = new ObservableCollection<BillingRecordListItem>();

            CloseReceiptPrintPreviewCommand = new RelayCommand(CloseReceiptPrintPreview);

            OpenTransactionHistoryOverlayCommand = new RelayCommand(OpenTransactionHistoryOverlay);
            CloseTransactionHistoryOverlayCommand = new RelayCommand(CloseTransactionHistoryOverlay);

            ExpandReceiptCommand = new RelayCommand<BillingRecordListItem>(ExpandReceipt);
            CloseReceiptOverlayCommand = new RelayCommand(CloseReceiptOverlay);

            ProcessAppointmentPaymentCommand = new RelayCommand(ProcessAppointmentPayment);
            ClearAppointmentPaymentFormCommand = new RelayCommand(ClearAppointmentPaymentForm);

            SelectBalancePaymentCommand = new RelayCommand<BalancePaymentItem>(SelectBalancePayment);
            ProcessBalancePaymentCommand = new RelayCommand(ProcessBalancePayment);
            ClearBalancePaymentFormCommand = new RelayCommand(ClearBalancePaymentForm);


            ClosePromptCommand = new RelayCommand(ClosePrompt);

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

        private void SearchPatientsForPaymentHistory()
        {
            PaymentHistoryPatientResults.Clear();

            string keyword = PaymentHistorySearchText.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ClearPaymentHistorySelection();
                return;
            }

            try
            {
                foreach (BillingPatientLookupItem patient in billingRepository.SearchPatientsForBillingHistory(keyword))
                {
                    PaymentHistoryPatientResults.Add(patient);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Payment history patient search failed: {ex.Message}");
                ShowPrompt("Payment History", $"Failed to search patients: {ex.Message}");
            }
        }

        private void LoadSelectedPatientBillingRecords(int patientId)
        {
            SelectedPatientBillingRecords.Clear();
            LatestPatientBillingRecords.Clear();

            foreach (BillingRecordListItem billing in billingRepository.GetBillingRecordsByPatientId(patientId))
                SelectedPatientBillingRecords.Add(billing);

            foreach (BillingRecordListItem billing in SelectedPatientBillingRecords.Take(3))
                LatestPatientBillingRecords.Add(billing);

            OnPropertyChanged(nameof(HasMoreThanThreePatientBillingRecords));
        }

        private void ClearPaymentHistorySelection()
        {
            selectedPaymentHistoryPatient = null;
            OnPropertyChanged(nameof(SelectedPaymentHistoryPatient));

            PaymentHistoryPatientName = string.Empty;
            HasSelectedPaymentHistoryPatient = false;
            IsPaymentHistorySearchPopupOpen = false;

            SelectedPatientBillingRecords.Clear();
            LatestPatientBillingRecords.Clear();

            OnPropertyChanged(nameof(HasMoreThanThreePatientBillingRecords));
        }

        private void LoadAppointmentPaymentItems()
        {
            AppointmentPaymentItems.Clear();

            foreach (AppointmentPaymentItem item in billingRepository.GetUnbilledCompletedTreatments())
                AppointmentPaymentItems.Add(item);

            FilterAppointmentPaymentItems();
        }
        private void LoadBalancePaymentItems()
        {
            BalancePaymentItems.Clear();

            foreach (BalancePaymentItem item in billingRepository.GetBillingsWithBalance())
                BalancePaymentItems.Add(item);
            
            FilterBalancePaymentItems();
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

        // This method is currently used for the "View Receipt" button (Action Section)
        private void ViewReceipt(BillingRecordListItem? billing)
        {
            if (billing == null)
                return;
            
            ExpandReceipt(billing);
        }

        // This method is currently used for the "Print Receipt" button (Action Section)
        // It prepares the receipt details and opens the print preview
        private void PrintReceipt(BillingRecordListItem? billing)
        {
            if (billing == null)
                return;

            try
            {
                BillingReceiptDetail? detail =
                    billingRepository.GetBillingReceiptDetail(billing.BillingId);

                if (detail == null)
                {
                    ShowPrompt("Print Receipt", "Unable to load the selected receipt.");
                    return;
                }

                PrintReceiptPdf(detail);
            }
            catch (Exception ex)
            {
                ShowPrompt("Print Receipt", $"Failed to prepare receipt for printing: {ex.Message}");
            }
        }

        private void SaveReceiptPdf(BillingReceiptDetail? receipt)
        {
            if (receipt == null)
            {
                ShowPrompt("Receipt", "No receipt is selected.");
                return;
            }

            try
            {
                string filePath = ReceiptPDFService.GenerateReceiptPdf(receipt);
                ReceiptPDFService.OpenPdf(filePath);

                ShowPrompt("Receipt", "PDF receipt has been generated and opened.");
            }
            catch (Exception ex)
            {
                ShowPrompt("Receipt", $"Failed to generate PDF receipt: {ex.Message}");
            }
        }

        private void PrintReceiptPdf(BillingReceiptDetail? receipt)
        {
            if (receipt == null)
            {
                ShowPrompt("Receipt", "No receipt is selected.");
                return;
            }

            try
            {
                string filePath = ReceiptPDFService.GenerateReceiptPdf(receipt);

                ReceiptPrintPreviewUri = new Uri(filePath);
                IsReceiptPrintPreviewOpen = true;
            }
            catch (Exception ex)
            {
                ShowPrompt("Print Receipt", $"Failed to prepare receipt preview: {ex.Message}");
            }
        }

        private void OpenTransactionHistoryOverlay()
        {
            if (!HasSelectedPaymentHistoryPatient)
            {
                ShowPrompt("Payment History", "Please select a patient first.");
                return;
            }

            IsTransactionHistoryOverlayOpen = true;
        }

        private void CloseTransactionHistoryOverlay()
        {
            IsTransactionHistoryOverlayOpen = false;
        }

        private void ExpandReceipt(BillingRecordListItem? record)
        {
            if (record == null)
                return;

            try
            {
                BillingReceiptDetail? detail =
                    billingRepository.GetBillingReceiptDetail(record.BillingId);

                if (detail == null)
                {
                    ShowPrompt("Receipt", "Unable to load the selected receipt.");
                    return;
                }

                SelectedReceiptDetail = detail;
                IsReceiptOverlayOpen = true;
            }
            catch (Exception ex)
            {
                ShowPrompt("Receipt", $"Failed to load receipt details: {ex.Message}");
            }
        }

        private void CloseReceiptPrintPreview()
        {
            IsReceiptPrintPreviewOpen = false;
        }

        private void CloseReceiptOverlay()
        {
            IsReceiptOverlayOpen = false;
            SelectedReceiptDetail = null;
        }

        #endregion

        #region Appointment Payment Methods

        private bool IsAppointmentPatientDiscountEligible()
        {
            return AppointmentCategory == "PWD"
                || AppointmentCategory == "Senior Citizen"
                || AppointmentCategory == "PWD / Senior";
        }

        private decimal CalculateVatExemptSales(decimal grossAmount)
        {
            return Math.Round(grossAmount / 1.12m, 2);
        }

        private decimal CalculateSeniorPwdDiscount(decimal vatExemptSales)
        {
            return Math.Round(vatExemptSales * 0.20m, 2);
        }

        private decimal CalculateAppointmentBillableAmount()
        {
            if (AppointmentTotalAmount <= 0)
                return 0;

            if (!IsAppointmentPatientDiscountEligible())
                return AppointmentTotalAmount;

            decimal vatExemptSales = CalculateVatExemptSales(AppointmentTotalAmount);
            decimal discount = CalculateSeniorPwdDiscount(vatExemptSales);

            return Math.Round(vatExemptSales - discount, 2);
        }

        private void FilterAppointmentPaymentItems()
        {
            FilteredAppointmentPaymentItems.Clear();

            string keyword = AppointmentPaymentSearchText.Trim();

            var results = AppointmentPaymentItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                results = results.Where(item =>
                    item.PatientCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || item.PatientName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || item.ServiceName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || item.TreatmentDateDisplay.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            foreach (AppointmentPaymentItem item in results.Take(8))
                FilteredAppointmentPaymentItems.Add(item);
        }

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
            AppointmentCategory = item.Category;
            AppointmentDate = item.TreatmentDateDisplay;
            AppointmentServiceName = item.ServiceName;

            AppointmentTotalAmount = 0;
            AppointmentDiscountType = "None";
            AppointmentDiscountAmount = 0;
            AppointmentPaymentAmount = 0;
            AppointmentPaymentMethod = "Cash";
            AppointmentNotes = string.Empty;

            RecalculateAppointmentPayment();
        }

        private void RecalculateAppointmentPayment()
        {
            if (AppointmentTotalAmount < 0)
                AppointmentTotalAmount = 0;

            if (AppointmentPaymentAmount < 0)
                AppointmentPaymentAmount = 0;

            if (IsAppointmentPatientDiscountEligible())
            {
                decimal vatExemptSales = CalculateVatExemptSales(AppointmentTotalAmount);

                appointmentDiscountAmount = CalculateSeniorPwdDiscount(vatExemptSales);
                OnPropertyChanged(nameof(AppointmentDiscountAmount));

                appointmentDiscountType = AppointmentCategory == "PWD / Senior"
                    ? "PWD/Senior"
                    : AppointmentCategory;

                OnPropertyChanged(nameof(AppointmentDiscountType));
            }
            else
            {
                appointmentDiscountAmount = 0;
                OnPropertyChanged(nameof(AppointmentDiscountAmount));

                appointmentDiscountType = "None";
                OnPropertyChanged(nameof(AppointmentDiscountType));
            }

            decimal billableAmount = CalculateAppointmentBillableAmount();

            if (AppointmentPaymentAmount > billableAmount)
                AppointmentPaymentAmount = billableAmount;

            AppointmentBalance = billingRepository.CalculateBalance(billableAmount, AppointmentPaymentAmount);

            OnPropertyChanged(nameof(AppointmentVatExemptSalesDisplay));
            OnPropertyChanged(nameof(AppointmentDiscountLabel));
            OnPropertyChanged(nameof(AppointmentBillableAmountDisplay));
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

            decimal netAmount = CalculateAppointmentBillableAmount();

            if (AppointmentPaymentAmount < 0)
            {
                ShowAppointmentPaymentError("Payment amount cannot be negative.");
                return;
            }

            if (AppointmentPaymentAmount > netAmount)
            {
                ShowAppointmentPaymentError("Payment amount cannot be greater than the billable amount.");
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
            appointmentPaymentSearchText = string.Empty;
            OnPropertyChanged(nameof(AppointmentPaymentSearchText));
            IsAppointmentPaymentSearchPopupOpen = false;
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

            OnPropertyChanged(nameof(AppointmentVatExemptSalesDisplay));
            OnPropertyChanged(nameof(AppointmentDiscountLabel));
            OnPropertyChanged(nameof(AppointmentBillableAmountDisplay));
            OnPropertyChanged(nameof(AppointmentSubtotalDisplay));
            OnPropertyChanged(nameof(AppointmentBalanceDisplay));
        }

        private void ShowAppointmentPaymentError(string message)
        {
            AppointmentPaymentErrorMessage = message;
            HasAppointmentPaymentError = true;

            ShowPrompt("Appointment Payment", message);
        }

        private void ClearAppointmentPaymentError()
        {
            AppointmentPaymentErrorMessage = string.Empty;
            HasAppointmentPaymentError = false;
        }

        #endregion

        #region Balance Payment Methods
        private void FilterBalancePaymentItems()
        {
            FilteredBalancePaymentItems.Clear();

            string keyword = BalanceSearchText.Trim();

            IEnumerable<BalancePaymentItem> results = BalancePaymentItems;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                results = results.Where(item =>
                    item.ReceiptNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || item.PatientCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || item.PatientName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || item.ServiceName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || item.PaymentStatus.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            foreach (BalancePaymentItem item in results)
                FilteredBalancePaymentItems.Add(item);

            // When searching, collapse back to the first 3 matching results.
            ShowAllBalancePaymentItems = false;

            RefreshVisibleBalancePaymentItems();
        }

        private void RefreshVisibleBalancePaymentItems()
        {
            VisibleBalancePaymentItems.Clear();

            IEnumerable<BalancePaymentItem> visibleItems = ShowAllBalancePaymentItems
                ? FilteredBalancePaymentItems
                : FilteredBalancePaymentItems.Take(3);

            foreach (BalancePaymentItem item in visibleItems)
                VisibleBalancePaymentItems.Add(item);

            OnPropertyChanged(nameof(HasMoreBalancePaymentItems));
            OnPropertyChanged(nameof(ViewMoreBalancePaymentText));
        }

        private void SelectBalancePayment(BalancePaymentItem? item)
        {
            if (item == null)
                return;

            SelectedBalancePaymentItem = item;
        }

        private void FillBalancePaymentForm(BalancePaymentItem? item)
        {
            if (item == null)
            {
                ClearBalancePaymentFormFieldsOnly();
                return;
            }

            BalanceReceiptNumber = item.ReceiptNumber;
            BalancePatientName = item.PatientName;
            BalanceServiceName = item.ServiceName;
            BalanceCurrentBalance = item.RemainingBalance;

            BalancePaymentAmount = 0;
            BalancePaymentNotes = string.Empty;

            OnPropertyChanged(nameof(BalancePaymentDateDisplay));

            RecalculateBalancePayment();
        }

        private void RecalculateBalancePayment()
        {
            if (BalancePaymentAmount < 0)
                BalancePaymentAmount = 0;

            if (BalancePaymentAmount > BalanceCurrentBalance)
                BalancePaymentAmount = BalanceCurrentBalance;

            BalanceRemainingAfterPayment = Math.Max(BalanceCurrentBalance - BalancePaymentAmount, 0);

            OnPropertyChanged(nameof(BalanceCurrentBalanceDisplay));
            OnPropertyChanged(nameof(BalancePaymentAmountDisplay));
            OnPropertyChanged(nameof(BalanceRemainingAfterPaymentDisplay));
        }

        private void ProcessBalancePayment()
        {
            if (SelectedBalancePaymentItem == null)
            {
                ShowPrompt("Balance Payment", "Please select a billing record with existing balance.");
                return;
            }

            if (BalancePaymentAmount <= 0)
            {
                ShowPrompt("Balance Payment", "Please enter a valid payment amount.");
                return;
            }

            if (BalancePaymentAmount > BalanceCurrentBalance)
            {
                ShowPrompt("Balance Payment", "Payment amount cannot be greater than the current balance.");
                return;
            }

            try
            {
                PaymentRecord payment = new()
                {
                    BillingId = SelectedBalancePaymentItem.BillingId,
                    PatientId = SelectedBalancePaymentItem.PatientId,
                    AmountPaid = BalancePaymentAmount,
                    PaymentMethod = "Cash",
                    PaymentDate = DateTime.Today,
                    ReceivedByUserId = null,
                    Notes = BalancePaymentNotes
                };

                billingRepository.AddPaymentRecord(payment);

                LoadBillingData();
                ClearBalancePaymentForm();

                SelectedBillingModule = "Balance Payment";

                ShowPrompt("Balance Payment", "Payment has been recorded successfully.");
            }
            catch (Exception ex)
            {
                ShowPrompt("Balance Payment", $"Failed to record balance payment: {ex.Message}");
            }
        }

        private void ClearBalancePaymentForm()
        {
            SelectedBalancePaymentItem = null;
            ClearBalancePaymentFormFieldsOnly();
        }

        private void ClearBalancePaymentFormFieldsOnly()
        {
            BalanceReceiptNumber = string.Empty;
            BalancePatientName = string.Empty;
            BalanceServiceName = string.Empty;

            BalanceCurrentBalance = 0;
            BalancePaymentAmount = 0;
            BalanceRemainingAfterPayment = 0;
            BalancePaymentNotes = string.Empty;

            OnPropertyChanged(nameof(BalancePaymentDateDisplay));
            OnPropertyChanged(nameof(BalanceCurrentBalanceDisplay));
            OnPropertyChanged(nameof(BalancePaymentAmountDisplay));
            OnPropertyChanged(nameof(BalanceRemainingAfterPaymentDisplay));
        }

        #endregion

        #region Error Helpers

        private void ShowPrompt(string title, string message)
        {
            PromptTitle = title;
            PromptMessage = message;
            IsPromptOpen = true;
        }

        private void ClosePrompt()
        {
            IsPromptOpen = false;
            PromptTitle = string.Empty;
            PromptMessage = string.Empty;
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;

            ShowPrompt("Billing Notice", message);
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }

        #endregion
    }
}