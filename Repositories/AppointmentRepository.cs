using CruzNeryClinic.Data;
using CruzNeryClinic.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using CruzNeryClinic.Services;
using System.Text;

namespace CruzNeryClinic.Repositories
{
    public class AppointmentRepository
    {
        #region Appointment List

        public List<AppointmentListItem> GetAppointmentListItems()
        {
            List<AppointmentListItem> appointments = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    a.AppointmentId,
    a.PatientId,
    p.PatientCode,
    p.FirstName,
    p.MiddleName,
    p.LastName,
    a.AppointmentType,
    a.Category,
    a.ServiceId,
    a.ServiceName,
    a.DentistUserId,
    a.DentistName,
    a.AppointmentDate,
    a.AppointmentTime,
    a.ArrivalTime,
    a.QueueNumber,
    a.IsUrgent,
    a.Priority,
    a.Status,
    a.Notes,
    a.StartedAt,
    a.CompletedAt,
    a.CancelledAt
FROM Appointments a
INNER JOIN Patients p
    ON a.PatientId = p.PatientId
ORDER BY
    a.AppointmentDate ASC,
    CASE
        WHEN a.IsUrgent = 1 THEN 0
        WHEN a.AppointmentType = 'Scheduled' THEN 1
        ELSE 2
    END ASC,
    a.AppointmentTime ASC,
    a.AppointmentId ASC;";

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
                appointments.Add(MapReaderToAppointmentListItem(reader));

            return appointments;
        }

        #endregion

        #region Summary Counts

        public int GetTodayAppointmentCount()
        {
            return CountByQuery(@"
SELECT COUNT(*)
FROM Appointments
WHERE AppointmentDate = @Today
  AND Status IN ('Scheduled', 'Waiting', 'In Treatment');");
        }

        public int GetCompletedTodayCount()
        {
            return CountByQuery(@"
SELECT COUNT(*)
FROM Appointments
WHERE AppointmentDate = @Today
  AND Status = 'Completed';");
        }

        public int GetCancelledTodayCount()
        {
            return CountByQuery(@"
SELECT COUNT(*)
FROM Appointments
WHERE AppointmentDate = @Today
  AND Status = 'Cancelled';");
        }

        private int CountByQuery(string sql)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Today", DateTime.Today.ToString("yyyy-MM-dd"));

            return Convert.ToInt32(command.ExecuteScalar());
        }

        #endregion

        #region Patient Search

        public List<AppointmentPatientSearchItem> SearchActivePatients(string searchText)
        {
            List<AppointmentPatientSearchItem> patients = new();

            if (string.IsNullOrWhiteSpace(searchText))
                return patients;

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    PatientId,
    PatientCode,
    FirstName,
    MiddleName,
    LastName,
    BirthDate,
    IsPWD,
    IsSeniorCitizen
FROM Patients
WHERE IsActive = 1
  AND (
        PatientCode LIKE @SearchText
        OR FirstName LIKE @SearchText
        OR MiddleName LIKE @SearchText
        OR LastName LIKE @SearchText
        OR (FirstName || ' ' || LastName) LIKE @SearchText
        OR (FirstName || ' ' || MiddleName || ' ' || LastName) LIKE @SearchText
  )
ORDER BY LastName ASC, FirstName ASC
LIMIT 10;";

            command.Parameters.AddWithValue("@SearchText", $"%{searchText.Trim()}%");

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                patients.Add(new AppointmentPatientSearchItem
                {
                    PatientId = Convert.ToInt32(reader["PatientId"]),
                    PatientCode = reader["PatientCode"]?.ToString() ?? string.Empty,
                    FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                    MiddleName = reader["MiddleName"]?.ToString() ?? string.Empty,
                    LastName = reader["LastName"]?.ToString() ?? string.Empty,
                    BirthDate = ParseDate(reader["BirthDate"]?.ToString()),
                    IsPwd = Convert.ToInt32(reader["IsPWD"]) == 1,
                    IsSeniorCitizen = Convert.ToInt32(reader["IsSeniorCitizen"]) == 1
                });
            }

            return patients;
        }

        #endregion

        #region Calendar

        public Dictionary<DateTime, int> GetAppointmentCountsByDate(int year, int month)
        {
            Dictionary<DateTime, int> counts = new();

            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1);

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT
            AppointmentDate,
            COUNT(*) AS AppointmentCount
        FROM Appointments
        WHERE AppointmentDate >= @StartDate
        AND AppointmentDate < @EndDate
        AND Status IN ('Scheduled', 'Waiting', 'In Treatment')
        GROUP BY AppointmentDate;";

            command.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                DateTime date = ParseDate(reader["AppointmentDate"]?.ToString());
                int count = Convert.ToInt32(reader["AppointmentCount"]);

                counts[date.Date] = count;
            }

            return counts;
        }

        #endregion

        #region Dropdown Options

        public List<AppointmentServiceOption> GetActiveServices()
        {
            List<AppointmentServiceOption> services = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    ServiceId,
    ServiceName,
    DefaultPrice
FROM Services
WHERE IsActive = 1
ORDER BY ServiceName ASC;";

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                services.Add(new AppointmentServiceOption
                {
                    ServiceId = Convert.ToInt32(reader["ServiceId"]),
                    ServiceName = reader["ServiceName"]?.ToString() ?? string.Empty,
                    DefaultPrice = Convert.ToDouble(reader["DefaultPrice"])
                });
            }

            return services;
        }

        public List<AppointmentDentistOption> GetActiveDentists()
        {
            List<AppointmentDentistOption> dentists = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    UserId,
    FirstName,
    MiddleName,
    LastName
FROM Users
WHERE IsActive = 1
  AND Role = 'Dentist'
ORDER BY LastName ASC, FirstName ASC;";

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string firstName = reader["FirstName"]?.ToString() ?? string.Empty;
                string middleName = reader["MiddleName"]?.ToString() ?? string.Empty;
                string lastName = reader["LastName"]?.ToString() ?? string.Empty;

                string fullName = string.IsNullOrWhiteSpace(middleName)
                    ? $"{firstName} {lastName}".Trim()
                    : $"{firstName} {middleName} {lastName}".Trim();

                dentists.Add(new AppointmentDentistOption
                {
                    DentistUserId = Convert.ToInt32(reader["UserId"]),
                    DentistName = fullName
                });
            }

            return dentists;
        }

        #endregion

        #region Patient Medical Alert

        public AppointmentPatientMedicalAlert GetPatientMedicalAlert(int patientId)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT
            HasMedicalCondition,
            MedicalConditionNotes,
            AllergyNotes,
            CurrentMedication,
            RequiresMedicalClearance,
            ClearanceNotes
        FROM PatientHistories
        WHERE PatientId = @PatientId
        LIMIT 1;";

            command.Parameters.AddWithValue("@PatientId", patientId);

            using SqliteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return new AppointmentPatientMedicalAlert();

            return new AppointmentPatientMedicalAlert
            {
                HasMedicalCondition = SafeGetInt(reader, "HasMedicalCondition") == 1,
                MedicalConditionNotes = CryptoService.DecryptString(SafeGetString(reader, "MedicalConditionNotes")),
                AllergyNotes = CryptoService.DecryptString(SafeGetString(reader, "AllergyNotes")),
                CurrentMedication = CryptoService.DecryptString(SafeGetString(reader, "CurrentMedication")),
                RequiresMedicalClearance = SafeGetInt(reader, "RequiresMedicalClearance") == 1,
                ClearanceNotes = CryptoService.DecryptString(SafeGetString(reader, "ClearanceNotes"))
            };
        }

        #endregion

        #region Appointment Conflict Checks

        public bool HasActiveScheduledAppointmentAtExactTime(
            DateTime appointmentDate,
            TimeSpan appointmentTime,
            int? ignoredAppointmentId = null)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText = @"
        SELECT COUNT(*)
        FROM Appointments
        WHERE AppointmentDate = @AppointmentDate
        AND AppointmentTime = @AppointmentTime
        AND AppointmentType = 'Scheduled'
        AND Status IN ('Scheduled', 'Waiting', 'In Treatment')
        AND (@IgnoredAppointmentId IS NULL OR AppointmentId <> @IgnoredAppointmentId);";

            command.Parameters.AddWithValue("@AppointmentDate", appointmentDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@AppointmentTime", appointmentTime.ToString(@"hh\:mm"));
            command.Parameters.AddWithValue("@IgnoredAppointmentId", ignoredAppointmentId.HasValue ? ignoredAppointmentId.Value : DBNull.Value);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public bool HasSamePatientActiveAppointmentAtExactTime(
            int patientId,
            DateTime appointmentDate,
            TimeSpan appointmentTime,
            int? ignoredAppointmentId = null)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText = @"
        SELECT COUNT(*)
        FROM Appointments
        WHERE PatientId = @PatientId
        AND AppointmentDate = @AppointmentDate
        AND AppointmentTime = @AppointmentTime
        AND Status IN ('Scheduled', 'Waiting', 'In Treatment')
        AND (@IgnoredAppointmentId IS NULL OR AppointmentId <> @IgnoredAppointmentId);";

            command.Parameters.AddWithValue("@PatientId", patientId);
            command.Parameters.AddWithValue("@AppointmentDate", appointmentDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@AppointmentTime", appointmentTime.ToString(@"hh\:mm"));
            command.Parameters.AddWithValue("@IgnoredAppointmentId", ignoredAppointmentId.HasValue ? ignoredAppointmentId.Value : DBNull.Value);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public bool HasSamePatientActiveAppointmentOnSameDate(
            int patientId,
            DateTime appointmentDate,
            TimeSpan appointmentTime,
            int? ignoredAppointmentId = null)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText = @"
        SELECT COUNT(*)
        FROM Appointments
        WHERE PatientId = @PatientId
        AND AppointmentDate = @AppointmentDate
        AND AppointmentTime <> @AppointmentTime
        AND Status IN ('Scheduled', 'Waiting', 'In Treatment')
        AND (@IgnoredAppointmentId IS NULL OR AppointmentId <> @IgnoredAppointmentId);";

            command.Parameters.AddWithValue("@PatientId", patientId);
            command.Parameters.AddWithValue("@AppointmentDate", appointmentDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@AppointmentTime", appointmentTime.ToString(@"hh\:mm"));
            command.Parameters.AddWithValue("@IgnoredAppointmentId", ignoredAppointmentId.HasValue ? ignoredAppointmentId.Value : DBNull.Value);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        #endregion

        #region Add Appointment

        public int AddAppointment(Appointment appointment)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO Appointments (
    PatientId,
    AppointmentType,
    Category,
    ServiceId,
    ServiceName,
    DentistUserId,
    DentistName,
    AppointmentDate,
    AppointmentTime,
    ArrivalTime,
    IsUrgent,
    Priority,
    Status,
    Notes,
    CreatedByUserId,
    CreatedAt
)
VALUES (
    @PatientId,
    @AppointmentType,
    @Category,
    @ServiceId,
    @ServiceName,
    @DentistUserId,
    @DentistName,
    @AppointmentDate,
    @AppointmentTime,
    @ArrivalTime,
    @IsUrgent,
    @Priority,
    @Status,
    @Notes,
    @CreatedByUserId,
    @CreatedAt
);

SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@PatientId", appointment.PatientId);
            command.Parameters.AddWithValue("@AppointmentType", appointment.AppointmentType);
            command.Parameters.AddWithValue("@Category", appointment.Category);
            command.Parameters.AddWithValue("@ServiceId", appointment.ServiceId.HasValue ? appointment.ServiceId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ServiceName", appointment.ServiceName);
            command.Parameters.AddWithValue("@DentistUserId", appointment.DentistUserId.HasValue ? appointment.DentistUserId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@DentistName", appointment.DentistName);
            command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime.ToString(@"hh\:mm"));
            command.Parameters.AddWithValue("@ArrivalTime", appointment.ArrivalTime.HasValue ? appointment.ArrivalTime.Value.ToString(@"hh\:mm") : DBNull.Value);
            command.Parameters.AddWithValue("@IsUrgent", appointment.IsUrgent ? 1 : 0);
            command.Parameters.AddWithValue("@Priority", appointment.Priority);
            command.Parameters.AddWithValue("@Status", appointment.Status);
            command.Parameters.AddWithValue("@Notes", appointment.Notes.Trim());
            command.Parameters.AddWithValue("@CreatedByUserId", appointment.CreatedByUserId.HasValue ? appointment.CreatedByUserId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            return Convert.ToInt32(command.ExecuteScalar());
        }

        #endregion

        #region Appointment Details

        public AppointmentListItem? GetAppointmentListItemById(int appointmentId)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT
            a.AppointmentId,
            a.PatientId,
            p.PatientCode,
            p.FirstName,
            p.MiddleName,
            p.LastName,
            a.AppointmentType,
            a.Category,
            a.ServiceId,
            a.ServiceName,
            a.DentistUserId,
            a.DentistName,
            a.AppointmentDate,
            a.AppointmentTime,
            a.ArrivalTime,
            a.QueueNumber,
            a.IsUrgent,
            a.Priority,
            a.Status,
            a.Notes,
            a.StartedAt,
            a.CompletedAt,
            a.CancelledAt
        FROM Appointments a
        INNER JOIN Patients p
            ON a.PatientId = p.PatientId
        WHERE a.AppointmentId = @AppointmentId
        LIMIT 1;";

            command.Parameters.AddWithValue("@AppointmentId", appointmentId);

            using SqliteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return MapReaderToAppointmentListItem(reader);
        }

        #endregion

        #region Reschedule Appointment

        public void RescheduleAppointment(Appointment appointment)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        UPDATE Appointments
        SET
            ServiceId = @ServiceId,
            ServiceName = @ServiceName,
            DentistUserId = @DentistUserId,
            DentistName = @DentistName,
            AppointmentDate = @AppointmentDate,
            AppointmentTime = @AppointmentTime,
            Notes = @Notes,
            Status = 'Scheduled',
            Priority = 'Scheduled',
            ArrivalTime = NULL,
            IsUrgent = 0,
            UpdatedAt = @UpdatedAt
        WHERE AppointmentId = @AppointmentId;";

            command.Parameters.AddWithValue("@ServiceId", appointment.ServiceId.HasValue ? appointment.ServiceId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ServiceName", appointment.ServiceName);
            command.Parameters.AddWithValue("@DentistUserId", appointment.DentistUserId.HasValue ? appointment.DentistUserId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@DentistName", appointment.DentistName);
            command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime.ToString(@"hh\:mm"));
            command.Parameters.AddWithValue("@Notes", appointment.Notes.Trim());
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@AppointmentId", appointment.AppointmentId);

            command.ExecuteNonQuery();
        }

        #endregion

        #region Treatment Records

        public bool TreatmentRecordExistsForAppointment(int appointmentId)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT COUNT(*)
        FROM TreatmentRecords
        WHERE AppointmentId = @AppointmentId;";

            command.Parameters.AddWithValue("@AppointmentId", appointmentId);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public void CreateTreatmentRecordFromAppointment(int appointmentId, string treatmentNotes)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqliteCommand existsCommand = connection.CreateCommand();
                existsCommand.Transaction = transaction;
                existsCommand.CommandText = @"
        SELECT COUNT(*)
        FROM TreatmentRecords
        WHERE AppointmentId = @AppointmentId;";

                existsCommand.Parameters.AddWithValue("@AppointmentId", appointmentId);

                int existingCount = Convert.ToInt32(existsCommand.ExecuteScalar());

                if (existingCount > 0)
                {
                    transaction.Commit();
                    return;
                }

                using SqliteCommand insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = @"
        INSERT INTO TreatmentRecords (
            PatientId,
            AppointmentId,
            ServiceId,
            ServiceName,
            DentistUserId,
            DentistName,
            TreatmentDate,
            TreatmentTime,
            TreatmentNotes,
            CreatedAt
        )
        SELECT
            PatientId,
            AppointmentId,
            ServiceId,
            ServiceName,
            DentistUserId,
            DentistName,
            AppointmentDate,
            AppointmentTime,
            @TreatmentNotes,
            @CreatedAt
        FROM Appointments
        WHERE AppointmentId = @AppointmentId;";

                insertCommand.Parameters.AddWithValue("@AppointmentId", appointmentId);
                insertCommand.Parameters.AddWithValue("@TreatmentNotes", treatmentNotes.Trim());
                insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                insertCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion


        #region Status Actions

        public void MarkArrived(int appointmentId)
        {
            UpdateStatus(
                appointmentId,
                status: "Waiting",
                extraSetSql: "ArrivalTime = @ArrivalTime, UpdatedAt = @UpdatedAt",
                configureParameters: command =>
                {
                    command.Parameters.AddWithValue("@ArrivalTime", DateTime.Now.ToString("HH:mm"));
                });
        }

        public void MarkNoShow(int appointmentId)
        {
            UpdateStatus(
                appointmentId,
                status: "No Show",
                extraSetSql: "UpdatedAt = @UpdatedAt",
                configureParameters: command =>
                {

            });
        } 

        public bool StartTreatment(int appointmentId)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqliteCommand checkCommand = connection.CreateCommand();
                checkCommand.Transaction = transaction;
                checkCommand.CommandText = @"
SELECT COUNT(*)
FROM Appointments
WHERE AppointmentDate = @Today
  AND Status = 'In Treatment'
  AND AppointmentId <> @AppointmentId;";

                checkCommand.Parameters.AddWithValue("@Today", DateTime.Today.ToString("yyyy-MM-dd"));
                checkCommand.Parameters.AddWithValue("@AppointmentId", appointmentId);

                int inTreatmentCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (inTreatmentCount > 0)
                {
                    transaction.Rollback();
                    return false;
                }

                using SqliteCommand updateCommand = connection.CreateCommand();
                updateCommand.Transaction = transaction;
                updateCommand.CommandText = @"
UPDATE Appointments
SET
    Status = 'In Treatment',
    StartedAt = @StartedAt,
    UpdatedAt = @UpdatedAt
WHERE AppointmentId = @AppointmentId;";

                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                updateCommand.Parameters.AddWithValue("@StartedAt", now);
                updateCommand.Parameters.AddWithValue("@UpdatedAt", now);
                updateCommand.Parameters.AddWithValue("@AppointmentId", appointmentId);

                updateCommand.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void CompleteAppointment(int appointmentId)
        {
            UpdateStatus(
                appointmentId,
                status: "Completed",
                extraSetSql: "CompletedAt = @CompletedAt, UpdatedAt = @UpdatedAt",
                configureParameters: command =>
                {
                    command.Parameters.AddWithValue("@CompletedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                });
        }

        public void CancelAppointment(int appointmentId, string cancellationReason = "")
        {
            UpdateStatus(
                appointmentId,
                status: "Cancelled",
                extraSetSql: "CancelledAt = @CancelledAt, CancellationReason = @CancellationReason, UpdatedAt = @UpdatedAt",
                configureParameters: command =>
                {
                    command.Parameters.AddWithValue("@CancelledAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@CancellationReason", cancellationReason.Trim());
                });
        }

        public void ToggleUrgent(int appointmentId, bool isUrgent)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
UPDATE Appointments
SET
    IsUrgent = @IsUrgent,
    Priority = @Priority,
    UpdatedAt = @UpdatedAt
WHERE AppointmentId = @AppointmentId;";

            command.Parameters.AddWithValue("@IsUrgent", isUrgent ? 1 : 0);
            command.Parameters.AddWithValue("@Priority", isUrgent ? "Urgent" : "Normal");
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);

            command.ExecuteNonQuery();
        }

        private void UpdateStatus(
            int appointmentId,
            string status,
            string extraSetSql,
            Action<SqliteCommand> configureParameters)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = $@"
UPDATE Appointments
SET
    Status = @Status,
    {extraSetSql}
WHERE AppointmentId = @AppointmentId;";

            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);

            configureParameters(command);

            command.ExecuteNonQuery();
        }

        #endregion

        #region Helpers

        private AppointmentListItem MapReaderToAppointmentListItem(SqliteDataReader reader)
        {
            string firstName = reader["FirstName"]?.ToString() ?? string.Empty;
            string middleName = reader["MiddleName"]?.ToString() ?? string.Empty;
            string lastName = reader["LastName"]?.ToString() ?? string.Empty;

            string patientName = string.IsNullOrWhiteSpace(middleName)
                ? $"{firstName} {lastName}".Trim()
                : $"{firstName} {middleName} {lastName}".Trim();

            return new AppointmentListItem
            {
                AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                PatientId = Convert.ToInt32(reader["PatientId"]),
                PatientCode = reader["PatientCode"]?.ToString() ?? string.Empty,
                PatientName = patientName,
                AppointmentType = reader["AppointmentType"]?.ToString() ?? string.Empty,
                Category = reader["Category"]?.ToString() ?? "Regular",
                ServiceId = SafeGetNullableInt(reader, "ServiceId"),
                ServiceName = reader["ServiceName"]?.ToString() ?? string.Empty,
                DentistUserId = SafeGetNullableInt(reader, "DentistUserId"),
                DentistName = reader["DentistName"]?.ToString() ?? "Unassigned",
                AppointmentDate = ParseDate(reader["AppointmentDate"]?.ToString()),
                AppointmentTime = ParseTime(reader["AppointmentTime"]?.ToString()),
                ArrivalTime = ParseNullableTime(reader["ArrivalTime"]?.ToString()),
                QueueNumber = SafeGetNullableInt(reader, "QueueNumber"),
                IsUrgent = Convert.ToInt32(reader["IsUrgent"]) == 1,
                Priority = reader["Priority"]?.ToString() ?? "Normal",
                Status = reader["Status"]?.ToString() ?? "Scheduled",
                Notes = reader["Notes"]?.ToString() ?? string.Empty,
                StartedAt = ParseNullableDateTime(reader["StartedAt"]?.ToString()),
                CompletedAt = ParseNullableDateTime(reader["CompletedAt"]?.ToString()),
                CancelledAt = ParseNullableDateTime(reader["CancelledAt"]?.ToString())
            };
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

        private TimeSpan ParseTime(string? value)
        {
            return TimeSpan.TryParse(value, out TimeSpan time)
                ? time
                : TimeSpan.Zero;
        }

        private TimeSpan? ParseNullableTime(string? value)
        {
            return TimeSpan.TryParse(value, out TimeSpan time)
                ? time
                : null;
        }

        private DateTime? ParseNullableDateTime(string? value)
        {
            return DateTime.TryParse(value, out DateTime dateTime)
                ? dateTime
                : null;
        }

        private string SafeGetString(SqliteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
                return string.Empty;

            return reader.GetString(ordinal);
        }

        private int SafeGetInt(SqliteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
                return 0;

            return Convert.ToInt32(reader[columnName]);
        }

        #endregion
    }
}