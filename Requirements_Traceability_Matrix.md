# Requirements Traceability Matrix
## Cruz-Nery Dental Clinic Management System

---

## Document Information

| Field | Value |
|---|---|
| Project | Cruz-Nery Dental Clinic Management System |
| Document Type | Requirements Traceability Matrix |
| Scope | End-to-end system requirements from authentication through all application modules |
| Status Basis | Current implemented system as reflected in the WPF application, SQLite repositories, services, and recent billing, privacy, styling, archive, and security updates |
| Prepared By | Augustine Barredo |
| Date | 2026-06-10 |

---

## Test Area Summary

| # | Test Area | Items | Met | Unmet | Remarks | Responsibility |
|---|---|---:|---:|---:|---|---|
| 1 | Authorization, Login, and Session Control | 12 | 12 | 0 | Implemented | Barredo |
| 2 | Employee Privacy Notice and Acknowledgement | 5 | 5 | 0 | Implemented | Barredo |
| 3 | Forgot Password and Security Questions | 9 | 9 | 0 | Implemented | Barredo |
| 4 | Dashboard | 8 | 8 | 0 | Implemented | Barredo |
| 5 | User Management | 14 | 14 | 0 | Implemented | Barredo |
| 6 | Patient Management | 16 | 16 | 0 | Implemented | Barredo |
| 7 | Appointment and Treatment Management | 18 | 18 | 0 | Implemented | Barredo |
| 8 | Billing, Payments, Invoices, and Receipts | 22 | 22 | 0 | Implemented | Barredo |
| 9 | Inventory Management | 12 | 12 | 0 | Implemented | Barredo |
| 10 | Reports and Activity Reporting | 10 | 10 | 0 | Implemented | Barredo |
| 11 | Maintenance, Backup, and Restore | 12 | 12 | 0 | Implemented | Barredo |
| 12 | Help and User Manual | 5 | 5 | 0 | Implemented | Barredo |
| 13 | System-Wide Security, Privacy, Audit, and UI Standards | 9 | 9 | 0 | Implemented | Barredo |
| **TOTAL** |  | **152** | **152** | **0** | **100% completed** |  |

---

## Module 1: Authorization, Login, and Session Control

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Authorization | 1.1 | The system shall display a login screen as the first access point before a user can access the dashboard or any module. | Met | LoginView, LoginViewModel | Done | Barredo |
| Authorization | 1.2 | Event flow: user enters user ID and password -> system validates required fields -> system checks the account record -> system verifies password hash and salt -> system either opens the main shell or displays an invalid credential message. | Met | LoginViewModel, UserRepository, PasswordService | Done | Barredo |
| Authorization | 1.3 | The system shall store passwords using a generated salt and SHA-256 hash, and shall never store a plain text password. | Met | PasswordService, DatabaseInitializer | Done | Barredo |
| Authorization | 1.4 | The system shall prevent archived or inactive user accounts from signing in as active users. | Met | UserRepository, LoginViewModel | Done | Barredo |
| Authorization | 1.5 | The system shall load the authenticated user into a session service so the rest of the application can determine the current user, role, and module access rights. | Met | SessionService, MainShellViewModel | Done | Barredo |
| Authorization | 1.6 | The system shall support role-based access for Admin, Dentist, Secretary, and Dental Assistant accounts. | Met | Users table, SidebarViewModel | Done | Barredo |
| Authorization | 1.7 | The system shall hide or restrict admin-only modules from non-admin users, including User Management, Reports, and Maintenance functions where applicable. | Met | SidebarViewModel, MainShellView | Done | Barredo |
| Authorization | 1.8 | The system shall redirect a successful user login to the main dashboard after session creation and required privacy acknowledgement checks. | Met | LoginViewModel, MainShellViewModel | Done | Barredo |
| Authorization | 1.9 | The system shall provide password visibility controls on password inputs without changing the centralized password field design. | Met | LoginView, CreateNewPasswordView | Done | Barredo |
| Authorization | 1.10 | The system shall apply centralized input styling to login text fields and password fields while preserving icon spacing for user ID, lock, and eye icons. | Met | App.xaml, LoginView | Done | Barredo |
| Authorization | 1.11 | The system shall write login and user-related security actions to the activity log when applicable. | Met | ActivityLogService, UserRepository | Done | Barredo |
| Authorization | 1.12 | Event flow: user selects logout -> system clears session state -> user is returned to the login screen and protected modules are no longer available. | Met | SidebarViewModel, SessionService | Done | Barredo |

---

## Module 2: Employee Privacy Notice and Acknowledgement

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Employee Privacy | 2.1 | The system shall require staff users to acknowledge an Employee Privacy Notice before accessing protected system modules for the first time. | Met | UserRepository, MainShell/Login flow | Done | Barredo |
| Employee Privacy | 2.2 | Event flow: staff logs in for the first time -> system detects missing privacy acknowledgement -> system displays privacy notice -> staff accepts -> acknowledgement is saved -> staff proceeds to the system. | Met | UserRepository, privacy acknowledgement fields | Done | Barredo |
| Employee Privacy | 2.3 | The system shall store employee privacy acknowledgement status, acknowledgement date and time, and privacy notice version. | Met | Users table fields | Done | Barredo |
| Employee Privacy | 2.4 | The system shall log employee privacy acknowledgement as an auditable user action. | Met | UserRepository, ActivityLogs | Done | Barredo |
| Employee Privacy | 2.5 | The privacy notice shall clarify that the clinic stores and processes both patient data and staff account/activity data for clinical, administrative, security, and compliance purposes. | Met | Privacy notice implementation | Done | Barredo |

---

## Module 3: Forgot Password and Security Questions

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Forgot Password | 3.1 | The system shall provide a Forgot Password option from the login screen. | Met | LoginView, ForgotPasswordView | Done | Barredo |
| Forgot Password | 3.2 | Event flow: user opens Forgot Password -> enters user ID -> system verifies account existence -> system loads assigned security questions -> user answers -> system verifies answers -> user creates a new password. | Met | ForgotPasswordViewModel, CreateNewPasswordViewModel | Done | Barredo |
| Forgot Password | 3.3 | The system shall require valid user identification before showing password recovery questions. | Met | ForgotPasswordViewModel, UserRepository | Done | Barredo |
| Forgot Password | 3.4 | The system shall store security answers as normalized salted hashes rather than plain text. | Met | PasswordService, UserRepository | Done | Barredo |
| Forgot Password | 3.5 | The system shall support legacy security answer normalization so older stored answers can still be verified during recovery. | Met | PasswordService | Done | Barredo |
| Forgot Password | 3.6 | The system shall require successful security question verification before opening the Create New Password screen. | Met | ForgotPasswordViewModel | Done | Barredo |
| Forgot Password | 3.7 | The system shall validate new password and confirm password fields before saving a reset password. | Met | CreateNewPasswordViewModel | Done | Barredo |
| Forgot Password | 3.8 | The system shall hash and salt the new password after reset and update the user record securely. | Met | UserRepository, PasswordService | Done | Barredo |
| Forgot Password | 3.9 | The system shall apply the centralized login/password field style to forgot password and create password screens while preserving icons and placeholder alignment. | Met | App.xaml, ForgotPasswordView, CreateNewPasswordView | Done | Barredo |

---

## Module 4: Dashboard

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Dashboard | 4.1 | The system shall display dashboard summary cards after successful login, including patient, user, appointment, billing, and inventory indicators. | Met | DashboardView, DashboardRepository | Done | Barredo |
| Dashboard | 4.2 | The dashboard shall show appointment queue information so staff can quickly identify waiting, scheduled, urgent, and in-treatment patients. | Met | DashboardViewModel, AppointmentRepository | Done | Barredo |
| Dashboard | 4.3 | The dashboard shall show low stock inventory warnings based on configured minimum stock thresholds. | Met | DashboardRepository, InventoryRepository | Done | Barredo |
| Dashboard | 4.4 | The dashboard shall show recent activities from the activity log for quick monitoring of system changes. | Met | DashboardRepository, ActivityLogs | Done | Barredo |
| Dashboard | 4.5 | The dashboard shall provide a calendar-style view of appointment days and allow navigation between months. | Met | DashboardView, DashboardDayItem | Done | Barredo |
| Dashboard | 4.6 | The dashboard shall support global search across relevant clinic records such as patients and users. | Met | DashboardViewModel, DashboardRepository | Done | Barredo |
| Dashboard | 4.7 | Event flow: user opens dashboard -> system queries summaries, queues, low stock, and activities -> dashboard renders updated operational data. | Met | DashboardViewModel | Done | Barredo |
| Dashboard | 4.8 | The dashboard shall use the same centralized visual language as the rest of the system, including icons, scrollbars, cards, and spacing. | Met | DashboardView, App.xaml | Done | Barredo |

---

## Module 5: User Management

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| User Management | 5.1 | Admin users shall be able to view active user accounts in a searchable and filterable user table. | Met | UserManagementView, UserManagementViewModel | Done | Barredo |
| User Management | 5.2 | The system shall allow admin users to open an Add User overlay with a multi-step flow for basic account information, security questions, and confirmation. | Met | AddUserOverlayView | Done | Barredo |
| User Management | 5.3 | Event flow: admin enters name, contact number, and role -> system validates required fields -> admin selects security questions and answers -> system previews confirmation -> admin submits -> user account is created. | Met | UserManagementViewModel, UserRepository | Done | Barredo |
| User Management | 5.4 | The system shall generate or validate a unique username for each user account and prevent duplicate active usernames. | Met | UserRepository | Done | Barredo |
| User Management | 5.5 | The system shall require first name, last name, role, contact number, password, and security questions for new user creation. | Met | AddUserOverlayView, UserManagementViewModel | Done | Barredo |
| User Management | 5.6 | The system shall align security question labels, dropdowns, and answer fields in one row for readable data entry. | Met | AddUserOverlayView, UpdateUserOverlayView | Done | Barredo |
| User Management | 5.7 | The system shall use centralized text box, password box, readonly box, and placeholder styling from App.xaml in user overlays. | Met | App.xaml, user overlay views | Done | Barredo |
| User Management | 5.8 | Admin users shall be able to update user account information including name, contact number, role, password, and recovery questions. | Met | UpdateUserOverlayView, UserManagementViewModel | Done | Barredo |
| User Management | 5.9 | The system shall archive users instead of permanently deleting accounts, preserving records for audit and historical traceability. | Met | UserRepository ArchiveUser | Done | Barredo |
| User Management | 5.10 | The system shall allow archived user accounts to be restored by authorized admin users. | Met | UserRepository RestoreUser | Done | Barredo |
| User Management | 5.11 | The system shall prevent invalid administrative actions that would leave the system without an active administrator account. | Met | UserRepository, UserManagementViewModel | Done | Barredo |
| User Management | 5.12 | The system shall support dentist role identification so dentist users can be assigned to appointments and treatment records. | Met | IsDentistRole field, UserRepository | Done | Barredo |
| User Management | 5.13 | User account additions, updates, archives, restores, password changes, and security question updates shall be logged in the activity log. | Met | UserRepository, ActivityLogService | Done | Barredo |
| User Management | 5.14 | The user details view shall allow staff to inspect selected account information without directly editing it in the list table. | Met | UserDetailsOverlayView | Done | Barredo |

---

## Module 6: Patient Management

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Patient Management | 6.1 | Staff shall be able to view patient records in a searchable patient list with patient ID, name, contact details, category, and status. | Met | PatientManagement.xaml, PatientRepository | Done | Barredo |
| Patient Management | 6.2 | The system shall allow staff to register new patients using an Add Patient overlay. | Met | AddPatientOverlayView, PatientManagementViewModel | Done | Barredo |
| Patient Management | 6.3 | Event flow: staff opens Add Patient -> enters identity, contact, address, date of birth, medical information, emergency contact, and consent -> system validates -> patient is saved -> list refreshes. | Met | PatientManagementViewModel, PatientRepository | Done | Barredo |
| Patient Management | 6.4 | The system shall generate and display patient codes so patients can be searched by code or name. | Met | PatientRepository, PatientListItem | Done | Barredo |
| Patient Management | 6.5 | The system shall require patient date of birth rules that prevent invalid newborn or same-day DOB entries inappropriate for the clinic workflow. | Met | PatientManagementViewModel DOB validation | Done | Barredo |
| Patient Management | 6.6 | The system shall implement a date-of-birth picker using separate day, month, and year dropdowns for clearer DOB entry. | Met | Patient overlay views | Done | Barredo |
| Patient Management | 6.7 | The system shall support patient categories such as Regular, PWD, Senior Citizen, and PWD/Senior where applicable for billing discount and VAT exemption rules. | Met | Patient model, BillingViewModel | Done | Barredo |
| Patient Management | 6.8 | The system shall store medical condition notes, allergy notes, clearance notes, and initial treatment notes as part of the patient record. | Met | Patients table, PatientRepository | Done | Barredo |
| Patient Management | 6.9 | Staff shall be able to update existing patient records using an Update Patient overlay with the same validation and consent handling as registration. | Met | UpdatePatientOverlayView, PatientManagementViewModel | Done | Barredo |
| Patient Management | 6.10 | The system shall present a patient data privacy consent form before storing or updating personal and medical data. | Met | PatientManagementViewModel, consent overlay | Done | Barredo |
| Patient Management | 6.11 | The system shall store patient privacy consent status, consent timestamp, and consent version. | Met | PatientRepository, Patients table | Done | Barredo |
| Patient Management | 6.12 | The system shall display a clear divider before the data consent form in Add Patient and Update Patient overlays. | Met | Patient overlay views | Done | Barredo |
| Patient Management | 6.13 | The system shall archive patient records instead of permanently deleting patient data. | Met | PatientRepository archive logic | Done | Barredo |
| Patient Management | 6.14 | The system shall allow authorized users to restore archived patient records. | Met | PatientRepository restore logic | Done | Barredo |
| Patient Management | 6.15 | The system shall display patient profile information, appointment history, treatment history, and billing-related references where applicable. | Met | PatientManagementViewModel, repositories | Done | Barredo |
| Patient Management | 6.16 | Patient create, update, archive, restore, and privacy consent changes shall be logged for accountability. | Met | PatientRepository, ActivityLogService | Done | Barredo |

---

## Module 7: Appointment and Treatment Management

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Appointments | 7.1 | Staff shall be able to create scheduled appointments for selected patients with appointment date, arrival time, dentist, service, notes, and optional images. | Met | AddScheduledAppointmentOverlayView, AppointmentRepository | Done | Barredo |
| Appointments | 7.2 | Staff shall be able to create walk-in appointments and place the patient into the queue for the current clinic date. | Met | AddWalkInAppointmentOverlayView, AppointmentRepository | Done | Barredo |
| Appointments | 7.3 | Event flow: staff selects patient -> selects service and dentist -> chooses date/time or walk-in details -> system validates required fields and scheduling rules -> appointment is saved -> queue and calendar refresh. | Met | AppointmentManagementViewModel | Done | Barredo |
| Appointments | 7.4 | The system shall support patient lookup in appointment forms by patient ID or name. | Met | AppointmentManagementViewModel, AppointmentRepository | Done | Barredo |
| Appointments | 7.5 | The appointment date picker shall use a standard calendar-style DatePicker with project-appropriate colors and styling. | Met | App.xaml, appointment views | Done | Barredo |
| Appointments | 7.6 | The arrival time picker shall use dropdown-based time selection so hour, minute, and AM/PM values are controlled and consistent. | Met | Appointment overlay views | Done | Barredo |
| Appointments | 7.7 | The reschedule overlay shall use the updated time dropdown controls to match appointment creation forms. | Met | AppointmentManagementView.xaml | Done | Barredo |
| Appointments | 7.8 | The system shall prevent invalid schedule selections and reduce double-booking risk for dentist and appointment slots. | Met | AppointmentManagementViewModel, AppointmentRepository | Done | Barredo |
| Appointments | 7.9 | Staff shall be able to mark scheduled appointments as arrived, moving the patient into the active queue. | Met | AppointmentManagementViewModel | Done | Barredo |
| Appointments | 7.10 | Staff shall be able to mark appointments as urgent and update queue priority. | Met | AppointmentManagementViewModel, Appointments table | Done | Barredo |
| Appointments | 7.11 | Staff shall be able to start treatment from an appointment row and record treatment operation details. | Met | AppointmentManagementViewModel, TreatmentRecord | Done | Barredo |
| Appointments | 7.12 | Staff shall be able to complete appointments and create treatment records with treatment notes, service stage, follow-up date, and operation details. | Met | TreatmentRecord, AppointmentRepository | Done | Barredo |
| Appointments | 7.13 | Staff shall be able to mark appointments as no-show or cancel appointments with reason documentation. | Met | AppointmentManagementViewModel | Done | Barredo |
| Appointments | 7.14 | The system shall provide appointment details and operation details overlays for review without editing directly in the table. | Met | AppointmentManagementView.xaml | Done | Barredo |
| Appointments | 7.15 | The appointment table shall support search, filters, and sort controls appropriate for staff daily workflow. | Met | AppointmentManagementView | Done | Barredo |
| Appointments | 7.16 | The system shall maintain uploaded appointment/treatment images and allow image management where applicable. | Met | AppointmentImageService, appointment overlays | Done | Barredo |
| Appointments | 7.17 | Completed treatments shall become available to billing as unbilled completed treatment items. | Met | BillingRepository, AppointmentPaymentItem | Done | Barredo |
| Appointments | 7.18 | Appointment add, update, reschedule, status change, treatment start, completion, no-show, and cancellation actions shall be logged. | Met | AppointmentRepository, ActivityLogService | Done | Barredo |

---

## Module 8: Billing, Payments, Invoices, and Receipts

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Billing | 8.1 | Staff shall be able to process invoice payments from completed and unbilled appointment treatments. | Met | BillingViewModel, BillingRepository | Done | Barredo |
| Billing | 8.2 | Event flow: staff searches completed treatment -> selects patient/treatment -> creates or selects invoice -> adds treatment item -> enters amount and payment method -> system calculates balance/change -> payment is recorded -> receipt can be viewed or printed. | Met | BillingView, BillingViewModel | Done | Barredo |
| Billing | 8.3 | Staff shall be able to create a new invoice for a completed treatment and assign a clear invoice title and invoice item details. | Met | BillingRepository CreateOpenInvoice | Done | Barredo |
| Billing | 8.4 | Staff shall be able to add selected completed treatment items to an existing open invoice for the same patient. | Met | BillingRepository, SelectedInvoiceItems | Done | Barredo |
| Billing | 8.5 | The system shall prevent the same completed treatment from being billed more than once. | Met | Treatment billing flags, BillingRepository | Done | Barredo |
| Billing | 8.6 | Staff shall be able to process balance payments for invoices with unpaid or partial balances. | Met | BalancePaymentView, BillingViewModel | Done | Barredo |
| Billing | 8.7 | Staff shall be able to create manual transactions for non-appointment billing while still linking the transaction to a patient. | Met | ManualTransactionView, BillingViewModel | Done | Barredo |
| Billing | 8.8 | The system shall calculate gross amount, discount amount, billable amount, amount paid, remaining balance, and change amount. | Met | BillingViewModel, BillingRepository | Done | Barredo |
| Billing | 8.9 | The system shall automatically calculate change when payment amount exceeds the billable amount or remaining balance. | Met | BillingViewModel change fields | Done | Barredo |
| Billing | 8.10 | The system shall support discounts and VAT exemption rules for Senior Citizen, PWD, and PWD/Senior patient categories. | Met | BillingViewModel, BillingReceiptDetails | Done | Barredo |
| Billing | 8.11 | The system shall track payment status as Unpaid, Partial, or Paid according to total paid and remaining balance. | Met | BillingRepository DeterminePaymentStatus | Done | Barredo |
| Billing | 8.12 | The system shall support payment history per patient, including compact recent transactions and a full transaction history overlay. | Met | BillingView, SelectedPatientBillingRecords | Done | Barredo |
| Billing | 8.13 | Compact payment history cards shall show a view icon and a menu icon for archive or restore actions without cutting off controls. | Met | BillingView compact card actions | Done | Barredo |
| Billing | 8.14 | Full transaction history shall provide a filter for Current and Archived transactions, hiding archived records from the current list. | Met | BillingViewModel FilteredPatientBillingRecords | Done | Barredo |
| Billing | 8.15 | Archived billing records shall be restorable from the transaction history overlay and the Archived filter of the recent billing table. | Met | BillingRepository RestoreBillingRecord | Done | Barredo |
| Billing | 8.16 | Archive and restore actions for billing records shall ask for user confirmation before state changes are saved. | Met | BillingViewModel ArchiveBillingRecord, RestoreBillingRecord | Done | Barredo |
| Billing | 8.17 | Archived billing records shall not display normal Print or View actions in the recent billing table; they shall display Restore instead. | Met | BillingView DataGrid actions | Done | Barredo |
| Billing | 8.18 | The system shall generate receipt and dental invoice detail views containing patient information, transaction date, invoice items, payment history, gross amount, discounts, billable amount, amount paid, change, remaining balance, and payment status. | Met | BillingReceiptDetails, BillingView | Done | Barredo |
| Billing | 8.19 | The system shall generate printable PDF receipts/invoices with the same billing details, including the change amount. | Met | ReceiptPDFService | Done | Barredo |
| Billing | 8.20 | Sensitive billing transaction, payment record, and invoice item fields shall be stored with AES-GCM encryption at rest, while non-sensitive query metadata remains queryable. | Met | CryptoService, BillingRepository, DatabaseInitializer | Done | Barredo |
| Billing | 8.21 | The system shall encrypt older plain billing values during startup schema/encryption migration when they are not already encrypted. | Met | DatabaseInitializer EnsureBillingSensitiveDataEncrypted | Done | Barredo |
| Billing | 8.22 | Billing create, payment, archive, restore, print, and receipt-related actions shall be logged for audit trail purposes. | Met | BillingRepository, ActivityLogService | Done | Barredo |

---

## Module 9: Inventory Management

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Inventory | 9.1 | Admin users shall be able to view active inventory items with item name, quantity, unit price, stock threshold, and status. | Met | InventoryView, InventoryRepository | Done | Barredo |
| Inventory | 9.2 | Event flow: admin opens inventory -> adds item details -> system validates duplicate and required fields -> item is saved -> inventory table and low stock indicators refresh. | Met | InventoryViewModel, InventoryRepository | Done | Barredo |
| Inventory | 9.3 | The system shall allow inventory item creation with item name, description/category where applicable, quantity, unit price, and minimum threshold. | Met | InventoryViewModel, InventoryItem | Done | Barredo |
| Inventory | 9.4 | The system shall allow authorized users to update inventory details and stock information. | Met | InventoryRepository UpdateItem | Done | Barredo |
| Inventory | 9.5 | The system shall identify low stock items when quantity is less than or equal to the configured threshold. | Met | InventoryRepository, DashboardRepository | Done | Barredo |
| Inventory | 9.6 | The system shall support stock in and stock out adjustments with quantity validation. | Met | InventoryViewModel, InventoryRepository | Done | Barredo |
| Inventory | 9.7 | The system shall log inventory movements and adjustments with timestamp and responsible user. | Met | InventoryRepository, ActivityLogService | Done | Barredo |
| Inventory | 9.8 | The system shall archive inventory items instead of permanently deleting them. | Met | InventoryRepository ArchiveItem | Done | Barredo |
| Inventory | 9.9 | The system shall allow archived inventory items to be restored. | Met | InventoryRepository RestoreItem | Done | Barredo |
| Inventory | 9.10 | Inventory lists shall support search, filtering, sorting, pagination, and clear table actions. | Met | InventoryView | Done | Barredo |
| Inventory | 9.11 | Inventory overlays shall use centralized controls, date picker styling, scrollbars, and icon buttons consistent with the rest of the system. | Met | InventoryView, App.xaml | Done | Barredo |
| Inventory | 9.12 | Inventory data shall be available to dashboard and reports for operational summaries and low stock monitoring. | Met | DashboardRepository, ReportsRepository | Done | Barredo |

---

## Module 10: Reports and Activity Reporting

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Reports | 10.1 | Admin users shall be able to access the Reports module from the sidebar. | Met | SidebarViewModel, ReportsView | Done | Barredo |
| Reports | 10.2 | The system shall provide date range selection for reports using styled date inputs. | Met | ReportsView, ReportsViewModel | Done | Barredo |
| Reports | 10.3 | The system shall generate patient reports including patient demographics and record counts. | Met | ReportsRepository, ReportsModels | Done | Barredo |
| Reports | 10.4 | The system shall generate appointment reports showing appointment volume and status information. | Met | ReportsRepository | Done | Barredo |
| Reports | 10.5 | The system shall generate billing and collection reports with payment and revenue breakdowns. | Met | ReportsRepository, BillingRepository | Done | Barredo |
| Reports | 10.6 | The system shall generate inventory reports showing stock levels and low stock indicators. | Met | ReportsRepository, InventoryRepository | Done | Barredo |
| Reports | 10.7 | The system shall generate activity log reports grouped by action type and module. | Met | ReportsRepository Activity Logs | Done | Barredo |
| Reports | 10.8 | The system shall display charts and visual summaries for report insights. | Met | Views/Charts, ReportsView | Done | Barredo |
| Reports | 10.9 | Event flow: admin selects report type and date range -> system queries repository -> system renders table and chart data -> admin reviews or exports/prints where supported. | Met | ReportsViewModel | Done | Barredo |
| Reports | 10.10 | Report screens shall apply the same centralized scrollbar and table styling used across the application. | Met | ReportsView, App.xaml | Done | Barredo |

---

## Module 11: Maintenance, Backup, and Restore

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Maintenance | 11.1 | Admin users shall be able to access Maintenance functions for backup and restore operations. | Met | SidebarViewModel, MaintenanceView | Done | Barredo |
| Maintenance | 11.2 | The system shall allow the admin to select a primary backup location. | Met | MaintenanceViewModel BrowsePrimaryBackupLocation | Done | Barredo |
| Maintenance | 11.3 | The system shall allow the admin to select a secondary backup location for backup copy redundancy. | Met | MaintenanceViewModel BrowseSecondaryBackupLocation | Done | Barredo |
| Maintenance | 11.4 | Event flow: admin selects backup location -> enters backup password -> system creates encrypted backup package -> optional copy is saved to secondary location -> backup history and retention policy refresh. | Met | BackupPackageService, MaintenanceViewModel | Done | Barredo |
| Maintenance | 11.5 | The system shall create encrypted backup files using the clinic backup file extension and backup package service. | Met | BackupPackageService, MaintenanceViewModel | Done | Barredo |
| Maintenance | 11.6 | The system shall maintain backup history showing file name, backup type, file size, creation date, and location. | Met | BackupHistoryItem, MaintenanceViewModel | Done | Barredo |
| Maintenance | 11.7 | The backup history overlay shall support pagination so long backup lists remain usable. | Met | MaintenanceViewModel BackupPageItems | Done | Barredo |
| Maintenance | 11.8 | The system shall support backup retention options and automatically remove older backups according to the selected policy. | Met | BackupRetentionService | Done | Barredo |
| Maintenance | 11.9 | The system shall allow an admin to browse for a backup file to restore. | Met | MaintenanceViewModel BrowseRestoreFile | Done | Barredo |
| Maintenance | 11.10 | Event flow: admin selects restore file -> system prompts confirmation that current database and AES-GCM key will be replaced -> admin enters backup password -> system restores encrypted backup -> system prompts application restart. | Met | MaintenanceViewModel RestoreBackupFile | Done | Barredo |
| Maintenance | 11.11 | The system shall reject invalid backup passwords or corrupted backup files and display a restore failure message. | Met | BackupPackageService, MaintenanceViewModel | Done | Barredo |
| Maintenance | 11.12 | Backup and restore activities shall be logged or otherwise traceable through the maintenance workflow. | Met | MaintenanceViewModel, ActivityLogService | Done | Barredo |

---

## Module 12: Help and User Manual

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| Help | 12.1 | The system shall provide a Help module accessible from the sidebar. | Met | HelpView, HelpViewModel | Done | Barredo |
| Help | 12.2 | The Help module shall provide topic-based user manual content for major system functions. | Met | HelpManualTopic, HelpViewModel | Done | Barredo |
| Help | 12.3 | The Help module shall provide FAQ items for common questions and user guidance. | Met | FAQItem, HelpViewModel | Done | Barredo |
| Help | 12.4 | The system shall allow users to search help documentation by keyword. | Met | HelpViewModel | Done | Barredo |
| Help | 12.5 | The system shall support user manual PDF generation for documentation distribution. | Met | UserManualPDFService | Done | Barredo |

---

## Module 13: System-Wide Security, Privacy, Audit, and UI Standards

| Module | R# | Requirement / Detailed Flow | Status | Source / Trace | Remarks | Responsibility |
|---|---|---|---|---|---|---|
| System Standards | 13.1 | The system shall use SQLite as the local database and initialize required tables, indexes, schema migrations, and seed records on startup. | Met | DatabaseInitializer, DatabaseService | Done | Barredo |
| System Standards | 13.2 | The system shall use AES-GCM encryption for sensitive stored values such as billing details and protected backup packages. | Met | CryptoService, BackupPackageService, BillingRepository | Done | Barredo |
| System Standards | 13.3 | The system shall maintain an activity log table containing user, action type, module, description, and timestamp for traceability. | Met | ActivityLogs table, ActivityLogService | Done | Barredo |
| System Standards | 13.4 | The system shall prefer archive and restore workflows over permanent deletion for users, patients, inventory, and billing records. | Met | UserRepository, PatientRepository, InventoryRepository, BillingRepository | Done | Barredo |
| System Standards | 13.5 | The system shall apply centralized text field, readonly field, password field, text area, date picker, time picker, list box, DataGrid, and scrollbar styles where applicable. | Met | App.xaml and view resource dictionaries | Done | Barredo |
| System Standards | 13.6 | The system shall use consistent visual controls such as icons for tool actions, dropdowns for option sets, date/time pickers for temporal data, and tooltips for icon-only actions. | Met | WPF views, FontAwesome icons | Done | Barredo |
| System Standards | 13.7 | The system shall apply required-field asterisk styling consistently across forms so mandatory data is visually distinguishable. | Met | User, Patient, Appointment, Billing views | Done | Barredo |
| System Standards | 13.8 | The system shall avoid overlapping text, clipped controls, and inconsistent field heights through centralized styles and targeted layout corrections. | Met | App.xaml, module views | Done | Barredo |
| System Standards | 13.9 | The system shall keep archived data out of default current views while providing explicit filters and restore actions for authorized recovery. | Met | Billing, User, Patient, Inventory modules | Done | Barredo |

---

## Compliance Summary

| Metric | Value |
|---|---:|
| Total Requirements | 152 |
| Met Requirements | 152 |
| Unmet Requirements | 0 |
| Completion Status | 100% |

---

## Sign-off

| Role | Name | Signature | Date |
|---|---|---|---|
| Project Manager |  |  |  |
| Lead Developer | Augustine Barredo |  | 2026-06-10 |
| QA Lead |  |  |  |
| Client / Stakeholder |  |  |  |

