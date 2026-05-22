# Requirements Traceability Matrix
## Clinic Management System

---

## Test Area Summary

| # | Test Area | Items | Met (Count) | Unmet (Count) | Remarks | Responsibility |
|---|-----------|-------|-----------|--------------|---------|-----------------|
| 1 | Login | 8 | 8 | 0 | Done | Backend/Security |
| 2 | Forgot Password | 6 | 6 | 0 | Done | Security/Authentication |
| 3 | User Management | 7 | 7 | 0 | Done | Admin |
| 4 | Patient Management | 9 | 9 | 0 | Done | Staff/Admin |
| 5 | Appointments | 8 | 8 | 0 | Done | Staff/Admin |
| 6 | Billing | 7 | 7 | 0 | Done | Staff/Admin |
| 7 | Inventory Management | 8 | 8 | 0 | Done | Admin/Staff |
| 8 | Dashboard | 6 | 6 | 0 | Done | All Users |
| 9 | Activity Logging | 5 | 5 | 0 | Done | System |
| 10 | Session Management | 4 | 4 | 0 | Done | Backend/Security |
| | **TOTAL** | **68** | **68** | **0** | | |

---

## Module 1: Login

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 1.1 | **Event Flow:** User accesses login page → submits username and password → system validates credentials → generates SHA256 token → determines access level or displays error | Met | Done | Backend/Security |
| 1.2 | Login page accepts input fields for username and password | Met | Done | Frontend (LoginView.xaml) |
| 1.3 | Usernames limited to alphanumeric characters, maximum 16 characters | Met | Done | LoginViewModel |
| 1.4 | Passwords are case-sensitive with minimum 8 characters | Met | Done | PasswordService.cs |
| 1.5 | Only registered user accounts can login; invalid credentials display error message: "Invalid Credentials" | Met | Done | UserRepository/Auth |
| 1.6 | Random unique token generated upon successful login with expiration time | Met | Done | SessionService.cs |
| 1.7 | Access level determined: Staff access (Customer Management, Dashboard, Help); Admin access (all staff + User Management, Inventory, Reports, Maintenance) | Met | Done | SessionService/Authorization |
| 1.8 | Successful login redirects to Dashboard page | Met | Done | MainWindow/Navigation |

---

## Module 2: Forgot Password

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 2.1 | User can access Forgot Password page from login screen | Met | Done | LoginView Navigation |
| 2.2 | User enters username to initiate password recovery | Met | Done | ForgotPasswordView.xaml |
| 2.3 | System verifies username exists in database | Met | Done | UserRepository |
| 2.4 | System prompts user with security questions | Met | Done | SecurityQuestionsView.xaml |
| 2.5 | User answers security questions correctly to proceed | Met | Done | SecurityQuestionsViewModel |
| 2.6 | Upon verification, user redirected to Create New Password page | Met | Done | CreateNewPasswordView.xaml |

---

## Module 3: User Management

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 3.1 | Admin can view list of all registered users | Met | Done | UserManagementView.xaml |
| 3.2 | Admin can add new user accounts with username, password, and role assignment | Met | Done | AddUserOverlayView.xaml |
| 3.3 | Admin can update existing user information (username, role, status) | Met | Done | UpdateUserOverlayView.xaml |
| 3.4 | User passwords stored securely using encryption/hashing | Met | Done | PasswordService.cs |
| 3.5 | System validates new username for uniqueness and format compliance | Met | Done | UserRepository |
| 3.6 | Admin can deactivate/activate user accounts | Met | Done | UserManagementViewModel |
| 3.7 | Changes to user accounts are logged in Activity Log | Met | Done | ActivityLog.cs |

---

## Module 4: Patient Management

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 4.1 | Staff can view list of all registered patients | Met | Done | PatientManagement.xaml |
| 4.2 | Staff can register new patient with name, contact, medical history | Met | Done | PatientManagementViewModel |
| 4.3 | Staff can search/filter patients by name, ID, contact number | Met | Done | PatientManagement Search |
| 4.4 | Staff can view complete patient profile and medical records | Met | Done | PatientListItem.cs |
| 4.5 | Staff can update patient information (contact, address, emergency contact) | Met | Done | PatientManagementViewModel |
| 4.6 | System maintains patient confidentiality and access control | Met | Done | Authorization Layer |
| 4.7 | Patient records include appointment history and billing information | Met | Done | Appointment/Billing Models |
| 4.8 | Changes to patient records are logged in Activity Log | Met | Done | ActivityLog.cs |
| 4.9 | System supports patient deactivation (soft delete) | Met | Done | PatientManagementViewModel |

---

## Module 5: Appointments

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 5.1 | Staff can create new appointment with patient, date/time, reason | Met | Done | Appointment.cs Model |
| 5.2 | System displays appointment calendar view | Met | Done | Dashboard/DashboardView |
| 5.3 | Staff can view appointment queue/status (pending, confirmed, completed) | Met | Done | DashboardQueueItem.cs |
| 5.4 | System prevents double-booking of appointments | Met | Done | Appointment Validation |
| 5.5 | Staff can reschedule existing appointments | Met | Done | AppointmentViewModel |
| 5.6 | Staff can cancel appointments with reason documentation | Met | Done | Appointment.cs |
| 5.7 | System sends appointment reminders/notifications | Met | Done | Dashboard Notifications |
| 5.8 | Appointments automatically linked to patient and billing records | Met | Done | Appointment Model Relations |

---

## Module 6: Billing

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 6.1 | Staff can create billing record linked to patient appointment | Met | Done | Billing.cs Model |
| 6.2 | System calculates total cost based on services provided | Met | Done | Billing Calculation |
| 6.3 | Staff can apply discounts/adjustments to billing | Met | Done | Billing.cs |
| 6.4 | System tracks payment status (paid, pending, overdue) | Met | Done | Billing Model |
| 6.5 | Staff can generate invoices for patient | Met | Done | Billing/Reports |
| 6.6 | System supports multiple payment methods | Met | Done | Billing.cs |
| 6.7 | Billing records are archived and accessible for audit | Met | Done | DatabaseService |

---

## Module 7: Inventory Management

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 7.1 | Admin can view inventory list with item name, quantity, unit cost | Met | Done | InventoryItem.cs |
| 7.2 | Admin can add new inventory items | Met | Done | Inventory Operations |
| 7.3 | Admin can update inventory quantity and pricing | Met | Done | InventoryManagement |
| 7.4 | System tracks low stock items (alerts when below threshold) | Met | Done | DashboardLowStockItem.cs |
| 7.5 | Admin can set minimum stock level for automatic reorder alerts | Met | Done | InventoryItem.cs |
| 7.6 | System logs inventory movements and adjustments | Met | Done | ActivityLog.cs |
| 7.7 | Staff can view inventory for billing purposes (read-only) | Met | Done | InventoryItem Access |
| 7.8 | System supports inventory categorization and search | Met | Done | InventoryItem.cs |

---

## Module 8: Dashboard

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 8.1 | Dashboard displays summary of key metrics (patients, appointments, revenue) | Met | Done | DashboardSummary.cs |
| 8.2 | Dashboard shows recent activity feed with timestamps | Met | Done | DashboardActivityItem.cs |
| 8.3 | Dashboard displays appointment queue for current day | Met | Done | DashboardQueueItem.cs |
| 8.4 | Dashboard alerts display low inventory items | Met | Done | DashboardLowStockItem.cs |
| 8.5 | Dashboard displays financial summary (revenue, pending payments) | Met | Done | DashboardTransactionItems.cs |
| 8.6 | Users can customize dashboard layout (admin/staff role-specific) | Met | Done | DashboardViewModel |

---

## Module 9: Activity Logging

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 9.1 | System logs all user login/logout events with timestamp and user ID | Met | Done | ActivityLog.cs |
| 9.2 | System logs data modifications (create, update, delete) with user and timestamp | Met | Done | ActivityLog.cs |
| 9.3 | Admin can view activity logs filtered by date, user, or action type | Met | Done | ActivityLog Queries |
| 9.4 | Activity logs are non-editable and archived for audit compliance | Met | Done | DatabaseInitializer |
| 9.5 | System tracks access to sensitive patient data | Met | Done | ActivityLog.cs |

---

## Module 10: Session Management

| R# | Requirements | Status | Remarks | Responsibility |
|----|--------------|--------|---------|-----------------|
| 10.1 | System maintains active session with token validation | Met | Done | SessionService.cs |
| 10.2 | Session expires after inactivity timeout (default: 30 minutes) | Met | Done | SessionService Configuration |
| 10.3 | User can manually logout, clearing session and token | Met | Done | SessionService.Logout |
| 10.4 | System prevents access to protected pages without valid session | Met | Done | Authorization Middleware |

---

## Implementation Artifacts

### Key Components Implemented:
- **Authentication:** LoginView, LoginViewModel, SessionService, PasswordService
- **User Management:** UserManagementView, UserRepository, User.cs Model
- **Patient Management:** PatientManagement.xaml, PatientListItem.cs
- **Data Layer:** DatabaseService.cs, DatabaseInitializer.cs
- **Models:** User, Appointment, Billing, InventoryItem, ActivityLog, and Dashboard models
- **Security:** SHA256 token generation, password hashing, role-based access control

### Testing Artifacts:
- Unit tests for authentication and validation
- Integration tests for database operations
- UI tests for major workflows

---

## Compliance Summary

| Metric | Value |
|--------|-------|
| **Total Requirements** | 68 |
| **Met Requirements** | 68 |
| **Unmet Requirements** | 0 |
| **Completion Status** | 100% ✅ |

---

## Sign-off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Project Manager | | | 2026-05-22 |
| Lead Developer | | | 2026-05-22 |
| QA Lead | | | 2026-05-22 |
| Client/Stakeholder | | | |
