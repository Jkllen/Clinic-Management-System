using CruzNeryClinic.Data;
using CruzNeryClinic.Models.Dashboard;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace CruzNeryClinic.Repositories
{
    // DashboardRepository gets summary data for the Dashboard screen.
    // It reads from multiple tables: Patients, Appointments, BillingTransactions,
    // InventoryItems, and ActivityLogs.
    public class DashboardRepository
    {
        public DashboardSummary GetDashboardSummary()
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            return new DashboardSummary
            {
                TotalPatients = Count(connection, "SELECT COUNT(*) FROM Patients WHERE IsActive = 1;"),

                NewPatientsThisMonth = Count(
                    connection,
                    @"
                    SELECT COUNT(*)
                    FROM Patients
                    WHERE IsActive = 1
                    AND strftime('%Y-%m', CreatedAt) = strftime('%Y-%m', 'now', 'localtime');"
                ),

                PendingPayments = Count(
                    connection,
                    @"
                    SELECT COUNT(*)
                    FROM BillingTransactions
                    WHERE PaymentStatus IN ('Unpaid', 'Partial');"
                ),

                TotalUnpaidBalance = GetDecimal(
                    connection,
                    @"
                    SELECT IFNULL(SUM(RemainingBalance), 0)
                    FROM BillingTransactions
                    WHERE PaymentStatus IN ('Unpaid', 'Partial');"
                ),

                LowStockItemCount = Count(
                    connection,
                    @"
                    SELECT COUNT(*)
                    FROM InventoryItems
                    WHERE IsActive = 1
                    AND Quantity <= MinimumThreshold;"
                )
            };
        }

        public List<DashboardLowStockItem> GetLowStockItems(int limit = 3)
        {
            List<DashboardLowStockItem> items = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT ItemName, Quantity
FROM InventoryItems
WHERE IsActive = 1
AND Quantity <= MinimumThreshold
ORDER BY Quantity ASC, ItemName ASC
LIMIT @Limit;";

            command.Parameters.AddWithValue("@Limit", limit);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new DashboardLowStockItem
                {
                    ItemName = reader["ItemName"].ToString() ?? string.Empty,
                    QuantityLeft = Convert.ToInt32(reader["Quantity"])
                });
            }

            return items;
        }

        public List<DashboardActivityItem> GetRecentActivities(int limit = 2)
        {
            List<DashboardActivityItem> activities = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT Action, Module, Description, CreatedAt
FROM ActivityLogs
ORDER BY CreatedAt DESC
LIMIT @Limit;";

            command.Parameters.AddWithValue("@Limit", limit);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                DateTime createdAt = DateTime.Parse(reader["CreatedAt"].ToString()!);

                activities.Add(new DashboardActivityItem
                {
                    Action = reader["Action"].ToString() ?? string.Empty,
                    Module = reader["Module"].ToString() ?? string.Empty,
                    Description = reader["Description"].ToString() ?? string.Empty,
                    Time = createdAt.ToString("hh:mm tt")
                });
            }

            return activities;
        }

        // Gets today's appointment queue.
        // This is used for the Queue table by default.
        public List<DashboardQueueItem> GetTodayQueue()
        {
            return GetAppointmentsByDate(DateTime.Today);
        }

        // Gets appointments for a specific selected date.
        // This is used by the dashboard calendar.
        public List<DashboardQueueItem> GetAppointmentsByDate(DateTime selectedDate)
        {
            List<DashboardQueueItem> queue = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT
            a.QueueNumber,
            a.AppointmentTime,
            a.AppointmentType,
            a.ServiceName,
            a.Status,
            p.PatientCode,
            p.FirstName,
            p.MiddleName,
            p.LastName
        FROM Appointments a
        INNER JOIN Patients p ON a.PatientId = p.PatientId
        WHERE a.AppointmentDate = @SelectedDate
        AND a.Status IN ('Waiting', 'In Treatment', 'Completed')
        ORDER BY 
            CASE WHEN a.Priority = 'High' THEN 0 ELSE 1 END,
            a.QueueNumber ASC,
            a.AppointmentTime ASC;";

            command.Parameters.AddWithValue("@SelectedDate", selectedDate.ToString("yyyy-MM-dd"));

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string firstName = reader["FirstName"].ToString() ?? string.Empty;
                string middleName = reader["MiddleName"].ToString() ?? string.Empty;
                string lastName = reader["LastName"].ToString() ?? string.Empty;

                queue.Add(new DashboardQueueItem
                {
                    QueueNumber = reader["QueueNumber"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QueueNumber"]),
                    Time = reader["AppointmentTime"].ToString() ?? string.Empty,
                    PatientCode = reader["PatientCode"].ToString() ?? string.Empty,
                    PatientName = FormatPatientName(firstName, middleName, lastName),
                    AppointmentType = reader["AppointmentType"].ToString() ?? string.Empty,
                    Treatment = reader["ServiceName"].ToString() ?? string.Empty,
                    Status = reader["Status"].ToString() ?? string.Empty
                });
            }

            return queue;
        }

        // Formats patient name as: LastName, FirstName M.
        private string FormatPatientName(string firstName, string middleName, string lastName)
        {
            string middleInitial = string.IsNullOrWhiteSpace(middleName)
                ? string.Empty
                : $" {middleName.Trim()[0]}.";

            return $"{lastName}, {firstName}{middleInitial}";
        }
        public List<DashboardTransactionItem> GetRecentPatientTransactions(int limit = 5)
        {
            List<DashboardTransactionItem> transactions = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    b.TransactionDate,
    b.ServiceName,
    b.AmountPaid,
    b.PaymentStatus,
    p.FirstName,
    p.MiddleName,
    p.LastName
FROM BillingTransactions b
INNER JOIN Patients p ON b.PatientId = p.PatientId
ORDER BY b.TransactionDate DESC
LIMIT @Limit;";

            command.Parameters.AddWithValue("@Limit", limit);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                DateTime transactionDate = DateTime.Parse(reader["TransactionDate"].ToString()!);

                string firstName = reader["FirstName"].ToString() ?? string.Empty;
                string middleName = reader["MiddleName"].ToString() ?? string.Empty;
                string lastName = reader["LastName"].ToString() ?? string.Empty;

                string patientName = string.IsNullOrWhiteSpace(middleName)
                    ? $"{lastName}, {firstName}"
                    : $"{lastName}, {firstName} {middleName[0]}.";

                transactions.Add(new DashboardTransactionItem
                {
                    Time = transactionDate.ToString("hh:mm tt"),
                    PatientName = patientName,
                    Service = reader["ServiceName"].ToString() ?? string.Empty,
                    Amount = Convert.ToDecimal(reader["AmountPaid"]),
                    PaymentStatus = reader["PaymentStatus"].ToString() ?? string.Empty
                });
            }

            return transactions;
        }

        private int Count(SqliteConnection connection, string sql)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;

            long count = (long)command.ExecuteScalar()!;
            return (int)count;
        }

        private decimal GetDecimal(SqliteConnection connection, string sql)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;

            object? result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                return 0;

            return Convert.ToDecimal(result);
        }
    }
}