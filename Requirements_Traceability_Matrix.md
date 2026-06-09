# Requirements Traceability Matrix
## Clinic Management System

---

## Test Area Summary

| # | Test Area | Items | Met (Count) | Unmet (Count) | Remarks | Responsibility |
|---|-----------|-------|------------|--------------|---------|-----------------|
| 1 | Login | 8 | 8 | 0 | Done | Barredo |
| 2 | Forgot Password | 6 | 6 | 0 | Done | Barredo |
| 3 | User Management | 7 | 7 | 0 | Done | Barredo |
| 4 | Patient Management | 9 | 9 | 0 | Done | Barredo |
| 5 | Appointments | 9 | 9 | 0 | Done | Barredo |
| 6 | Billing | 10 | 10 | 0 | Done | Barredo |
| 7 | Inventory Management | 8 | 8 | 0 | Done | Barredo |
| 8 | Backup | 4 | 4 | 0 | Done | Barredo |
| 9 | Restore | 4 | 4 | 0 | Done | Barredo |
| 10 | Reports | 5 | 5 | 0 | Done | Barredo |
| 11 | Help | 3 | 3 | 0 | Done | Barredo |
| **TOTAL** | | **73** | **73** | **0** | | |

---

## Module 1: Login

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Login | 1.1 | Event Flow: User accesses login page → submits username and password → system validates credentials → generates SHA256 token → determines access level or displays error | Met | Done | Barredo |
| Login | 1.2 | Login page accepts input fields for username and password | Met | Done | Barredo |
| Login | 1.3 | Usernames limited to alphanumeric characters, maximum 16 characters | Met | Done | Barredo |
| Login | 1.4 | Passwords are case-sensitive with minimum 8 characters | Met | Done | Barredo |
| Login | 1.5 | Only registered user accounts can login; invalid credentials display error message: "Invalid Credentials" | Met | Done | Barredo |
| Login | 1.6 | Random unique token generated upon successful login with expiration time | Met | Done | Barredo |
| Login | 1.7 | Access level determined: Staff access (Patient Management, Dashboard, Appointment, Billing, Inventory, Help); Admin access (all staff + User Management, Reports, Maintenance) | Met | Done | Barredo |
| Login | 1.8 | Successful login redirects to Dashboard page | Met | Done | Barredo |

---

## Module 2: Forgot Password

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Forgot Password | 2.1 | User can access Forgot Password page from login screen | Met | Done | Barredo |
| Forgot Password | 2.2 | User enters username to initiate password recovery | Met | Done | Barredo |
| Forgot Password | 2.3 | System verifies username exists in database | Met | Done | Barredo |
| Forgot Password | 2.4 | System prompts user with security questions | Met | Done | Barredo |
| Forgot Password | 2.5 | User answers security questions correctly to proceed | Met | Done | Barredo |
| Forgot Password | 2.6 | Upon verification, user redirected to Create New Password page | Met | Done | Barredo |

---

## Module 3: User Management

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| User Management | 3.1 | Admin can view list of all registered users | Met | Done | Barredo |
| User Management | 3.2 | Admin can add new user accounts with username, password, and role assignment | Met | Done | Barredo |
| User Management | 3.3 | Admin can update existing user information (username, role, status) | Met | Done | Barredo |
| User Management | 3.4 | User passwords stored securely using encryption/hashing | Met | Done | Barredo |
| User Management | 3.5 | System validates new username for uniqueness and format compliance | Met | Done | Barredo |
| User Management | 3.6 | Admin can deactivate/activate user accounts | Met | Done | Barredo |
| User Management | 3.7 | Changes to user accounts are logged in Activity Log | Met | Done | Barredo |

---

## Module 4: Patient Management

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Patient Management | 4.1 | Staff can view list of all registered patients | Met | Done | Barredo |
| Patient Management | 4.2 | Staff can register new patient with name, contact, medical history | Met | Done | Barredo |
| Patient Management | 4.3 | Staff can search/filter patients by name, ID, contact number | Met | Done | Barredo |
| Patient Management | 4.4 | Staff can view complete patient profile and medical records | Met | Done | Barredo |
| Patient Management | 4.5 | Staff can update patient information (contact, address, emergency contact) | Met | Done | Barredo |
| Patient Management | 4.6 | System maintains patient confidentiality and access control | Met | Done | Barredo |
| Patient Management | 4.7 | Patient records include appointment history and billing information | Met | Done | Barredo |
| Patient Management | 4.8 | Changes to patient records are logged in Activity Log | Met | Done | Barredo |
| Patient Management | 4.9 | System supports patient deactivation (archive) | Met | Done | Barredo |

---

## Module 5: Appointments

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Appointments | 5.1 | Staff can create scheduled appointment with patient, date/time, dentist, and services | Met | Done | Barredo |
| Appointments | 5.2 | Staff can create walk-in appointment with patient and services | Met | Done | Barredo |
| Appointments | 5.3 | System displays appointment calendar view with all scheduled appointments | Met | Done | Barredo |
| Appointments | 5.4 | Staff can view appointment queue/status (pending, confirmed, completed, no-show) | Met | Done | Barredo |
| Appointments | 5.5 | System prevents double-booking of appointments (same time/dentist) | Met | Done | Barredo |
| Appointments | 5.6 | Staff can reschedule existing appointments to new date/time | Met | Done | Barredo |
| Appointments | 5.7 | Staff can cancel appointments with reason documentation | Met | Done | Barredo |
| Appointments | 5.8 | System automatically links appointments to patient and billing records | Met | Done | Barredo |
| Appointments | 5.9 | System captures treatment details and follow-up dates for completed appointments | Met | Done | Barredo |

---

## Module 6: Billing

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Billing | 6.1 | Staff can create billing record linked to patient appointment | Met | Done | Barredo |
| Billing | 6.2 | System calculates total cost based on services provided and quantities | Met | Done | Barredo |
| Billing | 6.3 | Staff can apply discounts/adjustments to billing (Senior Citizen 15%, PWD 15%) | Met | Done | Barredo |
| Billing | 6.4 | System tracks payment status (paid, pending, overdue) | Met | Done | Barredo |
| Billing | 6.5 | Staff can generate and print invoices/receipts for patient | Met | Done | Barredo |
| Billing | 6.6 | System supports multiple payment methods (Cash, Check, Card) | Met | Done | Barredo |
| Billing | 6.7 | Billing records are archived and accessible for audit purposes | Met | Done | Barredo |
| Billing | 6.8 | System applies VAT exemption for Senior Citizen and PWD discounts | Met | Done | Barredo |
| Billing | 6.9 | Staff can record partial payments and track remaining balance | Met | Done | Barredo |
| Billing | 6.10 | System generates daily collection summary (payments count and total cash) | Met | Done | Barredo |

---

## Module 7: Inventory Management

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Inventory Management | 7.1 | Admin can view inventory list with item name, quantity, unit cost | Met | Done | Barredo |
| Inventory Management | 7.2 | Admin can add new inventory items with details | Met | Done | Barredo |
| Inventory Management | 7.3 | Admin can update inventory quantity and pricing | Met | Done | Barredo |
| Inventory Management | 7.4 | System tracks low stock items and alerts when below threshold | Met | Done | Barredo |
| Inventory Management | 7.5 | Admin can set minimum stock level for automatic reorder alerts | Met | Done | Barredo |
| Inventory Management | 7.6 | System logs inventory movements and adjustments with user and timestamp | Met | Done | Barredo |
| Inventory Management | 7.7 | Staff can view inventory for billing purposes (read-only access) | Met | Done | Barredo |
| Inventory Management | 7.8 | System supports inventory categorization and search functionality | Met | Done | Barredo |

---

## Module 8: Backup

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Backup | 8.1 | Admin can configure primary and secondary backup locations | Met | Done | Barredo |
| Backup | 8.2 | System can create manual backup of entire database | Met | Done | Barredo |
| Backup | 8.3 | System maintains backup history with dates, sizes, and retention policy | Met | Done | Barredo |
| Backup | 8.4 | System automatically removes old backups based on retention policy | Met | Done | Barredo |

---

## Module 9: Restore

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Restore | 9.1 | Admin can browse and select backup files for restoration | Met | Done | Barredo |
| Restore | 9.2 | System can restore database from selected backup file | Met | Done | Barredo |
| Restore | 9.3 | System displays backup history with file details (date, size, location) | Met | Done | Barredo |
| Restore | 9.4 | System confirms successful restore and logs restoration activity | Met | Done | Barredo |

---

## Module 10: Reports

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Reports | 10.1 | System can generate patient list report with demographic information | Met | Done | Barredo |
| Reports | 10.2 | System can generate appointment report (daily, weekly, monthly view) | Met | Done | Barredo |
| Reports | 10.3 | System can generate billing/revenue report with payment status breakdown | Met | Done | Barredo |
| Reports | 10.4 | System can generate inventory report with current stock levels and valuations | Met | Done | Barredo |
| Reports | 10.5 | System can export reports in multiple formats (PDF, Excel, CSV) | Met | Done | Barredo |

---

## Module 11: Help

| Module | R# | Requirements | Status | Remarks | Responsibility |
|--------|----|----|--------|---------|-----------------|
| Help | 11.1 | Help module provides user documentation and system guides | Met | Done | Barredo |
| Help | 11.2 | Help module includes FAQ section with common questions and answers | Met | Done | Barredo |
| Help | 11.3 | Users can search help documentation by keyword or topic | Met | Done | Barredo |

---

## Compliance Summary

| Metric | Value |
|--------|-------|
| **Total Requirements** | 73 |
| **Met Requirements** | 73 |
| **Unmet Requirements** | 0 |
| **Completion Status** | 100% ✅ |

---

## Sign-off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Project Manager | | | |
| Lead Developer | Augustine Barredo | | 2026-06-09 |
| QA Lead | | | |
| Client/Stakeholder | | | |

---

