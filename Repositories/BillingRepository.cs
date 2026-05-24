using CruzNeryClinic.Data;
using CruzNeryClinic.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace CruzNeryClinic.Repositories
{
    public class BillingRepository
    {
        #region Appointment Payment

        public List<AppointmentPaymentItem> GetUnbilledCompletedTreatments()
        {
            List<AppointmentPaymentItem> items = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    tr.TreatmentRecordId,
    tr.AppointmentId,
    tr.PatientId,
    p.PatientCode,
    p.FirstName,
    p.MiddleName,
    p.LastName,
    CASE
        WHEN p.IsPWD = 1 AND p.IsSeniorCitizen = 1 THEN 'PWD / Senior'
        WHEN p.IsPWD = 1 THEN 'PWD'
        WHEN p.IsSeniorCitizen = 1 THEN 'Senior Citizen'
        ELSE 'Regular'
    END AS Category,
    tr.ServiceId,
    tr.ServiceName,
    tr.DentistName,
    tr.TreatmentDate,
    tr.TreatmentTime,
    COALESCE(s.DefaultPrice, 0) AS DefaultPrice
FROM TreatmentRecords tr
INNER JOIN Patients p
    ON tr.PatientId = p.PatientId
LEFT JOIN Services s
    ON tr.ServiceId = s.ServiceId
LEFT JOIN BillingTransactions bt
    ON bt.TreatmentRecordId = tr.TreatmentRecordId
WHERE bt.BillingId IS NULL
ORDER BY tr.TreatmentDate DESC, tr.TreatmentTime DESC, tr.TreatmentRecordId DESC;";

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new AppointmentPaymentItem
                {
                    TreatmentRecordId = Convert.ToInt32(reader["TreatmentRecordId"]),
                    AppointmentId = SafeGetNullableInt(reader, "AppointmentId"),
                    PatientId = Convert.ToInt32(reader["PatientId"]),
                    PatientCode = SafeGetString(reader, "PatientCode"),
                    PatientName = BuildFullName(
                        SafeGetString(reader, "FirstName"),
                        SafeGetString(reader, "MiddleName"),
                        SafeGetString(reader, "LastName")
                    ),
                    Category = SafeGetString(reader, "Category", "Regular"),
                    ServiceId = SafeGetNullableInt(reader, "ServiceId"),
                    ServiceName = SafeGetString(reader, "ServiceName"),
                    DentistName = SafeGetString(reader, "DentistName", "Unassigned"),
                    TreatmentDate = ParseDate(SafeGetString(reader, "TreatmentDate")),
                    TreatmentTime = ParseNullableTime(SafeGetString(reader, "TreatmentTime")),
                    DefaultPrice = Convert.ToDecimal(reader["DefaultPrice"])
                });
            }

            return items;
        }

        #endregion

        #region Balance Payment

        public List<BalancePaymentItem> GetBillingsWithBalance()
        {
            List<BalancePaymentItem> items = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    bt.BillingId,
    bt.PatientId,
    p.PatientCode,
    p.FirstName,
    p.MiddleName,
    p.LastName,
    bt.ReceiptNumber,
    bt.ServiceName,
    bt.TotalAmount,
    bt.AmountPaid,
    bt.RemainingBalance,
    bt.PaymentStatus,
    bt.TransactionDate
FROM BillingTransactions bt
INNER JOIN Patients p
    ON bt.PatientId = p.PatientId
WHERE bt.RemainingBalance > 0
  AND bt.PaymentStatus IN ('Unpaid', 'Partial')
ORDER BY bt.TransactionDate DESC, bt.BillingId DESC;";

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new BalancePaymentItem
                {
                    BillingId = Convert.ToInt32(reader["BillingId"]),
                    PatientId = Convert.ToInt32(reader["PatientId"]),
                    PatientCode = SafeGetString(reader, "PatientCode"),
                    PatientName = BuildFullName(
                        SafeGetString(reader, "FirstName"),
                        SafeGetString(reader, "MiddleName"),
                        SafeGetString(reader, "LastName")
                    ),
                    ReceiptNumber = SafeGetString(reader, "ReceiptNumber"),
                    ServiceName = SafeGetString(reader, "ServiceName"),
                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                    AmountPaid = Convert.ToDecimal(reader["AmountPaid"]),
                    RemainingBalance = Convert.ToDecimal(reader["RemainingBalance"]),
                    PaymentStatus = SafeGetString(reader, "PaymentStatus"),
                    TransactionDate = ParseDate(SafeGetString(reader, "TransactionDate"))
                });
            }

            return items;
        }

        #endregion

        #region Billing Records

        public List<BillingRecordListItem> GetBillingRecords()
        {
            List<BillingRecordListItem> items = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    bt.BillingId,
    bt.PatientId,
    p.PatientCode,
    p.FirstName,
    p.MiddleName,
    p.LastName,
    bt.ReceiptNumber,
    bt.BillingSource,
    bt.ServiceName,
    bt.TotalAmount,
    bt.DiscountAmount,
    bt.SubtotalAfterDiscount,
    bt.AmountPaid,
    bt.RemainingBalance,
    bt.PaymentStatus,
    bt.TransactionDate
FROM BillingTransactions bt
INNER JOIN Patients p
    ON bt.PatientId = p.PatientId
ORDER BY bt.TransactionDate DESC, bt.BillingId DESC;";

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new BillingRecordListItem
                {
                    BillingId = Convert.ToInt32(reader["BillingId"]),
                    PatientId = Convert.ToInt32(reader["PatientId"]),
                    PatientCode = SafeGetString(reader, "PatientCode"),
                    PatientName = BuildFullName(
                        SafeGetString(reader, "FirstName"),
                        SafeGetString(reader, "MiddleName"),
                        SafeGetString(reader, "LastName")
                    ),
                    ReceiptNumber = SafeGetString(reader, "ReceiptNumber"),
                    BillingSource = SafeGetString(reader, "BillingSource"),
                    ServiceName = SafeGetString(reader, "ServiceName"),
                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                    DiscountAmount = Convert.ToDecimal(reader["DiscountAmount"]),
                    SubtotalAfterDiscount = Convert.ToDecimal(reader["SubtotalAfterDiscount"]),
                    AmountPaid = Convert.ToDecimal(reader["AmountPaid"]),
                    RemainingBalance = Convert.ToDecimal(reader["RemainingBalance"]),
                    PaymentStatus = SafeGetString(reader, "PaymentStatus"),
                    TransactionDate = ParseDate(SafeGetString(reader, "TransactionDate"))
                });
            }

            return items;
        }

        #endregion

        #region Billing Receipt Details
        public BillingReceiptDetail? GetBillingReceiptDetail(int billingId)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT
            bt.BillingId,
            bt.PatientId,
            bt.AppointmentId,
            bt.TreatmentRecordId,
            bt.ReceiptNumber,
            bt.BillingSource,
            bt.ServiceName,
            bt.Description,
            bt.TotalAmount,
            bt.DiscountType,
            bt.DiscountAmount,
            bt.SubtotalAfterDiscount,
            bt.AmountPaid,
            bt.RemainingBalance,
            bt.PaymentStatus,
            bt.TransactionDate,
            bt.Notes,

            p.PatientCode,
            p.FirstName,
            p.MiddleName,
            p.LastName,
            CASE
                WHEN COALESCE(p.IsPWD, 0) = 1 AND COALESCE(p.IsSeniorCitizen, 0) = 1 THEN 'PWD / Senior'
                WHEN COALESCE(p.IsPWD, 0) = 1 THEN 'PWD'
                WHEN COALESCE(p.IsSeniorCitizen, 0) = 1 THEN 'Senior Citizen'
                ELSE 'Regular'
            END AS PatientCategory,

            COALESCE((
                SELECT pr.PaymentMethod
                FROM PaymentRecords pr
                WHERE pr.BillingId = bt.BillingId
                ORDER BY pr.PaymentDate DESC, pr.PaymentRecordId DESC
                LIMIT 1
            ), 'Cash') AS PaymentMethod,

            (
                SELECT pr.PaymentDate
                FROM PaymentRecords pr
                WHERE pr.BillingId = bt.BillingId
                ORDER BY pr.PaymentDate DESC, pr.PaymentRecordId DESC
                LIMIT 1
            ) AS LatestPaymentDate,

            COALESCE((
                SELECT SUM(pr.AmountPaid)
                FROM PaymentRecords pr
                WHERE pr.BillingId = bt.BillingId
            ), 0) AS ActualAmountPaid

        FROM BillingTransactions bt
        INNER JOIN Patients p
            ON p.PatientId = bt.PatientId
        WHERE bt.BillingId = @BillingId
        LIMIT 1;";

            command.Parameters.AddWithValue("@BillingId", billingId);

            using SqliteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            string firstName = SafeGetString(reader, "FirstName");
            string middleName = SafeGetString(reader, "MiddleName");
            string lastName = SafeGetString(reader, "LastName");

            return new BillingReceiptDetail
            {
                BillingId = Convert.ToInt32(reader["BillingId"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                AppointmentId = SafeGetNullableInt(reader, "AppointmentId"),
                TreatmentRecordId = SafeGetNullableInt(reader, "TreatmentRecordId"),

                ReceiptNumber = SafeGetString(reader, "ReceiptNumber"),
                BillingSource = SafeGetString(reader, "BillingSource"),
                PatientCode = SafeGetString(reader, "PatientCode"),
                PatientName = BuildFullName(firstName, middleName, lastName),
                PatientCategory = SafeGetString(reader, "PatientCategory", "Regular"),

                ServiceName = SafeGetString(reader, "ServiceName"),
                Description = SafeGetString(reader, "Description"),

                TotalAmount = SafeGetDecimal(reader, "TotalAmount"),
                DiscountType = SafeGetString(reader, "DiscountType", "None"),
                DiscountAmount = SafeGetDecimal(reader, "DiscountAmount"),
                SubtotalAfterDiscount = SafeGetDecimal(reader, "SubtotalAfterDiscount"),

                // Use actual SUM from PaymentRecords to be safer.
                AmountPaid = SafeGetDecimal(reader, "ActualAmountPaid"),

                RemainingBalance = SafeGetDecimal(reader, "RemainingBalance"),
                PaymentStatus = SafeGetString(reader, "PaymentStatus"),
                PaymentMethod = SafeGetString(reader, "PaymentMethod", "Cash"),

                TransactionDate = ParseDate(SafeGetString(reader, "TransactionDate")),
                LatestPaymentDate = ParseNullableDate(SafeGetString(reader, "LatestPaymentDate")),
                Notes = SafeGetString(reader, "Notes")
            };
        }


        #endregion

        #region Billing Patient Lookup
        public List<BillingPatientLookupItem> SearchPatientsForBillingHistory(string keyword)
        {
            List<BillingPatientLookupItem> patients = new();

            if (string.IsNullOrWhiteSpace(keyword))
                return patients;

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT DISTINCT
            p.PatientId,
            p.PatientCode,
            p.FirstName,
            p.MiddleName,
            p.LastName,
            CASE
                WHEN COALESCE(p.IsPWD, 0) = 1 AND COALESCE(p.IsSeniorCitizen, 0) = 1 THEN 'PWD / Senior'
                WHEN COALESCE(p.IsPWD, 0) = 1 THEN 'PWD'
                WHEN COALESCE(p.IsSeniorCitizen, 0) = 1 THEN 'Senior Citizen'
                ELSE 'Regular'
            END AS Category
        FROM Patients p
        INNER JOIN BillingTransactions bt
            ON bt.PatientId = p.PatientId
        WHERE
            COALESCE(p.IsActive, 1) = 1
            AND (
                p.PatientCode LIKE @Keyword
                OR p.FirstName LIKE @Keyword
                OR p.MiddleName LIKE @Keyword
                OR p.LastName LIKE @Keyword
                OR (p.LastName || ', ' || p.FirstName) LIKE @Keyword
                OR (p.FirstName || ' ' || p.LastName) LIKE @Keyword
            )
        ORDER BY p.LastName, p.FirstName
        LIMIT 8;";

            command.Parameters.AddWithValue("@Keyword", $"%{keyword}%");

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                patients.Add(new BillingPatientLookupItem
                {
                    PatientId = Convert.ToInt32(reader["PatientId"]),
                    PatientCode = SafeGetString(reader, "PatientCode"),
                    PatientName = BuildFullName(
                        SafeGetString(reader, "FirstName"),
                        SafeGetString(reader, "MiddleName"),
                        SafeGetString(reader, "LastName")
                    ),
                    Category = SafeGetString(reader, "Category", "Regular")
                });
            }

            return patients;
        }
        public List<BillingRecordListItem> GetBillingRecordsByPatientId(int patientId)
        {
            List<BillingRecordListItem> records = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT
            bt.BillingId,
            bt.PatientId,
            p.PatientCode,
            p.FirstName,
            p.MiddleName,
            p.LastName,
            bt.ReceiptNumber,
            bt.BillingSource,
            bt.ServiceName,
            bt.TotalAmount,
            bt.DiscountAmount,
            bt.SubtotalAfterDiscount,
            bt.AmountPaid,
            bt.RemainingBalance,
            bt.PaymentStatus,
            bt.TransactionDate
        FROM BillingTransactions bt
        INNER JOIN Patients p
            ON bt.PatientId = p.PatientId
        WHERE bt.PatientId = @PatientId
        ORDER BY bt.TransactionDate DESC, bt.BillingId DESC;";

            command.Parameters.AddWithValue("@PatientId", patientId);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                records.Add(new BillingRecordListItem
                {
                    BillingId = Convert.ToInt32(reader["BillingId"]),
                    PatientId = Convert.ToInt32(reader["PatientId"]),
                    PatientCode = SafeGetString(reader, "PatientCode"),
                    PatientName = BuildFullName(
                        SafeGetString(reader, "FirstName"),
                        SafeGetString(reader, "MiddleName"),
                        SafeGetString(reader, "LastName")
                    ),
                    ReceiptNumber = SafeGetString(reader, "ReceiptNumber"),
                    BillingSource = SafeGetString(reader, "BillingSource"),
                    ServiceName = SafeGetString(reader, "ServiceName"),
                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                    DiscountAmount = Convert.ToDecimal(reader["DiscountAmount"]),
                    SubtotalAfterDiscount = Convert.ToDecimal(reader["SubtotalAfterDiscount"]),
                    AmountPaid = Convert.ToDecimal(reader["AmountPaid"]),
                    RemainingBalance = Convert.ToDecimal(reader["RemainingBalance"]),
                    PaymentStatus = SafeGetString(reader, "PaymentStatus"),
                    TransactionDate = ParseDate(SafeGetString(reader, "TransactionDate"))
                });
            }

            return records;
        }
        #endregion

        #region Create Billing

        public int CreateBillingTransaction(BillingTransaction billing)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO BillingTransactions (
    PatientId,
    AppointmentId,
    TreatmentRecordId,
    BillingSource,
    ReceiptNumber,
    ServiceId,
    ServiceName,
    Description,
    TotalAmount,
    DiscountType,
    DiscountAmount,
    SubtotalAfterDiscount,
    AmountPaid,
    RemainingBalance,
    PaymentStatus,
    TransactionDate,
    CreatedByUserId,
    Notes,
    CreatedAt
)
VALUES (
    @PatientId,
    @AppointmentId,
    @TreatmentRecordId,
    @BillingSource,
    @ReceiptNumber,
    @ServiceId,
    @ServiceName,
    @Description,
    @TotalAmount,
    @DiscountType,
    @DiscountAmount,
    @SubtotalAfterDiscount,
    @AmountPaid,
    @RemainingBalance,
    @PaymentStatus,
    @TransactionDate,
    @CreatedByUserId,
    @Notes,
    @CreatedAt
);

SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@PatientId", billing.PatientId);
            command.Parameters.AddWithValue("@AppointmentId", billing.AppointmentId.HasValue ? billing.AppointmentId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@TreatmentRecordId", billing.TreatmentRecordId.HasValue ? billing.TreatmentRecordId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@BillingSource", billing.BillingSource);
            command.Parameters.AddWithValue("@ReceiptNumber", billing.ReceiptNumber);
            command.Parameters.AddWithValue("@ServiceId", billing.ServiceId.HasValue ? billing.ServiceId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ServiceName", billing.ServiceName);
            command.Parameters.AddWithValue("@Description", billing.Description);
            command.Parameters.AddWithValue("@TotalAmount", billing.TotalAmount);
            command.Parameters.AddWithValue("@DiscountType", billing.DiscountType);
            command.Parameters.AddWithValue("@DiscountAmount", billing.DiscountAmount);
            command.Parameters.AddWithValue("@SubtotalAfterDiscount", billing.SubtotalAfterDiscount);
            command.Parameters.AddWithValue("@AmountPaid", billing.AmountPaid);
            command.Parameters.AddWithValue("@RemainingBalance", billing.RemainingBalance);
            command.Parameters.AddWithValue("@PaymentStatus", billing.PaymentStatus);
            command.Parameters.AddWithValue("@TransactionDate", billing.TransactionDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@CreatedByUserId", billing.CreatedByUserId.HasValue ? billing.CreatedByUserId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Notes", billing.Notes);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            return Convert.ToInt32(command.ExecuteScalar());
        }

        #endregion

        #region Payments

        public void AddPaymentRecord(PaymentRecord payment)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqliteCommand insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = @"
INSERT INTO PaymentRecords (
    BillingId,
    PatientId,
    AmountPaid,
    PaymentMethod,
    PaymentDate,
    ReceivedByUserId,
    Notes,
    CreatedAt
)
VALUES (
    @BillingId,
    @PatientId,
    @AmountPaid,
    @PaymentMethod,
    @PaymentDate,
    @ReceivedByUserId,
    @Notes,
    @CreatedAt
);";

                insertCommand.Parameters.AddWithValue("@BillingId", payment.BillingId);
                insertCommand.Parameters.AddWithValue("@PatientId", payment.PatientId);
                insertCommand.Parameters.AddWithValue("@AmountPaid", payment.AmountPaid);
                insertCommand.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                insertCommand.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate.ToString("yyyy-MM-dd"));
                insertCommand.Parameters.AddWithValue("@ReceivedByUserId", payment.ReceivedByUserId.HasValue ? payment.ReceivedByUserId.Value : DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Notes", payment.Notes);
                insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                insertCommand.ExecuteNonQuery();

                using SqliteCommand updateCommand = connection.CreateCommand();
                updateCommand.Transaction = transaction;
                updateCommand.CommandText = @"
UPDATE BillingTransactions
SET
    AmountPaid = AmountPaid + @AmountPaid,
    RemainingBalance = CASE
        WHEN RemainingBalance - @AmountPaid < 0 THEN 0
        ELSE RemainingBalance - @AmountPaid
    END,
    PaymentStatus = CASE
        WHEN RemainingBalance - @AmountPaid <= 0 THEN 'Paid'
        ELSE 'Partial'
    END,
    UpdatedAt = @UpdatedAt
WHERE BillingId = @BillingId;";

                updateCommand.Parameters.AddWithValue("@AmountPaid", payment.AmountPaid);
                updateCommand.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                updateCommand.Parameters.AddWithValue("@BillingId", payment.BillingId);

                updateCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion

        #region Helpers

        public string GenerateReceiptNumber()
        {
            return $"CN-{DateTime.Now:yyyyMMdd-HHmmss}";
        }

        public string DeterminePaymentStatus(decimal netAmount, decimal amountPaid)
        {
            if (amountPaid <= 0)
                return "Unpaid";

            if (amountPaid >= netAmount)
                return "Paid";

            return "Partial";
        }

        public decimal CalculateBalance(decimal netAmount, decimal amountPaid)
        {
            decimal balance = netAmount - amountPaid;
            return balance < 0 ? 0 : balance;
        }

        private string BuildFullName(string firstName, string middleName, string lastName)
        {
            string middle = string.IsNullOrWhiteSpace(middleName)
                ? string.Empty
                : $" {middleName.Trim()}";

            return $"{firstName.Trim()}{middle} {lastName.Trim()}".Trim();
        }

        private string SafeGetString(SqliteDataReader reader, string columnName, string fallback = "")
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
                return fallback;

            return reader.GetString(ordinal);
        }

        private int? SafeGetNullableInt(SqliteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
                return null;

            return Convert.ToInt32(reader[columnName]);
        }

        private DateTime ParseDate(string? value)
        {
            return DateTime.TryParse(value, out DateTime date)
                ? date
                : DateTime.Today;
        }

        private DateTime? ParseNullableDate(string? value)
        {
            return DateTime.TryParse(value, out DateTime date)
                ? date
                : null;
        }

        private TimeSpan? ParseNullableTime(string? value)
        {
            return TimeSpan.TryParse(value, out TimeSpan time)
                ? time
                : null;
        }

        private static decimal SafeGetDecimal(SqliteDataReader reader, string columnName, decimal fallback = 0)
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
                return fallback;

            return Convert.ToDecimal(reader.GetValue(ordinal));
        }

        #endregion
    }
}