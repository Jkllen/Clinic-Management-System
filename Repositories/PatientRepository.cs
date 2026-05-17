using CruzNeryClinic.Data;
using CruzNeryClinic.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace CruzNeryClinic.Repositories
{
    public class PatientRepository
    {
        #region Patient List and Summary

        public List<PatientListItem> GetPatientListItems()
        {
            List<PatientListItem> patients = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    p.PatientId,
    p.PatientCode,
    p.FirstName,
    p.MiddleName,
    p.LastName,
    p.PhoneNumber,
    p.BirthDate,
    p.Gender,
    p.InitialTreatment,
    p.IsPWD,
    p.IsSeniorCitizen,
    p.IsActive,
    p.CreatedAt,

    CASE
        WHEN ph.PatientHistoryId IS NULL THEN 0
        ELSE 1
    END AS HasPatientHistory,

    CASE
        WHEN EXISTS (
            SELECT 1
            FROM BillingTransactions bt
            WHERE bt.PatientId = p.PatientId
              AND bt.RemainingBalance > 0
              AND bt.PaymentStatus <> 'Paid'
        ) THEN 1
        ELSE 0
    END AS HasBalance

FROM Patients p
LEFT JOIN PatientHistories ph
    ON p.PatientId = ph.PatientId
ORDER BY p.PatientId ASC;";

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                patients.Add(MapReaderToPatientListItem(reader));
            }

            return patients;
        }

        public int GetNewPatientsThisMonthCount()
        {
            DateTime now = DateTime.Now;
            DateTime monthStart = new(now.Year, now.Month, 1);
            DateTime nextMonthStart = monthStart.AddMonths(1);

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT COUNT(*)
FROM Patients
WHERE IsActive = 1
  AND CreatedAt >= @MonthStart
  AND CreatedAt < @NextMonthStart;";

            command.Parameters.AddWithValue("@MonthStart", monthStart.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@NextMonthStart", nextMonthStart.ToString("yyyy-MM-dd HH:mm:ss"));

            return Convert.ToInt32(command.ExecuteScalar());
        }

        #endregion

        #region Get Single Patient

        public Patient? GetPatientById(int patientId)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    p.PatientId,
    p.PatientCode,
    p.FirstName,
    p.MiddleName,
    p.LastName,
    p.PhoneNumber,
    p.BirthDate,
    p.Gender,
    p.Address,
    p.IsPWD,
    p.IsSeniorCitizen,
    p.InitialTreatment,
    p.IsActive,
    p.CreatedAt,
    p.UpdatedAt,

    ph.DentalHistory,
    ph.MedicalHistory,
    ph.AllergyMedicationNotes

FROM Patients p
LEFT JOIN PatientHistories ph
    ON p.PatientId = ph.PatientId
WHERE p.PatientId = @PatientId
LIMIT 1;";

            command.Parameters.AddWithValue("@PatientId", patientId);

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
                return MapReaderToPatient(reader);

            return null;
        }

        #endregion

        #region Add Patient

        public int AddPatient(Patient patient)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                string patientCode = GenerateNextPatientCode(connection, transaction);
                string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                using SqliteCommand insertPatientCommand = connection.CreateCommand();
                insertPatientCommand.Transaction = transaction;
                insertPatientCommand.CommandText = @"
INSERT INTO Patients (
    PatientCode,
    FirstName,
    MiddleName,
    LastName,
    PhoneNumber,
    BirthDate,
    Gender,
    Address,
    IsPWD,
    IsSeniorCitizen,
    InitialTreatment,
    IsActive,
    CreatedAt
)
VALUES (
    @PatientCode,
    @FirstName,
    @MiddleName,
    @LastName,
    @PhoneNumber,
    @BirthDate,
    @Gender,
    @Address,
    @IsPWD,
    @IsSeniorCitizen,
    @InitialTreatment,
    1,
    @CreatedAt
);

SELECT last_insert_rowid();";

                insertPatientCommand.Parameters.AddWithValue("@PatientCode", patientCode);
                insertPatientCommand.Parameters.AddWithValue("@FirstName", patient.FirstName.Trim());
                insertPatientCommand.Parameters.AddWithValue("@MiddleName", patient.MiddleName.Trim());
                insertPatientCommand.Parameters.AddWithValue("@LastName", patient.LastName.Trim());
                insertPatientCommand.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber.Trim());
                insertPatientCommand.Parameters.AddWithValue("@BirthDate", patient.BirthDate.ToString("yyyy-MM-dd"));
                insertPatientCommand.Parameters.AddWithValue("@Gender", patient.Gender.Trim());
                insertPatientCommand.Parameters.AddWithValue("@Address", patient.Address.Trim());
                insertPatientCommand.Parameters.AddWithValue("@IsPWD", patient.IsPwd ? 1 : 0);
                insertPatientCommand.Parameters.AddWithValue("@IsSeniorCitizen", patient.IsSeniorCitizen ? 1 : 0);
                insertPatientCommand.Parameters.AddWithValue("@InitialTreatment", patient.InitialTreatment.Trim());
                insertPatientCommand.Parameters.AddWithValue("@CreatedAt", createdAt);

                int patientId = Convert.ToInt32(insertPatientCommand.ExecuteScalar());

                using SqliteCommand insertHistoryCommand = connection.CreateCommand();
                insertHistoryCommand.Transaction = transaction;
                insertHistoryCommand.CommandText = @"
INSERT INTO PatientHistories (
    PatientId,
    DentalHistory,
    MedicalHistory,
    AllergyMedicationNotes,
    CreatedAt
)
VALUES (
    @PatientId,
    @DentalHistory,
    @MedicalHistory,
    @AllergyMedicationNotes,
    @CreatedAt
);";

                insertHistoryCommand.Parameters.AddWithValue("@PatientId", patientId);
                insertHistoryCommand.Parameters.AddWithValue("@DentalHistory", patient.DentalHistory.Trim());
                insertHistoryCommand.Parameters.AddWithValue("@MedicalHistory", patient.MedicalHistory.Trim());
                insertHistoryCommand.Parameters.AddWithValue("@AllergyMedicationNotes", patient.AllergyMedicationNotes.Trim());
                insertHistoryCommand.Parameters.AddWithValue("@CreatedAt", createdAt);

                insertHistoryCommand.ExecuteNonQuery();

                transaction.Commit();
                return patientId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion

        #region Update Patient

        public void UpdatePatient(Patient patient)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                string updatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                using SqliteCommand updatePatientCommand = connection.CreateCommand();
                updatePatientCommand.Transaction = transaction;
                updatePatientCommand.CommandText = @"
UPDATE Patients
SET
    FirstName = @FirstName,
    MiddleName = @MiddleName,
    LastName = @LastName,
    PhoneNumber = @PhoneNumber,
    BirthDate = @BirthDate,
    Gender = @Gender,
    Address = @Address,
    IsPWD = @IsPWD,
    IsSeniorCitizen = @IsSeniorCitizen,
    InitialTreatment = @InitialTreatment,
    UpdatedAt = @UpdatedAt
WHERE PatientId = @PatientId;";

                updatePatientCommand.Parameters.AddWithValue("@FirstName", patient.FirstName.Trim());
                updatePatientCommand.Parameters.AddWithValue("@MiddleName", patient.MiddleName.Trim());
                updatePatientCommand.Parameters.AddWithValue("@LastName", patient.LastName.Trim());
                updatePatientCommand.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber.Trim());
                updatePatientCommand.Parameters.AddWithValue("@BirthDate", patient.BirthDate.ToString("yyyy-MM-dd"));
                updatePatientCommand.Parameters.AddWithValue("@Gender", patient.Gender.Trim());
                updatePatientCommand.Parameters.AddWithValue("@Address", patient.Address.Trim());
                updatePatientCommand.Parameters.AddWithValue("@IsPWD", patient.IsPwd ? 1 : 0);
                updatePatientCommand.Parameters.AddWithValue("@IsSeniorCitizen", patient.IsSeniorCitizen ? 1 : 0);
                updatePatientCommand.Parameters.AddWithValue("@InitialTreatment", patient.InitialTreatment.Trim());
                updatePatientCommand.Parameters.AddWithValue("@UpdatedAt", updatedAt);
                updatePatientCommand.Parameters.AddWithValue("@PatientId", patient.PatientId);

                updatePatientCommand.ExecuteNonQuery();

                using SqliteCommand upsertHistoryCommand = connection.CreateCommand();
                upsertHistoryCommand.Transaction = transaction;
                upsertHistoryCommand.CommandText = @"
INSERT INTO PatientHistories (
    PatientId,
    DentalHistory,
    MedicalHistory,
    AllergyMedicationNotes,
    CreatedAt,
    UpdatedAt
)
VALUES (
    @PatientId,
    @DentalHistory,
    @MedicalHistory,
    @AllergyMedicationNotes,
    @CreatedAt,
    @UpdatedAt
)
ON CONFLICT(PatientId) DO UPDATE SET
    DentalHistory = excluded.DentalHistory,
    MedicalHistory = excluded.MedicalHistory,
    AllergyMedicationNotes = excluded.AllergyMedicationNotes,
    UpdatedAt = excluded.UpdatedAt;";

                upsertHistoryCommand.Parameters.AddWithValue("@PatientId", patient.PatientId);
                upsertHistoryCommand.Parameters.AddWithValue("@DentalHistory", patient.DentalHistory.Trim());
                upsertHistoryCommand.Parameters.AddWithValue("@MedicalHistory", patient.MedicalHistory.Trim());
                upsertHistoryCommand.Parameters.AddWithValue("@AllergyMedicationNotes", patient.AllergyMedicationNotes.Trim());
                upsertHistoryCommand.Parameters.AddWithValue("@CreatedAt", updatedAt);
                upsertHistoryCommand.Parameters.AddWithValue("@UpdatedAt", updatedAt);

                upsertHistoryCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion

        #region Archive and Restore

        public void SetPatientActiveStatus(int patientId, bool isActive)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
UPDATE Patients
SET
    IsActive = @IsActive,
    UpdatedAt = @UpdatedAt
WHERE PatientId = @PatientId;";

            command.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@PatientId", patientId);

            command.ExecuteNonQuery();
        }

        #endregion

        #region Duplicate Checking

        public PatientListItem? FindDuplicatePatient(
            string firstName,
            string middleName,
            string lastName,
            DateTime birthDate,
            string phoneNumber,
            int? excludedPatientId = null)
        {
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
    PhoneNumber,
    BirthDate,
    Gender,
    InitialTreatment,
    IsPWD,
    IsSeniorCitizen,
    IsActive,
    CreatedAt,
    0 AS HasPatientHistory,
    0 AS HasBalance
FROM Patients
WHERE
    (
        LOWER(TRIM(FirstName)) = LOWER(TRIM(@FirstName))
        AND LOWER(TRIM(IFNULL(MiddleName, ''))) = LOWER(TRIM(@MiddleName))
        AND LOWER(TRIM(LastName)) = LOWER(TRIM(@LastName))
        AND BirthDate = @BirthDate
    )
    OR PhoneNumber = @PhoneNumber
";

            if (excludedPatientId.HasValue)
            {
                command.CommandText += " AND PatientId <> @ExcludedPatientId";
                command.Parameters.AddWithValue("@ExcludedPatientId", excludedPatientId.Value);
            }

            command.CommandText += " LIMIT 1;";

            command.Parameters.AddWithValue("@FirstName", firstName.Trim());
            command.Parameters.AddWithValue("@MiddleName", middleName.Trim());
            command.Parameters.AddWithValue("@LastName", lastName.Trim());
            command.Parameters.AddWithValue("@BirthDate", birthDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@PhoneNumber", phoneNumber.Trim());

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
                return MapReaderToPatientListItem(reader);

            return null;
        }

        #endregion

        #region Helpers

        private string GenerateNextPatientCode(SqliteConnection connection, SqliteTransaction transaction)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
SELECT PatientCode
FROM Patients
WHERE PatientCode LIKE 'P%'
ORDER BY PatientId DESC
LIMIT 1;";

            object? result = command.ExecuteScalar();

            if (result == null)
                return "P001";

            string lastCode = result.ToString() ?? "P000";
            string digits = lastCode.Replace("P", "");

            if (!int.TryParse(digits, out int number))
                number = 0;

            return $"P{number + 1:000}";
        }

        private PatientListItem MapReaderToPatientListItem(SqliteDataReader reader)
        {
            return new PatientListItem
            {
                PatientId = Convert.ToInt32(reader["PatientId"]),
                PatientCode = reader["PatientCode"]?.ToString() ?? string.Empty,
                FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                MiddleName = reader["MiddleName"]?.ToString() ?? string.Empty,
                LastName = reader["LastName"]?.ToString() ?? string.Empty,
                PhoneNumber = reader["PhoneNumber"]?.ToString() ?? string.Empty,
                DateOfBirth = ParseNullableDate(reader["BirthDate"]?.ToString()),
                Gender = reader["Gender"]?.ToString() ?? string.Empty,
                Treatment = reader["InitialTreatment"]?.ToString() ?? string.Empty,
                IsPwd = Convert.ToInt32(reader["IsPWD"]) == 1,
                IsSenior = Convert.ToInt32(reader["IsSeniorCitizen"]) == 1,
                IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                CreatedAt = ParseDate(reader["CreatedAt"]?.ToString()),
                HasPatientHistory = Convert.ToInt32(reader["HasPatientHistory"]) == 1,
                HasBalance = Convert.ToInt32(reader["HasBalance"]) == 1
            };
        }

        private Patient MapReaderToPatient(SqliteDataReader reader)
        {
            return new Patient
            {
                PatientId = Convert.ToInt32(reader["PatientId"]),
                PatientCode = reader["PatientCode"]?.ToString() ?? string.Empty,
                FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                MiddleName = reader["MiddleName"]?.ToString() ?? string.Empty,
                LastName = reader["LastName"]?.ToString() ?? string.Empty,
                PhoneNumber = reader["PhoneNumber"]?.ToString() ?? string.Empty,
                BirthDate = ParseDate(reader["BirthDate"]?.ToString()),
                Gender = reader["Gender"]?.ToString() ?? string.Empty,
                Address = reader["Address"]?.ToString() ?? string.Empty,
                IsPwd = Convert.ToInt32(reader["IsPWD"]) == 1,
                IsSeniorCitizen = Convert.ToInt32(reader["IsSeniorCitizen"]) == 1,
                InitialTreatment = reader["InitialTreatment"]?.ToString() ?? string.Empty,
                IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                CreatedAt = ParseDate(reader["CreatedAt"]?.ToString()),
                UpdatedAt = ParseNullableDate(reader["UpdatedAt"]?.ToString()),
                DentalHistory = reader["DentalHistory"]?.ToString() ?? string.Empty,
                MedicalHistory = reader["MedicalHistory"]?.ToString() ?? string.Empty,
                AllergyMedicationNotes = reader["AllergyMedicationNotes"]?.ToString() ?? string.Empty
            };
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

        #endregion
    }
}