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

        private BillingRecordListItem? selectedOpenInvoice;
        private int? appointmentInvoicePatientId;
        private int? selectedInvoiceIdForPayment;

        private string newInvoiceTitle = string.Empty;
        private bool isCreatingNewInvoice;

        private string selectedBillingStatusFilter = "All";

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

        private int unpaidCount;
        private int partialCount;
        private int paidTodayCount;
        private decimal collectedToday;

        private string recentSearchText = string.Empty;

        private bool isInvoicePaymentOverlayOpen;
        private bool isBalancePaymentOverlayOpen;
        private bool isManualTransactionOverlayOpen;

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

        #region Backing Fields for Manual Transaction Module    

        private BillingPatientLookupItem? selectedManualPatient;

        private string manualPatientSearchText = string.Empty;
        private string manualReceiptNumber = string.Empty;
        private string manualPatientCode = string.Empty;
        private string manualPatientName = string.Empty;
        private string manualCategory = string.Empty;
        private string manualServiceName = string.Empty;
        private string manualDescription = string.Empty;

        private decimal manualTotalAmount;
        private string manualDiscountType = "None";
        private decimal manualVatExemptSales;
        private decimal manualDiscountAmount;
        private decimal manualPaymentAmount;
        private decimal manualBalance;

        private string manualNotes = string.Empty;

        #endregion

        #region Collections
        public ObservableCollection<string> BillingStatusFilterOptions { get; }
        public ObservableCollection<BillingRecordListItem> BillingRecords { get; }

        public ObservableCollection<AppointmentPaymentItem> AppointmentPaymentItems { get; }
        public ObservableCollection<AppointmentPaymentItem> FilteredAppointmentPaymentItems { get; }

        public ObservableCollection<BalancePaymentItem> BalancePaymentItems { get; }
        public ObservableCollection<BalancePaymentItem> FilteredBalancePaymentItems { get; }

        public ObservableCollection<BillingRecordListItem> PatientOpenInvoices { get; }

        public ObservableCollection<BillingTransactionItem> SelectedInvoiceItems { get; }

        public ObservableCollection<BillingPatientLookupItem> PaymentHistoryPatientResults { get; }

        public ObservableCollection<BillingRecordListItem> SelectedPatientBillingRecords { get; }

        public ObservableCollection<BillingRecordListItem> LatestPatientBillingRecords { get; }

        public ObservableCollection<BillingPatientLookupItem> ManualPatientLookupResults { get; }

        public ObservableCollection<string> ManualServiceOptions { get; }

        #endregion

        #region Properties

        public BillingRecordListItem? SelectedOpenInvoice
        {
            get => selectedOpenInvoice;
            set
            {
                if (SetProperty(ref selectedOpenInvoice, value))
                {
                    if (value != null)
                    {
                        selectedInvoiceIdForPayment = value.BillingId;
                        appointmentInvoicePatientId = value.PatientId;
                        AppointmentReceiptNumber = value.ReceiptNumber;
                        AppointmentBalance = value.RemainingBalance;
                    }
                    else
                    {
                        selectedInvoiceIdForPayment = null;
                        AppointmentReceiptNumber = string.Empty;
                    }

                    LoadSelectedInvoiceItems();
                    NotifySelectedInvoiceDisplayChanged();

                    RecalculateAppointmentPayment();
                }
            }
        }     
        
        public string NewInvoiceTitle
        {
            get => newInvoiceTitle;
            set => SetProperty(ref newInvoiceTitle, value);
        }

        public bool IsCreatingNewInvoice
        {
            get => isCreatingNewInvoice;
            set => SetProperty(ref isCreatingNewInvoice, value);
        }

        public bool HasSelectedOpenInvoice =>
            SelectedOpenInvoice != null;

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

        public string SelectedBillingStatusFilter
        {
            get => selectedBillingStatusFilter;
            set
            {
                if (SetProperty(ref selectedBillingStatusFilter, value))
                    LoadBillingRecords();
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
                "Balance Payment" => "Record payments for existing unpaid or partial invoices.",
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

        public string RecentSearchText
        {
            get => recentSearchText;
            set
            {
                if (SetProperty(ref recentSearchText, value))
                    LoadBillingRecords();
            }
        }

        public int UnpaidCount
        {
            get => unpaidCount;
            set => SetProperty(ref unpaidCount, value);
        }

        public int PartialCount
        {
            get => partialCount;
            set => SetProperty(ref partialCount, value);
        }

        public int PaidTodayCount
        {
            get => paidTodayCount;
            set => SetProperty(ref paidTodayCount, value);
        }

        public decimal CollectedToday
        {
            get => collectedToday;
            set
            {
                if (SetProperty(ref collectedToday, value))
                    OnPropertyChanged(nameof(CollectedTodayDisplay));
            }
        }

        public string CollectedTodayDisplay =>
            $"₱{CollectedToday:N2}";

        public bool IsInvoicePaymentOverlayOpen
        {
            get => isInvoicePaymentOverlayOpen;
            set => SetProperty(ref isInvoicePaymentOverlayOpen, value);
        }

        public bool IsBalancePaymentOverlayOpen
        {
            get => isBalancePaymentOverlayOpen;
            set => SetProperty(ref isBalancePaymentOverlayOpen, value);
        }

        public bool IsManualTransactionOverlayOpen
        {
            get => isManualTransactionOverlayOpen;
            set => SetProperty(ref isManualTransactionOverlayOpen, value);
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

        private string appointmentInvoiceItemName = string.Empty;

        public string AppointmentInvoiceItemName
        {
            get => appointmentInvoiceItemName;
            set => SetProperty(ref appointmentInvoiceItemName, value);
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

        public string SelectedInvoiceNumberDisplay =>
            SelectedOpenInvoice?.ReceiptNumber ?? "No invoice selected";

        public string SelectedInvoiceTitleDisplay =>
            SelectedOpenInvoice?.ServiceName ?? "No invoice selected";

        public string SelectedInvoiceTotalDisplay =>
            SelectedOpenInvoice == null
                ? "₱0.00"
                : $"₱{SelectedOpenInvoice.TotalAmount:N2}";

        public string SelectedInvoicePaidDisplay =>
            SelectedOpenInvoice == null
                ? "₱0.00"
                : $"₱{SelectedOpenInvoice.AmountPaid:N2}";

        public string SelectedInvoiceBalanceDisplay =>
            SelectedOpenInvoice == null
                ? "₱0.00"
                : $"₱{SelectedOpenInvoice.RemainingBalance:N2}";
        #endregion

        #region Manual Transaction Properties

        public BillingPatientLookupItem? SelectedManualPatient
        {
            get => selectedManualPatient;
            set
            {
                if (SetProperty(ref selectedManualPatient, value))
                    FillManualPatient(value);
            }
        }

        public string ManualPatientSearchText
        {
            get => manualPatientSearchText;
            set
            {
                if (SetProperty(ref manualPatientSearchText, value))
                    SearchManualPatients();
            }
        }

        public string ManualReceiptNumber
        {
            get => manualReceiptNumber;
            set => SetProperty(ref manualReceiptNumber, value);
        }

        public string ManualPatientCode
        {
            get => manualPatientCode;
            set => SetProperty(ref manualPatientCode, value);
        }

        public string ManualPatientName
        {
            get => manualPatientName;
            set => SetProperty(ref manualPatientName, value);
        }

        public string ManualCategory
        {
            get => manualCategory;
            set => SetProperty(ref manualCategory, value);
        }

        public string ManualServiceName
        {
            get => manualServiceName;
            set => SetProperty(ref manualServiceName, value);
        }

        public string ManualDescription
        {
            get => manualDescription;
            set => SetProperty(ref manualDescription, value);
        }

        public decimal ManualTotalAmount
        {
            get => manualTotalAmount;
            set
            {
                if (SetProperty(ref manualTotalAmount, value))
                    RecalculateManualTransaction();
            }
        }

        public string ManualDiscountType
        {
            get => manualDiscountType;
            set
            {
                if (SetProperty(ref manualDiscountType, value))
                    RecalculateManualTransaction();
            }
        }

        public decimal ManualVatExemptSales
        {
            get => manualVatExemptSales;
            set
            {
                if (SetProperty(ref manualVatExemptSales, value))
                    OnPropertyChanged(nameof(ManualVatExemptSalesDisplay));
            }
        }

        public decimal ManualDiscountAmount
        {
            get => manualDiscountAmount;
            set
            {
                if (SetProperty(ref manualDiscountAmount, value))
                    OnPropertyChanged(nameof(ManualDiscountAmountDisplay));
            }
        }

        public decimal ManualPaymentAmount
        {
            get => manualPaymentAmount;
            set
            {
                if (SetProperty(ref manualPaymentAmount, value))
                    RecalculateManualTransaction();
            }
        }

        public decimal ManualBalance
        {
            get => manualBalance;
            set
            {
                if (SetProperty(ref manualBalance, value))
                    OnPropertyChanged(nameof(ManualBalanceDisplay));
            }
        }

        public string ManualNotes
        {
            get => manualNotes;
            set => SetProperty(ref manualNotes, value);
        }

        public string ManualTransactionDateDisplay =>
            DateTime.Today.ToString("MM/dd/yyyy");

        public bool HasManualVatExemption =>
            ManualDiscountType is "PWD" or "Senior Citizen" or "PWD/Senior";

        public string ManualTotalAmountDisplay =>
            $"₱{ManualTotalAmount:N2}";

        public string ManualVatExemptSalesDisplay =>
            $"₱{ManualVatExemptSales:N2}";

        public string ManualDiscountAmountDisplay =>
            $"₱{ManualDiscountAmount:N2}";

        public string ManualPaymentAmountDisplay =>
            $"₱{ManualPaymentAmount:N2}";

        public string ManualBalanceDisplay =>
            $"₱{ManualBalance:N2}";

        public string ManualSubtotalDisplay
        {
            get
            {
                decimal billableAmount = HasManualVatExemption
                    ? Math.Max(ManualVatExemptSales - ManualDiscountAmount, 0)
                    : Math.Max(ManualTotalAmount - ManualDiscountAmount, 0);

                return $"₱{billableAmount:N2}";
            }
        }

        #endregion

        #region Commands

        public ICommand SelectBillingModuleCommand { get; }

        public ICommand OpenInvoicePaymentOverlayCommand { get; }
        public ICommand OpenBalancePaymentOverlayCommand { get; }
        public ICommand OpenManualTransactionOverlayCommand { get; }
        public ICommand CloseBillingModuleOverlayCommand { get; }

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

        public ICommand CreateNewInvoiceForSelectedTreatmentCommand { get; }

        public ICommand AddSelectedTreatmentToInvoiceCommand { get; }

        public ICommand CloseReceiptOverlayCommand { get; }

        // Balance Payment Commands
        public ICommand SelectBalancePaymentCommand { get; }

        public ICommand ProcessBalancePaymentCommand { get; }

        public ICommand ClearBalancePaymentFormCommand { get; }

        public ICommand ToggleBalancePaymentItemsCommand { get; }


        // Manual Transaction Commands
        public ICommand SelectManualPatientCommand { get; }

        public ICommand ProcessManualTransactionCommand { get; }

        public ICommand ClearManualTransactionFormCommand { get; }

        #endregion

        #region Constructor

        public BillingViewModel()
        {
            billingRepository = new BillingRepository();

            BillingRecords = new ObservableCollection<BillingRecordListItem>();
            AppointmentPaymentItems = new ObservableCollection<AppointmentPaymentItem>();
            FilteredAppointmentPaymentItems = new ObservableCollection<AppointmentPaymentItem>();
            VisibleBalancePaymentItems = new ObservableCollection<BalancePaymentItem>();

            ManualPatientLookupResults = new ObservableCollection<BillingPatientLookupItem>();
            ManualServiceOptions = new ObservableCollection<string>
            {
                "Consultation",
                "Prophylaxis",
                "Restoration / Pasta",
                "Extraction",
                "Orthodontics",
                "TMJ",
                "Dentures",
                "Other"
            };

            BillingStatusFilterOptions = new ObservableCollection<string>
            {
                "All",
                "Paid",
                "Partial",
                "Unpaid"
            };

            ToggleBalancePaymentItemsCommand = new RelayCommand(() =>
            {
                ShowAllBalancePaymentItems = !ShowAllBalancePaymentItems;
                RefreshVisibleBalancePaymentItems();
            });

            BalancePaymentItems = new ObservableCollection<BalancePaymentItem>();
            FilteredBalancePaymentItems = new ObservableCollection<BalancePaymentItem>();

            SelectBillingModuleCommand = new RelayCommand<string>(SelectBillingModule);

            OpenInvoicePaymentOverlayCommand = new RelayCommand(OpenInvoicePaymentOverlay);
            OpenBalancePaymentOverlayCommand = new RelayCommand(OpenBalancePaymentOverlay);
            OpenManualTransactionOverlayCommand = new RelayCommand(OpenManualTransactionOverlay);
            CloseBillingModuleOverlayCommand = new RelayCommand(CloseBillingModuleOverlay);

            RefreshBillingCommand = new RelayCommand(LoadBillingData);

            PatientOpenInvoices = new ObservableCollection<BillingRecordListItem>();
            SelectedInvoiceItems = new ObservableCollection<BillingTransactionItem>();

            CreateNewInvoiceForSelectedTreatmentCommand = new RelayCommand(CreateNewInvoiceForSelectedTreatment);
            AddSelectedTreatmentToInvoiceCommand = new RelayCommand(AddSelectedTreatmentToInvoice);

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

            SelectManualPatientCommand = new RelayCommand<BillingPatientLookupItem>(SelectManualPatient);
            ProcessManualTransactionCommand = new RelayCommand(ProcessManualTransaction);
            ClearManualTransactionFormCommand = new RelayCommand(ClearManualTransactionForm);


            ClosePromptCommand = new RelayCommand(ClosePrompt);

            LoadBillingData();
        }

        #endregion

        #region Load Methods
        private void LoadOpenInvoicesForSelectedAppointmentPatient()
        {
            PatientOpenInvoices.Clear();

            if (SelectedAppointmentPaymentItem == null)
                return;

            try
            {
                foreach (BillingRecordListItem invoice in billingRepository.GetOpenInvoicesByPatientId(SelectedAppointmentPaymentItem.PatientId))
                    PatientOpenInvoices.Add(invoice);
            }
            catch (Exception ex)
            {
                ShowAppointmentPaymentError($"Failed to load patient invoices: {ex.Message}");
            }
        }
        private void LoadGlobalOpenInvoicesForPayment()
        {
            PatientOpenInvoices.Clear();

            try
            {
                foreach (BillingRecordListItem invoice in BillingRecords
                    .Where(invoice =>
                        !string.Equals(invoice.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase)))
                {
                    PatientOpenInvoices.Add(invoice);
                }
            }
            catch (Exception ex)
            {
                ShowAppointmentPaymentError($"Failed to load open invoices: {ex.Message}");
            }
        }
        private void LoadSelectedInvoiceItems()
        {
            SelectedInvoiceItems.Clear();

            if (SelectedOpenInvoice == null)
            {
                OnPropertyChanged(nameof(HasSelectedOpenInvoice));
                return;
            }

            try
            {
                foreach (BillingTransactionItem item in billingRepository.GetBillingTransactionItems(SelectedOpenInvoice.BillingId))
                    SelectedInvoiceItems.Add(item);

                OnPropertyChanged(nameof(HasSelectedOpenInvoice));
            }
            catch (Exception ex)
            {
                ShowAppointmentPaymentError($"Failed to load invoice items: {ex.Message}");
            }
        }
        private void LoadBillingData()
        {
            try
            {
                ClearError();

                LoadBillingRecords();
                if (IsAppointmentPaymentSelected && SelectedAppointmentPaymentItem == null)
                    LoadGlobalOpenInvoicesForPayment();
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

            List<BillingRecordListItem> allRecords = billingRepository.GetBillingRecords();

            // Summary cards are computed from the full, unfiltered list so that the
            // search box and status filter never distort the totals.
            UpdateBillingSummary(allRecords);

            IEnumerable<BillingRecordListItem> records = allRecords;

            if (!string.IsNullOrWhiteSpace(SelectedBillingStatusFilter) &&
                SelectedBillingStatusFilter != "All")
            {
                records = records.Where(record =>
                    string.Equals(
                        record.PaymentStatus,
                        SelectedBillingStatusFilter,
                        StringComparison.OrdinalIgnoreCase
                    ));
            }

            string keyword = RecentSearchText?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                records = records.Where(record =>
                    record.PatientName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || record.PatientCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || record.ReceiptNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            foreach (BillingRecordListItem item in records)
                BillingRecords.Add(item);
        }

        private void UpdateBillingSummary(IEnumerable<BillingRecordListItem> allRecords)
        {
            UnpaidCount = allRecords.Count(record =>
                string.Equals(record.PaymentStatus, "Unpaid", StringComparison.OrdinalIgnoreCase));

            PartialCount = allRecords.Count(record =>
                string.Equals(record.PaymentStatus, "Partial", StringComparison.OrdinalIgnoreCase));

            (int paidTodayCount, decimal collectedTodayTotal) = billingRepository.GetTodayCollectionSummary();

            PaidTodayCount = paidTodayCount;
            CollectedToday = collectedTodayTotal;
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

        private void OpenInvoicePaymentOverlay()
        {
            SelectedBillingModule = "Appointment Payment";
            IsInvoicePaymentOverlayOpen = true;
        }

        private void OpenBalancePaymentOverlay()
        {
            SelectedBillingModule = "Balance Payment";
            IsBalancePaymentOverlayOpen = true;
        }

        private void OpenManualTransactionOverlay()
        {
            SelectedBillingModule = "Manual Transaction";
            IsManualTransactionOverlayOpen = true;
        }

        private void CloseBillingModuleOverlay()
        {
            IsInvoicePaymentOverlayOpen = false;
            IsBalancePaymentOverlayOpen = false;
            IsManualTransactionOverlayOpen = false;
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
        private void CreateNewInvoiceForSelectedTreatment()
        {
            if (SelectedAppointmentPaymentItem == null)
            {
                ShowAppointmentPaymentError("Please select a completed treatment first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewInvoiceTitle))
            {
                ShowAppointmentPaymentError("Please enter an invoice title.");
                return;
            }

            try
            {
                string invoiceNumber = billingRepository.GenerateNextInvoiceNumber();

                BillingTransaction invoice = new()
                {
                    PatientId = SelectedAppointmentPaymentItem.PatientId,
                    AppointmentId = null,
                    TreatmentRecordId = null,
                    BillingSource = "Appointment",
                    ReceiptNumber = invoiceNumber,

                    ServiceId = null,
                    ServiceName = NewInvoiceTitle.Trim(),
                    Description = $"Invoice created for {SelectedAppointmentPaymentItem.PatientName}",

                    InvoiceTitle = NewInvoiceTitle.Trim(),
                    IsInvoiceOpen = true,

                    TotalAmount = 0,
                    DiscountType = AppointmentDiscountType,
                    DiscountAmount = 0,
                    SubtotalAfterDiscount = 0,
                    AmountPaid = 0,
                    RemainingBalance = 0,
                    PaymentStatus = "Unpaid",
                    TransactionDate = DateTime.Today,
                    CreatedByUserId = null,
                    Notes = AppointmentNotes
                };

                int billingId = billingRepository.CreateInvoiceHeader(invoice);

                ReloadSelectedInvoiceForPayment(SelectedAppointmentPaymentItem.PatientId, billingId);

                IsCreatingNewInvoice = false;

                ShowPrompt("Invoice Created", $"Invoice {invoiceNumber} was created successfully.");
            }
            catch (Exception ex)
            {
                ShowAppointmentPaymentError($"Failed to create invoice: {ex.Message}");
            }
        }

        private void NotifySelectedInvoiceDisplayChanged()
        {
            OnPropertyChanged(nameof(HasSelectedOpenInvoice));
            OnPropertyChanged(nameof(SelectedInvoiceNumberDisplay));
            OnPropertyChanged(nameof(SelectedInvoiceTitleDisplay));
            OnPropertyChanged(nameof(SelectedInvoiceTotalDisplay));
            OnPropertyChanged(nameof(SelectedInvoicePaidDisplay));
            OnPropertyChanged(nameof(SelectedInvoiceBalanceDisplay));
            OnPropertyChanged(nameof(AppointmentBalanceDisplay));
        }

        private void ReloadSelectedInvoiceForPayment(int patientId, int billingId)
        {
            PatientOpenInvoices.Clear();

            foreach (BillingRecordListItem invoice in billingRepository.GetOpenInvoicesByPatientId(patientId))
                PatientOpenInvoices.Add(invoice);

            BillingRecordListItem? invoiceToSelect = PatientOpenInvoices
                .FirstOrDefault(invoice => invoice.BillingId == billingId);

            invoiceToSelect ??= billingRepository.GetInvoiceHeaderById(billingId);

            if (invoiceToSelect != null &&
                !PatientOpenInvoices.Any(invoice => invoice.BillingId == invoiceToSelect.BillingId))
            {
                PatientOpenInvoices.Add(invoiceToSelect);
            }

            SelectedOpenInvoice = invoiceToSelect;

            if (SelectedOpenInvoice != null)
            {
                appointmentInvoicePatientId = patientId;
                selectedInvoiceIdForPayment = billingId;
                AppointmentReceiptNumber = SelectedOpenInvoice.ReceiptNumber;
                AppointmentBalance = SelectedOpenInvoice.RemainingBalance;
            }

            NotifySelectedInvoiceDisplayChanged();
        }

        private void AddSelectedTreatmentToInvoice()
        {
            ClearAppointmentPaymentError();

            if (SelectedOpenInvoice == null)
            {
                ShowAppointmentPaymentError("Please select or create an invoice first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(AppointmentInvoiceItemName))
            {
                ShowAppointmentPaymentError("Please enter an invoice item name.");
                return;
            }

            if (AppointmentTotalAmount < 0)
            {
                ShowAppointmentPaymentError("Amount cannot be negative.");
                return;
            }

            try
            {
                int selectedInvoiceId = SelectedOpenInvoice.BillingId;
                int selectedInvoicePatientId = SelectedOpenInvoice.PatientId;
                string selectedInvoiceNumber = SelectedOpenInvoice.ReceiptNumber;

                bool hasCompletedTreatment = SelectedAppointmentPaymentItem != null;

                if (hasCompletedTreatment &&
                    SelectedOpenInvoice.PatientId != SelectedAppointmentPaymentItem!.PatientId)
                {
                    ShowAppointmentPaymentError("The selected invoice belongs to a different patient. Please select an invoice for the same patient.");
                    return;
                }

                string sourceTreatmentName = hasCompletedTreatment
                    ? SelectedAppointmentPaymentItem!.ServiceName
                    : "Manual invoice item";

                string invoiceItemName = AppointmentInvoiceItemName.Trim();

                BillingTransactionItem item = new()
                {
                    BillingId = selectedInvoiceId,

                    AppointmentId = hasCompletedTreatment
                        ? SelectedAppointmentPaymentItem!.AppointmentId
                        : null,

                    TreatmentRecordId = hasCompletedTreatment
                        ? SelectedAppointmentPaymentItem!.TreatmentRecordId
                        : null,

                    ServiceId = hasCompletedTreatment
                        ? SelectedAppointmentPaymentItem!.ServiceId
                        : null,

                    ServiceName = invoiceItemName,

                    ItemDescription = string.IsNullOrWhiteSpace(AppointmentNotes)
                        ? $"Source: {sourceTreatmentName}"
                        : AppointmentNotes.Trim(),

                    TreatmentDate = hasCompletedTreatment
                        ? SelectedAppointmentPaymentItem!.TreatmentDate
                        : DateTime.Today,

                    Amount = AppointmentTotalAmount,
                    IsIncluded = AppointmentTotalAmount == 0,
                    CreatedAt = DateTime.Now
                };

                billingRepository.AddBillingTransactionItem(item);

                LoadBillingRecords();
                LoadBalancePaymentItems();

                ReloadSelectedInvoiceForPayment(selectedInvoicePatientId, selectedInvoiceId);
                LoadSelectedInvoiceItems();

                AppointmentTotalAmount = 0;
                AppointmentPaymentAmount = 0;
                AppointmentNotes = string.Empty;

                // Keep item name only if no completed treatment was selected,
                // so staff can add multiple manual invoice items faster.
                if (hasCompletedTreatment)
                    AppointmentInvoiceItemName = string.Empty;

                if (SelectedOpenInvoice != null)
                    AppointmentBalance = SelectedOpenInvoice.RemainingBalance;

                OnPropertyChanged(nameof(AppointmentBalanceDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceNumberDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceTitleDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceTotalDisplay));
                OnPropertyChanged(nameof(SelectedInvoicePaidDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceBalanceDisplay));

                ShowPrompt(
                    "Invoice Updated",
                    $"{invoiceItemName} was added to invoice {selectedInvoiceNumber}."
                );
            }
            catch (Exception ex)
            {
                ShowAppointmentPaymentError($"Failed to add item to invoice: {ex.Message}");
            }
        }
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

                // When no completed treatment is selected,
                // still show existing unpaid/partial invoices so staff can pay them.
                LoadGlobalOpenInvoicesForPayment();

                SelectedOpenInvoice = null;
                SelectedInvoiceItems.Clear();

                OnPropertyChanged(nameof(SelectedInvoiceNumberDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceTitleDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceTotalDisplay));
                OnPropertyChanged(nameof(SelectedInvoicePaidDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceBalanceDisplay));
                OnPropertyChanged(nameof(AppointmentBalanceDisplay));

                return;
            }

            AppointmentReceiptNumber = string.Empty;
            AppointmentPatientCode = item.PatientCode;
            AppointmentPatientName = item.PatientName;
            AppointmentCategory = item.Category;
            AppointmentDate = item.TreatmentDateDisplay;
            AppointmentServiceName = item.ServiceName;
            
            AppointmentInvoiceItemName = item.ServiceName;

            AppointmentTotalAmount = 0;
            AppointmentDiscountType = "None";
            AppointmentDiscountAmount = 0;
            AppointmentPaymentAmount = 0;
            AppointmentPaymentMethod = "Cash";
            AppointmentNotes = string.Empty;

            SelectedOpenInvoice = null;
            SelectedInvoiceItems.Clear();
            
            NewInvoiceTitle = $"{item.ServiceName} Invoice";
            IsCreatingNewInvoice = false;

            // When a completed treatment is selected,
            // show only that patient's open invoices so the treatment is not added to the wrong patient.
            LoadOpenInvoicesForSelectedAppointmentPatient();

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

            decimal currentInvoiceBalance;

            if (SelectedOpenInvoice != null)
                currentInvoiceBalance = SelectedOpenInvoice.RemainingBalance;
            else if (selectedInvoiceIdForPayment.HasValue && AppointmentBalance > 0)
                currentInvoiceBalance = AppointmentBalance;
            else
                currentInvoiceBalance = billableAmount;

            // Do not force the payment back to 0 unless there is truly no payable balance.
            if (AppointmentPaymentAmount > currentInvoiceBalance)
                AppointmentPaymentAmount = currentInvoiceBalance;

            AppointmentBalance = Math.Max(currentInvoiceBalance - AppointmentPaymentAmount, 0);

            OnPropertyChanged(nameof(AppointmentVatExemptSalesDisplay));
            OnPropertyChanged(nameof(AppointmentDiscountLabel));
            OnPropertyChanged(nameof(AppointmentBillableAmountDisplay));
            OnPropertyChanged(nameof(AppointmentSubtotalDisplay));
            OnPropertyChanged(nameof(AppointmentBalanceDisplay));
            OnPropertyChanged(nameof(SelectedInvoiceBalanceDisplay));
        }

        private void ProcessAppointmentPayment()
        {
            ClearAppointmentPaymentError();

            if (SelectedOpenInvoice == null)
            {
                ShowAppointmentPaymentError("Please select or create an invoice first.");
                return;
            }

            if (AppointmentPaymentAmount <= 0)
            {
                ShowAppointmentPaymentError("Please enter a valid payment amount.");
                return;
            }

            try
            {
                int selectedInvoiceId = SelectedOpenInvoice.BillingId;
                int selectedInvoicePatientId = SelectedOpenInvoice.PatientId;

                bool shouldAddItemBeforePayment =
                    !string.IsNullOrWhiteSpace(AppointmentInvoiceItemName) &&
                    AppointmentTotalAmount > 0;

                if (shouldAddItemBeforePayment)
                {
                    bool hasCompletedTreatment = SelectedAppointmentPaymentItem != null;

                    if (hasCompletedTreatment &&
                        SelectedOpenInvoice.PatientId != SelectedAppointmentPaymentItem!.PatientId)
                    {
                        ShowAppointmentPaymentError("The selected invoice belongs to a different patient. Please select an invoice for the same patient.");
                        return;
                    }

                    string sourceTreatmentName = hasCompletedTreatment
                        ? SelectedAppointmentPaymentItem!.ServiceName
                        : "Manual invoice item";

                    BillingTransactionItem item = new()
                    {
                        BillingId = selectedInvoiceId,

                        AppointmentId = hasCompletedTreatment
                            ? SelectedAppointmentPaymentItem!.AppointmentId
                            : null,

                        TreatmentRecordId = hasCompletedTreatment
                            ? SelectedAppointmentPaymentItem!.TreatmentRecordId
                            : null,

                        ServiceId = hasCompletedTreatment
                            ? SelectedAppointmentPaymentItem!.ServiceId
                            : null,

                        ServiceName = AppointmentInvoiceItemName.Trim(),

                        ItemDescription = string.IsNullOrWhiteSpace(AppointmentNotes)
                            ? $"Source: {sourceTreatmentName}"
                            : AppointmentNotes.Trim(),

                        TreatmentDate = hasCompletedTreatment
                            ? SelectedAppointmentPaymentItem!.TreatmentDate
                            : DateTime.Today,

                        Amount = AppointmentTotalAmount,
                        IsIncluded = AppointmentTotalAmount == 0,
                        CreatedAt = DateTime.Now
                    };

                    billingRepository.AddBillingTransactionItem(item);

                    LoadBillingRecords();
                    LoadBalancePaymentItems();
                    ReloadSelectedInvoiceForPayment(selectedInvoicePatientId, selectedInvoiceId);
                    LoadSelectedInvoiceItems();
                }

                if (SelectedOpenInvoice == null)
                {
                    ShowAppointmentPaymentError("Selected invoice could not be reloaded.");
                    return;
                }

                if (AppointmentPaymentAmount > SelectedOpenInvoice.RemainingBalance)
                {
                    ShowAppointmentPaymentError("Payment amount cannot be greater than the selected invoice balance.");
                    return;
                }

                PaymentRecord payment = new()
                {
                    BillingId = SelectedOpenInvoice.BillingId,
                    PatientId = SelectedOpenInvoice.PatientId,
                    AmountPaid = AppointmentPaymentAmount,
                    PaymentMethod = AppointmentPaymentMethod,
                    PaymentDate = DateTime.Today,
                    ReceivedByUserId = null,
                    Notes = AppointmentNotes
                };

                billingRepository.AddPaymentRecord(payment);

                LoadBillingRecords();
                LoadBalancePaymentItems();
                ReloadSelectedInvoiceForPayment(selectedInvoicePatientId, selectedInvoiceId);
                LoadSelectedInvoiceItems();

                AppointmentTotalAmount = 0;
                AppointmentPaymentAmount = 0;
                AppointmentNotes = string.Empty;
                AppointmentInvoiceItemName = string.Empty;

                OnPropertyChanged(nameof(AppointmentBalanceDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceNumberDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceTitleDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceTotalDisplay));
                OnPropertyChanged(nameof(SelectedInvoicePaidDisplay));
                OnPropertyChanged(nameof(SelectedInvoiceBalanceDisplay));

                ShowPrompt("Invoice Payment", "Invoice item/payment has been recorded successfully.");
            }
            catch (Exception ex)
            {
                ShowAppointmentPaymentError($"Failed to record invoice payment: {ex.Message}");
            }
        }
        private void ClearAppointmentPaymentForm()
        {
            SelectedAppointmentPaymentItem = null;
            ClearAppointmentPaymentFormFieldsOnly();
            ClearAppointmentPaymentError();

            LoadGlobalOpenInvoicesForPayment();
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

            PatientOpenInvoices.Clear();
            SelectedInvoiceItems.Clear();
            appointmentInvoicePatientId = null;
            selectedInvoiceIdForPayment = null;
            selectedOpenInvoice = null;
            OnPropertyChanged(nameof(SelectedOpenInvoice));
            NotifySelectedInvoiceDisplayChanged();

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

        #region Manual Transaction Methods

        private void SearchManualPatients()
        {
            ManualPatientLookupResults.Clear();

            if (string.IsNullOrWhiteSpace(ManualPatientSearchText))
                return;

            try
            {
                foreach (BillingPatientLookupItem patient in billingRepository.SearchPatientsForBillingHistory(ManualPatientSearchText))
                    ManualPatientLookupResults.Add(patient);
            }
            catch (Exception ex)
            {
                ShowPrompt("Manual Transaction", $"Failed to search patients: {ex.Message}");
            }
        }

        private void SelectManualPatient(BillingPatientLookupItem? patient)
        {
            if (patient == null)
                return;

            SelectedManualPatient = patient;
        }

        private void FillManualPatient(BillingPatientLookupItem? patient)
        {
            if (patient == null)
            {
                ClearManualPatientFieldsOnly();
                return;
            }

            ManualReceiptNumber = string.Empty;
            ManualPatientCode = patient.PatientCode;
            ManualPatientName = patient.PatientName;
            ManualCategory = patient.Category;

            ManualDiscountType = patient.Category switch
            {
                "PWD" => "PWD",
                "Senior Citizen" => "Senior Citizen",
                "PWD / Senior" => "PWD/Senior",
                "PWD/Senior" => "PWD/Senior",
                _ => "None"
            };

            ManualPatientSearchText = patient.PatientCode;
            ManualPatientLookupResults.Clear();

            OnPropertyChanged(nameof(ManualTransactionDateDisplay));

            RecalculateManualTransaction();
        }

        private void RecalculateManualTransaction()
        {
            if (ManualTotalAmount < 0)
                ManualTotalAmount = 0;

            if (ManualPaymentAmount < 0)
                ManualPaymentAmount = 0;

            if (HasManualVatExemption && ManualTotalAmount > 0)
            {
                ManualVatExemptSales = Math.Round(ManualTotalAmount / 1.12m, 2);
                ManualDiscountAmount = Math.Round(ManualVatExemptSales * 0.20m, 2);
            }
            else
            {
                ManualVatExemptSales = 0;
                ManualDiscountAmount = 0;
            }

            decimal billableAmount = HasManualVatExemption
                ? Math.Max(ManualVatExemptSales - ManualDiscountAmount, 0)
                : Math.Max(ManualTotalAmount - ManualDiscountAmount, 0);

            if (ManualPaymentAmount > billableAmount)
                ManualPaymentAmount = billableAmount;

            ManualBalance = Math.Max(billableAmount - ManualPaymentAmount, 0);

            OnPropertyChanged(nameof(HasManualVatExemption));
            OnPropertyChanged(nameof(ManualTotalAmountDisplay));
            OnPropertyChanged(nameof(ManualVatExemptSalesDisplay));
            OnPropertyChanged(nameof(ManualDiscountAmountDisplay));
            OnPropertyChanged(nameof(ManualPaymentAmountDisplay));
            OnPropertyChanged(nameof(ManualBalanceDisplay));
            OnPropertyChanged(nameof(ManualSubtotalDisplay));
        }

        private void ProcessManualTransaction()
        {
            if (SelectedManualPatient == null)
            {
                ShowPrompt("Manual Transaction", "Please select a patient first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ManualServiceName))
            {
                ShowPrompt("Manual Transaction", "Please enter or select a service/treatment.");
                return;
            }

            if (ManualTotalAmount <= 0)
            {
                ShowPrompt("Manual Transaction", "Please enter a valid total amount.");
                return;
            }

            decimal billableAmount = HasManualVatExemption
                ? Math.Max(ManualVatExemptSales - ManualDiscountAmount, 0)
                : Math.Max(ManualTotalAmount - ManualDiscountAmount, 0);

            if (ManualPaymentAmount < 0)
            {
                ShowPrompt("Manual Transaction", "Payment amount cannot be negative.");
                return;
            }

            if (ManualPaymentAmount > billableAmount)
            {
                ShowPrompt("Manual Transaction", "Payment amount cannot be greater than the billable amount.");
                return;
            }

            try
            {
                string invoiceNumber = billingRepository.GenerateNextInvoiceNumber();

                BillingTransaction invoice = new()
                {
                    PatientId = SelectedManualPatient.PatientId,
                    AppointmentId = null,
                    TreatmentRecordId = null,
                    BillingSource = "Manual",
                    ReceiptNumber = invoiceNumber,

                    ServiceId = null,
                    ServiceName = ManualServiceName.Trim(),
                    Description = string.IsNullOrWhiteSpace(ManualDescription)
                        ? $"Manual invoice for {ManualServiceName}"
                        : ManualDescription.Trim(),

                    InvoiceTitle = ManualServiceName.Trim(),
                    IsInvoiceOpen = true,

                    TotalAmount = 0,
                    DiscountType = ManualDiscountType,
                    DiscountAmount = ManualDiscountAmount,
                    SubtotalAfterDiscount = 0,
                    AmountPaid = 0,
                    RemainingBalance = 0,
                    PaymentStatus = "Unpaid",
                    TransactionDate = DateTime.Today,
                    CreatedByUserId = null,
                    Notes = ManualNotes
                };

                int billingId = billingRepository.CreateInvoiceHeader(invoice);

                BillingTransactionItem item = new()
                {
                    BillingId = billingId,
                    AppointmentId = null,
                    TreatmentRecordId = null,
                    ServiceId = null,
                    ServiceName = ManualServiceName.Trim(),
                    ItemDescription = string.IsNullOrWhiteSpace(ManualDescription)
                        ? $"Manual transaction for {ManualServiceName}"
                        : ManualDescription.Trim(),
                    TreatmentDate = DateTime.Today,
                    Amount = ManualTotalAmount,
                    IsIncluded = false,
                    CreatedAt = DateTime.Now
                };

                billingRepository.AddBillingTransactionItem(item);

                if (ManualPaymentAmount > 0)
                {
                    PaymentRecord payment = new()
                    {
                        BillingId = billingId,
                        PatientId = SelectedManualPatient.PatientId,
                        AmountPaid = ManualPaymentAmount,
                        PaymentMethod = "Cash",
                        PaymentDate = DateTime.Today,
                        ReceivedByUserId = null,
                        Notes = ManualNotes
                    };

                    billingRepository.AddPaymentRecord(payment);
                }

                LoadBillingData();
                ClearManualTransactionForm();

                SelectedBillingModule = "Manual Transaction";

                ShowPrompt("Manual Transaction", $"Manual invoice {invoiceNumber} has been recorded successfully.");
            }
            catch (Exception ex)
            {
                ShowPrompt("Manual Transaction", $"Failed to process manual transaction: {ex.Message}");
            }
        }
        private void ClearManualTransactionForm()
        {
            SelectedManualPatient = null;
            ClearManualPatientFieldsOnly();

            ManualPatientSearchText = string.Empty;
            ManualPatientLookupResults.Clear();

            ManualServiceName = string.Empty;
            ManualDescription = string.Empty;
            ManualTotalAmount = 0;
            ManualDiscountType = "None";
            ManualVatExemptSales = 0;
            ManualDiscountAmount = 0;
            ManualPaymentAmount = 0;
            ManualBalance = 0;
            ManualNotes = string.Empty;

            OnPropertyChanged(nameof(ManualTransactionDateDisplay));
            OnPropertyChanged(nameof(HasManualVatExemption));
            OnPropertyChanged(nameof(ManualTotalAmountDisplay));
            OnPropertyChanged(nameof(ManualVatExemptSalesDisplay));
            OnPropertyChanged(nameof(ManualDiscountAmountDisplay));
            OnPropertyChanged(nameof(ManualPaymentAmountDisplay));
            OnPropertyChanged(nameof(ManualBalanceDisplay));
            OnPropertyChanged(nameof(ManualSubtotalDisplay));
        }

        private void ClearManualPatientFieldsOnly()
        {
            ManualReceiptNumber = string.Empty;
            ManualPatientCode = string.Empty;
            ManualPatientName = string.Empty;
            ManualCategory = string.Empty;
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