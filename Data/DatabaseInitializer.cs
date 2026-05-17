using CruzNeryClinic.Services;
using Microsoft.Data.Sqlite;
using System;

namespace CruzNeryClinic.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            EnableForeignKeys(connection);
            CreateTables(connection);
            // Security questions must be seeded before admin accounts,
            // because Users now store SecurityQuestionId1, 2, and 3.
            SeedDefaultSecurityQuestions(connection);

            SeedDefaultAdminAccounts(connection);
            SeedDefaultServices(connection);
        }

        private static void EnableForeignKeys(SqliteConnection connection)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();
        }

        private static void CreateTables(SqliteConnection connection)
        {
            // This SQL block creates all tables needed by the clinic system.
            // The database is created only if the tables do not exist yet.

            string sql = @"
CREATE TABLE IF NOT EXISTS Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT,

    -- UserCode is the visible ID shown in the UI, example: 2026-001.
    UserCode TEXT NOT NULL UNIQUE,

    FirstName TEXT NOT NULL,
    MiddleName TEXT,
    LastName TEXT NOT NULL,

    ContactNumber TEXT,
    Username TEXT NOT NULL UNIQUE,

    -- Password is not stored directly.
    -- Only the SHA-256 hash and salt are stored.
    PasswordHash TEXT NOT NULL,
    PasswordSalt TEXT NOT NULL,

    -- Supported roles based on the system design.
    Role TEXT NOT NULL CHECK(Role IN ('Admin', 'Dentist', 'Secretary', 'Dental Assistant')),

    -- Dynamic security question IDs from the SecurityQuestions table.
    SecurityQuestionId1 INTEGER NOT NULL,
    SecurityAnswerHash1 TEXT NOT NULL,
    SecurityAnswerSalt1 TEXT NOT NULL,

    SecurityQuestionId2 INTEGER NOT NULL,
    SecurityAnswerHash2 TEXT NOT NULL,
    SecurityAnswerSalt2 TEXT NOT NULL,

    SecurityQuestionId3 INTEGER NOT NULL,
    SecurityAnswerHash3 TEXT NOT NULL,
    SecurityAnswerSalt3 TEXT NOT NULL,

    -- IsActive is used instead of deleting users permanently.
    -- 1 = active, 0 = archived/deactivated.
    IsActive INTEGER NOT NULL DEFAULT 1,

    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    LastLoginAt TEXT,

    FOREIGN KEY (SecurityQuestionId1) REFERENCES SecurityQuestions(SecurityQuestionId),
    FOREIGN KEY (SecurityQuestionId2) REFERENCES SecurityQuestions(SecurityQuestionId),
    FOREIGN KEY (SecurityQuestionId3) REFERENCES SecurityQuestions(SecurityQuestionId)
);

CREATE TABLE IF NOT EXISTS SecurityQuestions (
    SecurityQuestionId INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Question text shown in registration dropdowns and forgot password screen.
    QuestionText TEXT NOT NULL UNIQUE,

    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Patients (
    PatientId INTEGER PRIMARY KEY AUTOINCREMENT,

    -- PatientCode is the visible ID shown in the UI, example: P001.
    PatientCode TEXT NOT NULL UNIQUE,

    FirstName TEXT NOT NULL,
    MiddleName TEXT,
    LastName TEXT NOT NULL,

    PhoneNumber TEXT NOT NULL,
    BirthDate TEXT NOT NULL,
    Gender TEXT NOT NULL CHECK(Gender IN ('Male', 'Female', 'Other')),

    Address TEXT,

    -- Used for priority and discount computation.
    IsPWD INTEGER NOT NULL DEFAULT 0,
    IsSeniorCitizen INTEGER NOT NULL DEFAULT 0,

    -- Initial service/treatment shown in Patient Management list.
    InitialTreatment TEXT,

    IsActive INTEGER NOT NULL DEFAULT 1,

    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT
);

CREATE TABLE IF NOT EXISTS PatientHistories (
    PatientHistoryId INTEGER PRIMARY KEY AUTOINCREMENT,

    PatientId INTEGER NOT NULL UNIQUE,

    -- Text areas for now. These can be encrypted later using AES-GCM.
    DentalHistory TEXT,
    MedicalHistory TEXT,
    AllergyMedicationNotes TEXT,

    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,

    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId)
);

CREATE TABLE IF NOT EXISTS Services (
    ServiceId INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Examples: Prophylaxis, Restoration/Pasta, Extraction, Orthodontics, TMJ, Dentures.
    ServiceName TEXT NOT NULL UNIQUE,

    DefaultPrice REAL NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,

    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT
);

CREATE TABLE IF NOT EXISTS Appointments (
    AppointmentId INTEGER PRIMARY KEY AUTOINCREMENT,

    PatientId INTEGER NOT NULL,

    -- Walk-In or Scheduled based on your appointment UI.
    AppointmentType TEXT NOT NULL CHECK(AppointmentType IN ('Walk-In', 'Scheduled')),

    -- Regular, PWD, or Senior.
    Category TEXT NOT NULL DEFAULT 'Regular' CHECK(Category IN ('Regular', 'PWD', 'Senior')),

    ServiceId INTEGER,
    ServiceName TEXT NOT NULL,

    -- Dentist can be assigned or unassigned.
    DentistUserId INTEGER,
    DentistName TEXT DEFAULT 'Unassigned',

    AppointmentDate TEXT NOT NULL,
    AppointmentTime TEXT,

    -- Used for queue display.
    QueueNumber INTEGER,

    -- High is usually for PWD/Senior or scheduled priority.
    Priority TEXT NOT NULL DEFAULT 'Normal' CHECK(Priority IN ('Normal', 'High')),

    -- Used by appointment table and billing flow.
    Status TEXT NOT NULL DEFAULT 'Waiting'
        CHECK(Status IN ('Waiting', 'In Treatment', 'Completed', 'Cancelled', 'No Show')),

    Notes TEXT,

    CreatedByUserId INTEGER,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,

    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId),
    FOREIGN KEY (DentistUserId) REFERENCES Users(UserId),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS BillingTransactions (
    BillingId INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Billing is usually created from a completed appointment.
    PatientId INTEGER NOT NULL,
    AppointmentId INTEGER,

    ReceiptNumber TEXT NOT NULL UNIQUE,

    ServiceName TEXT NOT NULL,

    TotalAmount REAL NOT NULL DEFAULT 0,
    DiscountType TEXT NOT NULL DEFAULT 'None',
    DiscountAmount REAL NOT NULL DEFAULT 0,

    -- SubtotalAfterDiscount = TotalAmount - DiscountAmount.
    SubtotalAfterDiscount REAL NOT NULL DEFAULT 0,

    AmountPaid REAL NOT NULL DEFAULT 0,
    RemainingBalance REAL NOT NULL DEFAULT 0,

    PaymentMethod TEXT NOT NULL DEFAULT 'Cash',

    -- Supports installment/partial payments.
    PaymentStatus TEXT NOT NULL DEFAULT 'Unpaid'
        CHECK(PaymentStatus IN ('Unpaid', 'Partial', 'Paid')),

    TransactionDate TEXT NOT NULL,

    CreatedByUserId INTEGER,
    Notes TEXT,

    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,

    FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS PaymentRecords (
    PaymentRecordId INTEGER PRIMARY KEY AUTOINCREMENT,

    -- One billing transaction can have many payment records.
    -- This is important for partial payments/installments.
    BillingId INTEGER NOT NULL,

    AmountPaid REAL NOT NULL,
    PaymentMethod TEXT NOT NULL DEFAULT 'Cash',
    PaymentDate TEXT NOT NULL,

    ReceivedByUserId INTEGER,
    Notes TEXT,

    CreatedAt TEXT NOT NULL,

    FOREIGN KEY (BillingId) REFERENCES BillingTransactions(BillingId),
    FOREIGN KEY (ReceivedByUserId) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS InventoryItems (
    ItemId INTEGER PRIMARY KEY AUTOINCREMENT,

    ItemName TEXT NOT NULL UNIQUE,
    Category TEXT,

    Quantity INTEGER NOT NULL DEFAULT 0,
    Unit TEXT,

    -- If Quantity is less than or equal to this value, item is low stock.
    MinimumThreshold INTEGER NOT NULL DEFAULT 0,

    LastRestockDate TEXT,
    ExpirationDate TEXT,

    Notes TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,

    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT
);

CREATE TABLE IF NOT EXISTS InventoryUsage (
    UsageId INTEGER PRIMARY KEY AUTOINCREMENT,

    ItemId INTEGER NOT NULL,
    AppointmentId INTEGER,

    QuantityUsed INTEGER NOT NULL,
    UsedByUserId INTEGER,
    UsageDate TEXT NOT NULL,

    Notes TEXT,
    CreatedAt TEXT NOT NULL,

    FOREIGN KEY (ItemId) REFERENCES InventoryItems(ItemId),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId),
    FOREIGN KEY (UsedByUserId) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS InventoryRestocks (
    RestockId INTEGER PRIMARY KEY AUTOINCREMENT,

    ItemId INTEGER NOT NULL,

    QuantityAdded INTEGER NOT NULL,
    RestockDate TEXT NOT NULL,
    RestockedByUserId INTEGER,

    Notes TEXT,
    CreatedAt TEXT NOT NULL,

    FOREIGN KEY (ItemId) REFERENCES InventoryItems(ItemId),
    FOREIGN KEY (RestockedByUserId) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS ActivityLogs (
    LogId INTEGER PRIMARY KEY AUTOINCREMENT,

    UserId INTEGER,
    UserCode TEXT,
    Username TEXT,

    -- Examples: Login, Add, Update, Archive, Print, Backup, Restore.
    Action TEXT NOT NULL,

    -- Examples: Users, Patients, Appointment, Billing, Inventory, Maintenance.
    Module TEXT NOT NULL,

    Description TEXT,
    CreatedAt TEXT NOT NULL,

    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS BackupRecords (
    BackupId INTEGER PRIMARY KEY AUTOINCREMENT,

    BackupFileName TEXT NOT NULL,
    BackupPath TEXT NOT NULL,

    -- Manual for now. Automatic can be added later.
    BackupType TEXT NOT NULL DEFAULT 'Manual',

    -- Backups will be encrypted using AES-GCM later.
    IsEncrypted INTEGER NOT NULL DEFAULT 1,

    CreatedByUserId INTEGER,
    CreatedAt TEXT NOT NULL,

    FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId)
);

-- Indexes make searching and dashboard loading faster.
CREATE INDEX IF NOT EXISTS idx_users_code ON Users(UserCode);
CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_role ON Users(Role);

CREATE INDEX IF NOT EXISTS idx_security_questions_active ON SecurityQuestions(IsActive);

CREATE INDEX IF NOT EXISTS idx_patients_code ON Patients(PatientCode);
CREATE INDEX IF NOT EXISTS idx_patients_name ON Patients(LastName, FirstName);
CREATE INDEX IF NOT EXISTS idx_patients_pwd_senior ON Patients(IsPWD, IsSeniorCitizen);
CREATE INDEX IF NOT EXISTS idx_patients_created_active ON Patients(CreatedAt, IsActive);
CREATE INDEX IF NOT EXISTS idx_patients_contact ON Patients(PhoneNumber);
CREATE INDEX IF NOT EXISTS idx_patient_histories_patient ON PatientHistories(PatientId);


CREATE INDEX IF NOT EXISTS idx_appointments_date ON Appointments(AppointmentDate);
CREATE INDEX IF NOT EXISTS idx_appointments_status ON Appointments(Status);
CREATE INDEX IF NOT EXISTS idx_appointments_patient ON Appointments(PatientId);

CREATE INDEX IF NOT EXISTS idx_billing_patient ON BillingTransactions(PatientId);
CREATE INDEX IF NOT EXISTS idx_billing_status ON BillingTransactions(PaymentStatus);
CREATE INDEX IF NOT EXISTS idx_billing_receipt ON BillingTransactions(ReceiptNumber);

CREATE INDEX IF NOT EXISTS idx_inventory_item_name ON InventoryItems(ItemName);
CREATE INDEX IF NOT EXISTS idx_activity_created ON ActivityLogs(CreatedAt);
";

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        private static void SeedDefaultSecurityQuestions(SqliteConnection connection)
        {
            // These are the selectable security question options.
            // User Registration will load these into dropdown boxes.

            SeedSecurityQuestion(connection, "What was the name of your first school?");
            SeedSecurityQuestion(connection, "What was the name of your first pet?");
            SeedSecurityQuestion(connection, "What city were you born in?");
            SeedSecurityQuestion(connection, "What is your mother’s maiden name?");
            SeedSecurityQuestion(connection, "What was your childhood nickname?");
            SeedSecurityQuestion(connection, "What is the name of your favorite teacher?");
            SeedSecurityQuestion(connection, "What was your first phone brand?");
            SeedSecurityQuestion(connection, "What is your favorite food?");
            SeedSecurityQuestion(connection, "What is the name of your best friend?");
            SeedSecurityQuestion(connection, "What barangay did you grow up in?");
        }

        private static void SeedSecurityQuestion(SqliteConnection connection, string questionText)
        {
            // Prevent duplicate question records.
            using SqliteCommand checkCommand = connection.CreateCommand();
            checkCommand.CommandText = @"
        SELECT COUNT(*)
        FROM SecurityQuestions
        WHERE QuestionText = @QuestionText;";

            checkCommand.Parameters.AddWithValue("@QuestionText", questionText);

            long existingCount = (long)checkCommand.ExecuteScalar()!;

            if (existingCount > 0)
                return;

            using SqliteCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
        INSERT INTO SecurityQuestions (
            QuestionText,
            IsActive,
            CreatedAt
        )
        VALUES (
            @QuestionText,
            1,
            @CreatedAt
        );";

            insertCommand.Parameters.AddWithValue("@QuestionText", questionText);
            insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            insertCommand.ExecuteNonQuery();
        }

        private static int GetSecurityQuestionId(SqliteConnection connection, string questionText)
        {
            // Gets the ID of a seeded security question.
            // This lets seeded users use the dynamic SecurityQuestions table.
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT SecurityQuestionId
        FROM SecurityQuestions
        WHERE QuestionText = @QuestionText
        LIMIT 1;";

            command.Parameters.AddWithValue("@QuestionText", questionText);

            object? result = command.ExecuteScalar();

            if (result == null)
                throw new InvalidOperationException($"Security question was not found: {questionText}");

            return Convert.ToInt32(result);
        }
        private static void SeedDefaultAdminAccounts(SqliteConnection connection)
        {
            // Get dynamic question IDs from the SecurityQuestions table.
            // This means seeded users no longer store hardcoded question text inside Users.
            int schoolQuestionId = GetSecurityQuestionId(connection, "What was the name of your first school?");
            int petQuestionId = GetSecurityQuestionId(connection, "What was the name of your first pet?");
            int cityQuestionId = GetSecurityQuestionId(connection, "What city were you born in?");

            int motherQuestionId = GetSecurityQuestionId(connection, "What is your mother’s maiden name?");
            int teacherQuestionId = GetSecurityQuestionId(connection, "What is the name of your favorite teacher?");
            int foodQuestionId = GetSecurityQuestionId(connection, "What is your favorite food?");

            // Clinic admin accounts still need temporary security answers,
            // because forgot password cannot work without stored answer hashes.
            // These are unique temporary answers and should be changed before deployment.

            SeedUser(
                connection,
                userCode: "2026-001",
                firstName: "Clinic",
                middleName: "",
                lastName: "Administrator One",
                username: "admin01",
                plainPassword: "AdminClinic@2025",
                role: "Admin",
                contactNumber: "N/A",

                securityQuestionId1: motherQuestionId,
                securityAnswer1: "admin01-mother",

                securityQuestionId2: teacherQuestionId,
                securityAnswer2: "admin01-teacher",

                securityQuestionId3: foodQuestionId,
                securityAnswer3: "admin01-food"
            );

            SeedUser(
                connection,
                userCode: "2026-002",
                firstName: "Clinic",
                middleName: "",
                lastName: "Administrator Two",
                username: "admin02",
                plainPassword: "AdminClinic2@2025",
                role: "Admin",
                contactNumber: "N/A",

                securityQuestionId1: motherQuestionId,
                securityAnswer1: "admin02-mother",

                securityQuestionId2: teacherQuestionId,
                securityAnswer2: "admin02-teacher",

                securityQuestionId3: foodQuestionId,
                securityAnswer3: "admin02-food"
            );

            // Dev account keeps simple known answers for testing only.
            SeedUser(
                connection,
                userCode: "DEV-001",
                firstName: "Augustine",
                middleName: "",
                lastName: "Barredo",
                username: "devAB",
                plainPassword: "PASS#12",
                role: "Admin",
                contactNumber: "N/A",

                securityQuestionId1: schoolQuestionId,
                securityAnswer1: "school",

                securityQuestionId2: petQuestionId,
                securityAnswer2: "pet",

                securityQuestionId3: cityQuestionId,
                securityAnswer3: "city"
            );

            SeedUser(
                connection,
                userCode: "DEV-002",
                firstName: "Lawrence",
                middleName: "",
                lastName: "Malaga",
                username: "devLM",
                plainPassword: "PASS#12",
                role: "Admin",
                contactNumber: "N/A",

                securityQuestionId1: schoolQuestionId,
                securityAnswer1: "school",

                securityQuestionId2: petQuestionId,
                securityAnswer2: "pet",

                securityQuestionId3: cityQuestionId,
                securityAnswer3: "city"
            );

            SeedUser(
                connection,
                userCode: "DEV-003",
                firstName: "Wenifredo",
                middleName: "",
                lastName: "De Lemos",
                username: "devWDL",
                plainPassword: "PASS#12",
                role: "Admin",
                contactNumber: "N/A",

                securityQuestionId1: schoolQuestionId,
                securityAnswer1: "school",

                securityQuestionId2: petQuestionId,
                securityAnswer2: "pet",

                securityQuestionId3: cityQuestionId,
                securityAnswer3: "city"
            );
        }

        private static void SeedUser(
            SqliteConnection connection,
            string userCode,
            string firstName,
            string middleName,
            string lastName,
            string username,
            string plainPassword,
            string role,
            string contactNumber,
            int securityQuestionId1,
            string securityAnswer1,
            int securityQuestionId2,
            string securityAnswer2,
            int securityQuestionId3,
            string securityAnswer3)
        {
            // Check first if the username already exists.
            // This prevents duplicate admin accounts every time the app starts.
            using SqliteCommand checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @Username;";
            checkCommand.Parameters.AddWithValue("@Username", username);

            long existingCount = (long)checkCommand.ExecuteScalar()!;

            if (existingCount > 0)
                return;

            // Generate password salt and hash.
            // The plain password is never stored in the database.
            string passwordSalt = PasswordService.GenerateSalt();
            string passwordHash = PasswordService.HashPassword(plainPassword, passwordSalt);

            // Generate separate salts for security answers.
            string answerSalt1 = PasswordService.GenerateSalt();
            string answerSalt2 = PasswordService.GenerateSalt();
            string answerSalt3 = PasswordService.GenerateSalt();

            // Store hashed answers only.
            // The plain security answers are never stored in the database.
            string answerHash1 = PasswordService.HashSecurityAnswer(securityAnswer1, answerSalt1);
            string answerHash2 = PasswordService.HashSecurityAnswer(securityAnswer2, answerSalt2);
            string answerHash3 = PasswordService.HashSecurityAnswer(securityAnswer3, answerSalt3);

            using SqliteCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
        INSERT INTO Users (
            UserCode,
            FirstName,
            MiddleName,
            LastName,
            ContactNumber,
            Username,
            PasswordHash,
            PasswordSalt,
            Role,

            SecurityQuestionId1,
            SecurityAnswerHash1,
            SecurityAnswerSalt1,

            SecurityQuestionId2,
            SecurityAnswerHash2,
            SecurityAnswerSalt2,

            SecurityQuestionId3,
            SecurityAnswerHash3,
            SecurityAnswerSalt3,

            IsActive,
            CreatedAt
        )
        VALUES (
            @UserCode,
            @FirstName,
            @MiddleName,
            @LastName,
            @ContactNumber,
            @Username,
            @PasswordHash,
            @PasswordSalt,
            @Role,

            @SecurityQuestionId1,
            @SecurityAnswerHash1,
            @SecurityAnswerSalt1,

            @SecurityQuestionId2,
            @SecurityAnswerHash2,
            @SecurityAnswerSalt2,

            @SecurityQuestionId3,
            @SecurityAnswerHash3,
            @SecurityAnswerSalt3,

            1,
            @CreatedAt
        );";

            insertCommand.Parameters.AddWithValue("@UserCode", userCode);
            insertCommand.Parameters.AddWithValue("@FirstName", firstName);
            insertCommand.Parameters.AddWithValue("@MiddleName", middleName);
            insertCommand.Parameters.AddWithValue("@LastName", lastName);
            insertCommand.Parameters.AddWithValue("@ContactNumber", contactNumber);
            insertCommand.Parameters.AddWithValue("@Username", username);
            insertCommand.Parameters.AddWithValue("@PasswordHash", passwordHash);
            insertCommand.Parameters.AddWithValue("@PasswordSalt", passwordSalt);
            insertCommand.Parameters.AddWithValue("@Role", role);

            insertCommand.Parameters.AddWithValue("@SecurityQuestionId1", securityQuestionId1);
            insertCommand.Parameters.AddWithValue("@SecurityAnswerHash1", answerHash1);
            insertCommand.Parameters.AddWithValue("@SecurityAnswerSalt1", answerSalt1);

            insertCommand.Parameters.AddWithValue("@SecurityQuestionId2", securityQuestionId2);
            insertCommand.Parameters.AddWithValue("@SecurityAnswerHash2", answerHash2);
            insertCommand.Parameters.AddWithValue("@SecurityAnswerSalt2", answerSalt2);

            insertCommand.Parameters.AddWithValue("@SecurityQuestionId3", securityQuestionId3);
            insertCommand.Parameters.AddWithValue("@SecurityAnswerHash3", answerHash3);
            insertCommand.Parameters.AddWithValue("@SecurityAnswerSalt3", answerSalt3);

            insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            insertCommand.ExecuteNonQuery();
        }
        private static void SeedDefaultServices(SqliteConnection connection)
        {
            // These services are based on the clinic services from the interview/documentation.
            // Prices can be updated later through the system if needed.

            SeedService(connection, "Prophylaxis", 0);
            SeedService(connection, "Restoration / Pasta", 0);
            SeedService(connection, "Extraction", 0);
            SeedService(connection, "Orthodontics", 0);
            SeedService(connection, "TMJ", 0);
            SeedService(connection, "Dentures", 0);
            SeedService(connection, "Consultation", 0);
            SeedService(connection, "Cleaning", 0);
        }

        private static void SeedService(SqliteConnection connection, string serviceName, double defaultPrice)
        {
            // Prevent duplicate service names.
            using SqliteCommand checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Services WHERE ServiceName = @ServiceName;";
            checkCommand.Parameters.AddWithValue("@ServiceName", serviceName);

            long existingCount = (long)checkCommand.ExecuteScalar()!;

            if (existingCount > 0)
                return;

            using SqliteCommand insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
INSERT INTO Services (
    ServiceName,
    DefaultPrice,
    IsActive,
    CreatedAt
)
VALUES (
    @ServiceName,
    @DefaultPrice,
    1,
    @CreatedAt
);";

            insertCommand.Parameters.AddWithValue("@ServiceName", serviceName);
            insertCommand.Parameters.AddWithValue("@DefaultPrice", defaultPrice);
            insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            insertCommand.ExecuteNonQuery();
        }
    }
}