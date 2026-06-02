
```
CruzNeryClinic
├─ App.xaml
├─ App.xaml.cs
├─ AssemblyInfo.cs
├─ Assets
│  ├─ Fonts
│  │  └─ OpenSans
│  │     ├─ OpenSans-Bold.ttf
│  │     ├─ OpenSans-Italic.ttf
│  │     ├─ OpenSans-Light.ttf
│  │     ├─ OpenSans-Medium.ttf
│  │     ├─ OpenSans-MediumItalic.ttf
│  │     ├─ OpenSans-Regular.ttf
│  │     ├─ OpenSans-SemiBold.ttf
│  │     └─ OpenSans-SemiBoldItalic.ttf
│  └─ Images
│     ├─ logo.png
│     └─ pin.png
├─ BigPicture
├─ Converters
│  ├─ CountToVisibilityConverter.cs
│  └─ PesoCurrencyConverter.cs
├─ CruzNeryClinic.csproj
├─ CruzNeryClinic.csproj.lscache
├─ CruzNeryClinic.sln
├─ Data
│  ├─ DatabaseInitializer.cs
│  └─ DatabaseService.cs
├─ MainWindow.xaml
├─ MainWindow.xaml.cs
├─ Models
│  ├─ ActivityLog.cs
│  ├─ Appointment.cs
│  ├─ AppointmentCalendarDay.cs
│  ├─ AppointmentDentistOption.cs
│  ├─ AppointmentListItem.cs
│  ├─ AppointmentPatientMedicalAlert.cs
│  ├─ AppointmentPatientSearchItem.cs
│  ├─ AppointmentPaymentItem.cs
│  ├─ AppointmentServiceOption.cs
│  ├─ BalancePaymentItem.cs
│  ├─ BillingPatientLookupItem.cs
│  ├─ BillingReceiptDetails.cs
│  ├─ BillingRecordListItem.cs
│  ├─ BillingTransaction.cs
│  ├─ Dashboard
│  │  ├─ DashboardActivityItem.cs
│  │  ├─ DashboardLowStockItem.cs
│  │  ├─ DashboardQueueItem.cs
│  │  ├─ DashboardSummary.cs
│  │  └─ DashboardTransactionItems.cs
│  ├─ FAQItem.cs
│  ├─ HelpManualTopic.cs
│  ├─ Inventory
│  │  └─ InventoryItem.cs
│  ├─ Maintenance
│  │  ├─ BackupHistoryItem.cs
│  │  └─ BackupMetadata.cs
│  ├─ Patient.cs
│  ├─ PatientListItem.cs
│  ├─ PaymentRecord.cs
│  ├─ ReportsModels.cs
│  ├─ SecurityQuestion.cs
│  ├─ ServiceItem.cs
│  ├─ TreatmentRecord.cs
│  ├─ TreatmentRecordListItem.cs
│  ├─ User.cs
│  └─ UserManagement
│     └─ UserListItem.cs
├─ README.md
├─ Repositories
│  ├─ AppointmentRepository.cs
│  ├─ BillingRepository.cs
│  ├─ DashboardRepository.cs
│  ├─ InventoryRepository.cs
│  ├─ PatientRepository.cs
│  ├─ ReportsRepository.cs
│  └─ UserRepository.cs
├─ Required_package.txt
├─ Requirements_Traceability_Matrix.md
├─ runClean.bat
├─ Services
│  ├─ ActivityLogService.cs
│  ├─ BackupPackageService.cs
│  ├─ CryptoService.cs
│  ├─ PasswordService.cs
│  ├─ ReceiptPDFService.cs
│  └─ SessionService.cs
├─ ViewModels
│  ├─ AppointmentManagementViewModel.cs
│  ├─ BaseViewModel.cs
│  ├─ BillingViewModel.cs
│  ├─ CreateNewPasswordViewModel.cs
│  ├─ DashboardViewModel.cs
│  ├─ ForgotPasswordViewModel.cs
│  ├─ HelpViewModel.cs
│  ├─ InventoryViewModel.cs
│  ├─ LoginViewModel.cs
│  ├─ MainShellViewModel.cs
│  ├─ MaintenanceViewModel.cs
│  ├─ PatientManagementViewModel.cs
│  ├─ ReportsViewModel.cs
│  ├─ SecurityQuestionsViewModel.cs
│  ├─ Shared
│  │  └─ SidebarViewModel.cs
│  ├─ SplashViewModel.cs
│  └─ UserManagementViewModel.cs
└─ Views
   ├─ AppointmentManagement
   │  ├─ AddScheduledAppointmentOverlayView.xaml
   │  ├─ AddScheduledAppointmentOverlayView.xaml.cs
   │  ├─ AddWalkInAppointmentOverlayView.xaml
   │  ├─ AddWalkInAppointmentOverlayView.xaml.cs
   │  └─ AppointmentManagementStyles.xaml
   ├─ AppointmentManagementView.xaml
   ├─ AppointmentManagementView.xaml.cs
   ├─ Billing
   │  ├─ BalancePaymentView.xaml
   │  ├─ BalancePaymentView.xaml.cs
   │  ├─ BillingManagementStyles.xaml
   │  ├─ ManualTransactionView.xaml
   │  └─ ManualTransactionView.xaml.cs
   ├─ BillingView.xaml
   ├─ BillingView.xaml.cs
   ├─ CreateNewPasswordView.xaml
   ├─ CreateNewPasswordView.xaml.cs
   ├─ DashboardView.xaml
   ├─ DashboardView.xaml.cs
   ├─ ForgotPasswordView.xaml
   ├─ ForgotPasswordView.xaml.cs
   ├─ HelpView.xaml
   ├─ HelpView.xaml.cs
   ├─ InventoryView.xaml
   ├─ InventoryView.xaml.cs
   ├─ LoginView.xaml
   ├─ LoginView.xaml.cs
   ├─ MainShellView.xaml
   ├─ MainShellView.xaml.cs
   ├─ MaintenanceView.xaml
   ├─ MaintenanceView.xaml.cs
   ├─ PatientManagement
   │  ├─ AddPatientOverlayView.xaml
   │  ├─ AddPatientOverlayView.xaml.cs
   │  ├─ PatientManagementStyles.xaml
   │  ├─ UpdatePatientOverlayView.xaml
   │  └─ UpdatePatientOverlayView.xaml.cs
   ├─ PatientManagement.xaml
   ├─ PatientManagement.xaml.cs
   ├─ ReportsView.xaml
   ├─ ReportsView.xaml.cs
   ├─ SecurityQuestionsView.xaml
   ├─ SecurityQuestionsView.xaml.cs
   ├─ Shared
   │  ├─ SidebarView.xaml
   │  └─ SidebarView.xaml.cs
   ├─ splash.xaml.cs
   ├─ SplashView.xaml
   ├─ UserManagement
   │  ├─ AddUserOverlayView.xaml
   │  ├─ AddUserOverlayView.xaml.cs
   │  ├─ UpdateUserOverlayView.xaml
   │  ├─ UpdateUserOverlayView.xaml.cs
   │  └─ UserManagementStyles.xaml
   ├─ UserManagementView.xaml
   └─ UserManagementView.xaml.cs

```



Things Need To Do:
Dashboard - filter beside the search also make the search functional.
Manage User Add user security question - fix layout alignment.
Manage User Update form - fix layout.
Patient Management - add view in actions
Appointment - overhaul of treatment service (uploading of teeths, trials etc.)
Billing - make it 15% discount. 
