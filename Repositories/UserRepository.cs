using CruzNeryClinic.Data;
using CruzNeryClinic.Models;
using CruzNeryClinic.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace CruzNeryClinic.Repositories
{
    // This repository handles all database actions related to users.
    // Examples: login, forgot password, add user, update user, archive user, and user counts.
    public class UserRepository
    {
        // Finds one active user by username.
        // This is mainly used during login.
        public User? GetActiveUserByUsername(string username)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT 
    u.*,

    q1.QuestionText AS SecurityQuestion1,
    q2.QuestionText AS SecurityQuestion2,
    q3.QuestionText AS SecurityQuestion3

FROM Users u

INNER JOIN SecurityQuestions q1 
    ON u.SecurityQuestionId1 = q1.SecurityQuestionId

INNER JOIN SecurityQuestions q2 
    ON u.SecurityQuestionId2 = q2.SecurityQuestionId

INNER JOIN SecurityQuestions q3 
    ON u.SecurityQuestionId3 = q3.SecurityQuestionId
WHERE u.Username = @Username
AND u.IsActive = 1
LIMIT 1;";

            command.Parameters.AddWithValue("@Username", username.Trim());

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
                return MapReaderToUser(reader);

            return null;
        }

        // Finds one active user by UserCode.
        // This is useful for Forgot Password
        public User? GetActiveUserByUserCode(string userCode)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT 
    u.*,

    q1.QuestionText AS SecurityQuestion1,
    q2.QuestionText AS SecurityQuestion2,
    q3.QuestionText AS SecurityQuestion3

FROM Users u

INNER JOIN SecurityQuestions q1 
    ON u.SecurityQuestionId1 = q1.SecurityQuestionId

INNER JOIN SecurityQuestions q2 
    ON u.SecurityQuestionId2 = q2.SecurityQuestionId

INNER JOIN SecurityQuestions q3 
    ON u.SecurityQuestionId3 = q3.SecurityQuestionId
WHERE u.UserCode = @UserCode
AND u.IsActive = 1
LIMIT 1;";

            command.Parameters.AddWithValue("@UserCode", userCode.Trim());

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
                return MapReaderToUser(reader);

            return null;
        }

        // Finds one active user using either username or user code.
        // This lets the login field accept either admin01 or 2026-001.
        public User? GetActiveUserByLoginInput(string loginInput)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT 
    u.*,

    q1.QuestionText AS SecurityQuestion1,
    q2.QuestionText AS SecurityQuestion2,
    q3.QuestionText AS SecurityQuestion3

FROM Users u

INNER JOIN SecurityQuestions q1 
    ON u.SecurityQuestionId1 = q1.SecurityQuestionId

INNER JOIN SecurityQuestions q2 
    ON u.SecurityQuestionId2 = q2.SecurityQuestionId

INNER JOIN SecurityQuestions q3 
    ON u.SecurityQuestionId3 = q3.SecurityQuestionId
WHERE (u.Username = @LoginInput OR u.UserCode = @LoginInput)
AND u.IsActive = 1
LIMIT 1;";

            command.Parameters.AddWithValue("@LoginInput", loginInput.Trim());

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
                return MapReaderToUser(reader);

            return null;
        }

        // Verifies if the entered login details are correct.
        // Returns the User object if login succeeds, otherwise returns null.
        public User? Login(string loginInput, string password)
        {
            User? user = GetActiveUserByLoginInput(loginInput);

            if (user == null)
                return null;

            bool isPasswordCorrect = PasswordService.VerifyPassword(
                password,
                user.PasswordSalt,
                user.PasswordHash
            );

            if (!isPasswordCorrect)
                return null;

            UpdateLastLogin(user.UserId);

            AddActivityLog(
                user.UserId,
                user.UserCode,
                user.Username,
                "Login",
                "Security",
                $"{user.FullName} logged in."
            );

            return user;
        }

        // Updates the LastLoginAt field after successful login.
        public void UpdateLastLogin(int userId)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
UPDATE Users
SET LastLoginAt = @LastLoginAt
WHERE UserId = @UserId;";

            command.Parameters.AddWithValue("@LastLoginAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@UserId", userId);

            command.ExecuteNonQuery();
        }

        // Checks if all three security answers are correct.
        // Used before allowing the user to reset their password.
        public bool VerifySecurityAnswers(
            User user,
            string answer1,
            string answer2,
            string answer3)
        {
            bool answer1Correct =
                PasswordService.HashSecurityAnswer(answer1, user.SecurityAnswerSalt1) == user.SecurityAnswerHash1;

            bool answer2Correct =
                PasswordService.HashSecurityAnswer(answer2, user.SecurityAnswerSalt2) == user.SecurityAnswerHash2;

            bool answer3Correct =
                PasswordService.HashSecurityAnswer(answer3, user.SecurityAnswerSalt3) == user.SecurityAnswerHash3;

            return answer1Correct && answer2Correct && answer3Correct;
        }

        // Resets the password of a user after they pass the security questions.
        public void ResetPassword(int userId, string newPassword)
        {
            string newSalt = PasswordService.GenerateSalt();
            string newHash = PasswordService.HashPassword(newPassword, newSalt);

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
UPDATE Users
SET PasswordHash = @PasswordHash,
    PasswordSalt = @PasswordSalt,
    UpdatedAt = @UpdatedAt
WHERE UserId = @UserId;";

            command.Parameters.AddWithValue("@PasswordHash", newHash);
            command.Parameters.AddWithValue("@PasswordSalt", newSalt);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@UserId", userId);

            command.ExecuteNonQuery();
        }

        // Gets all users for the User Management screen.
        // Archived users can be hidden or shown depending on includeArchived.
        public List<User> GetAllUsers(bool includeArchived = false)
        {
            List<User> users = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();

            if (includeArchived)
            {
                command.CommandText = @"
SELECT 
    u.*,

    q1.QuestionText AS SecurityQuestion1,
    q2.QuestionText AS SecurityQuestion2,
    q3.QuestionText AS SecurityQuestion3

FROM Users u

INNER JOIN SecurityQuestions q1 
    ON u.SecurityQuestionId1 = q1.SecurityQuestionId

INNER JOIN SecurityQuestions q2 
    ON u.SecurityQuestionId2 = q2.SecurityQuestionId

INNER JOIN SecurityQuestions q3 
    ON u.SecurityQuestionId3 = q3.SecurityQuestionId

ORDER BY u.CreatedAt DESC;";
            }
            else
            {
                command.CommandText = @"
SELECT 
    u.*,

    q1.QuestionText AS SecurityQuestion1,
    q2.QuestionText AS SecurityQuestion2,
    q3.QuestionText AS SecurityQuestion3

FROM Users u

INNER JOIN SecurityQuestions q1 
    ON u.SecurityQuestionId1 = q1.SecurityQuestionId

INNER JOIN SecurityQuestions q2 
    ON u.SecurityQuestionId2 = q2.SecurityQuestionId

INNER JOIN SecurityQuestions q3 
    ON u.SecurityQuestionId3 = q3.SecurityQuestionId
WHERE u.IsActive = 1
ORDER BY u.CreatedAt DESC;";
            }

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(MapReaderToUser(reader));
            }

            return users;
        }

        // Searches users by user code, first name, last name, username, contact number, or role.
        // This supports the search bar in the User Management UI.
        public List<User> SearchUsers(string searchText)
        {
            List<User> users = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT 
    u.*,

    q1.QuestionText AS SecurityQuestion1,
    q2.QuestionText AS SecurityQuestion2,
    q3.QuestionText AS SecurityQuestion3

FROM Users u

INNER JOIN SecurityQuestions q1 
    ON u.SecurityQuestionId1 = q1.SecurityQuestionId

INNER JOIN SecurityQuestions q2 
    ON u.SecurityQuestionId2 = q2.SecurityQuestionId

INNER JOIN SecurityQuestions q3 
    ON u.SecurityQuestionId3 = q3.SecurityQuestionId

WHERE u.IsActive = 1
AND (
    UserCode LIKE @SearchText
    OR FirstName LIKE @SearchText
    OR MiddleName LIKE @SearchText
    OR LastName LIKE @SearchText
    OR Username LIKE @SearchText
    OR ContactNumber LIKE @SearchText
    OR Role LIKE @SearchText
)
ORDER BY u.CreatedAt DESC;";

            command.Parameters.AddWithValue("@SearchText", $"%{searchText.Trim()}%");

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(MapReaderToUser(reader));
            }

            return users;
        }
 
        // Adds a new user account.
        // This will be used later by the Admin in the User Management screen.
        // Adds a new user account.
        // This will be used later by the Admin in the User Management screen.
        // Security questions are now dynamic, so we store the selected SecurityQuestionId values,
        // not the actual question text.
        public void AddUser(
            string userCode,
            string firstName,
            string middleName,
            string lastName,
            string contactNumber,
            string username,
            string plainPassword,
            string role,
            int securityQuestionId1,
            string securityAnswer1,
            int securityQuestionId2,
            string securityAnswer2,
            int securityQuestionId3,
            string securityAnswer3,
            int createdByUserId,
            string createdByUserCode,
            string createdByUsername)
        {
            // Validate that the user selected 3 different security questions.
            // This prevents the same question from being reused multiple times.
            if (securityQuestionId1 == securityQuestionId2 ||
                securityQuestionId1 == securityQuestionId3 ||
                securityQuestionId2 == securityQuestionId3)
            {
                throw new InvalidOperationException("Please select three different security questions.");
            }

            // Generate password salt and hash.
            // The plain password is never stored in the database.
            string passwordSalt = PasswordService.GenerateSalt();
            string passwordHash = PasswordService.HashPassword(plainPassword, passwordSalt);

            // Generate separate salts for each security answer.
            string answerSalt1 = PasswordService.GenerateSalt();
            string answerSalt2 = PasswordService.GenerateSalt();
            string answerSalt3 = PasswordService.GenerateSalt();

            // Store hashed answers only.
            // The plain answers are never stored in the database.
            string answerHash1 = PasswordService.HashSecurityAnswer(securityAnswer1, answerSalt1);
            string answerHash2 = PasswordService.HashSecurityAnswer(securityAnswer2, answerSalt2);
            string answerHash3 = PasswordService.HashSecurityAnswer(securityAnswer3, answerSalt3);

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = @"
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

                command.Parameters.AddWithValue("@UserCode", userCode.Trim());
                command.Parameters.AddWithValue("@FirstName", firstName.Trim());
                command.Parameters.AddWithValue("@MiddleName", middleName.Trim());
                command.Parameters.AddWithValue("@LastName", lastName.Trim());
                command.Parameters.AddWithValue("@ContactNumber", contactNumber.Trim());
                command.Parameters.AddWithValue("@Username", username.Trim());
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@PasswordSalt", passwordSalt);
                command.Parameters.AddWithValue("@Role", role.Trim());

                command.Parameters.AddWithValue("@SecurityQuestionId1", securityQuestionId1);
                command.Parameters.AddWithValue("@SecurityAnswerHash1", answerHash1);
                command.Parameters.AddWithValue("@SecurityAnswerSalt1", answerSalt1);

                command.Parameters.AddWithValue("@SecurityQuestionId2", securityQuestionId2);
                command.Parameters.AddWithValue("@SecurityAnswerHash2", answerHash2);
                command.Parameters.AddWithValue("@SecurityAnswerSalt2", answerSalt2);

                command.Parameters.AddWithValue("@SecurityQuestionId3", securityQuestionId3);
                command.Parameters.AddWithValue("@SecurityAnswerHash3", answerHash3);
                command.Parameters.AddWithValue("@SecurityAnswerSalt3", answerSalt3);

                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();

                AddActivityLogWithinTransaction(
                    connection,
                    transaction,
                    createdByUserId,
                    createdByUserCode,
                    createdByUsername,
                    "Add",
                    "Users",
                    $"Created user account: {userCode} - {firstName} {lastName}."
                );

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        // Updates user profile information.
        // Password is not changed here; password reset has a separate method.
        public void UpdateUser(
            int userId,
            string firstName,
            string middleName,
            string lastName,
            string contactNumber,
            string role,
            int updatedByUserId,
            string updatedByUserCode,
            string updatedByUsername)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = @"
UPDATE Users
SET FirstName = @FirstName,
    MiddleName = @MiddleName,
    LastName = @LastName,
    ContactNumber = @ContactNumber,
    Role = @Role,
    UpdatedAt = @UpdatedAt
WHERE UserId = @UserId;";

                command.Parameters.AddWithValue("@FirstName", firstName.Trim());
                command.Parameters.AddWithValue("@MiddleName", middleName.Trim());
                command.Parameters.AddWithValue("@LastName", lastName.Trim());
                command.Parameters.AddWithValue("@ContactNumber", contactNumber.Trim());
                command.Parameters.AddWithValue("@Role", role.Trim());
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@UserId", userId);

                command.ExecuteNonQuery();

                AddActivityLogWithinTransaction(
                    connection,
                    transaction,
                    updatedByUserId,
                    updatedByUserCode,
                    updatedByUsername,
                    "Update",
                    "Users",
                    $"Updated user account with ID: {userId}."
                );

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Archives a user instead of deleting them permanently.
        // This protects records and keeps activity history valid.
        public void ArchiveUser(
            int userId,
            int archivedByUserId,
            string archivedByUserCode,
            string archivedByUsername)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = @"
UPDATE Users
SET IsActive = 0,
    UpdatedAt = @UpdatedAt
WHERE UserId = @UserId;";

                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@UserId", userId);

                command.ExecuteNonQuery();

                AddActivityLogWithinTransaction(
                    connection,
                    transaction,
                    archivedByUserId,
                    archivedByUserCode,
                    archivedByUsername,
                    "Archive",
                    "Users",
                    $"Archived user account with ID: {userId}."
                );

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void RestoreUser(
            int userId,
            int restoredByUserId,
            string restoredByUserCode,
            string restoredByUsername)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteTransaction transaction = connection.BeginTransaction();

            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = @"
        UPDATE Users
        SET IsActive = 1,
            UpdatedAt = @UpdatedAt
        WHERE UserId = @UserId;";

                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@UserId", userId);

                command.ExecuteNonQuery();

                AddActivityLogWithinTransaction(
                    connection,
                    transaction,
                    restoredByUserId,
                    restoredByUserCode,
                    restoredByUsername,
                    "Restore",
                    "Users",
                    $"Restored user account with ID: {userId}."
                );

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Counts all active users.
        // Used by User Management summary card.
        public int CountActiveUsers()
        {
            return CountByQuery("SELECT COUNT(*) FROM Users WHERE IsActive = 1;");
        }

        // Counts active admin accounts.
        // Used by User Management summary card.
        public int CountAdmins()
        {
            return CountByQuery("SELECT COUNT(*) FROM Users WHERE IsActive = 1 AND Role = 'Admin';");
        }

        // Counts active non-admin staff.
        // Used by User Management summary card.
        public int CountStaff()
        {
            return CountByQuery("SELECT COUNT(*) FROM Users WHERE IsActive = 1 AND Role != 'Admin';");
        }

        // Checks if a username already exists.
        // Used before adding a new user.
        public bool UsernameExists(string username)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT COUNT(*)
FROM Users
WHERE Username = @Username;";

            command.Parameters.AddWithValue("@Username", username.Trim());

            long count = (long)command.ExecuteScalar()!;
            return count > 0;
        }

        // Checks if a user code already exists.
        // Used before adding a new user.
        public bool UserCodeExists(string userCode)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT COUNT(*)
FROM Users
WHERE UserCode = @UserCode;";

            command.Parameters.AddWithValue("@UserCode", userCode.Trim());

            long count = (long)command.ExecuteScalar()!;
            return count > 0;
        }

        // Generates the next UserCode using the current year.
        // Example output: 2026-001, 2026-002, 2026-003.
        public string GenerateNextUserCode()
        {
            string year = DateTime.Now.Year.ToString();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
SELECT UserCode
FROM Users
WHERE UserCode LIKE @YearPrefix
ORDER BY UserCode DESC
LIMIT 1;";

            command.Parameters.AddWithValue("@YearPrefix", $"{year}-%");

            object? result = command.ExecuteScalar();

            if (result == null)
                return $"{year}-001";

            string latestCode = result.ToString()!;

            string latestNumberText = latestCode.Split('-')[1];
            int latestNumber = int.Parse(latestNumberText);

            int nextNumber = latestNumber + 1;

            return $"{year}-{nextNumber:D3}";
        }

        // Adds a new activity log.
        // This is useful for login, update, archive, backup, restore, and other important actions.
        public void AddActivityLog(
            int? userId,
            string userCode,
            string username,
            string action,
            string module,
            string description)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO ActivityLogs (
    UserId,
    UserCode,
    Username,
    Action,
    Module,
    Description,
    CreatedAt
)
VALUES (
    @UserId,
    @UserCode,
    @Username,
    @Action,
    @Module,
    @Description,
    @CreatedAt
);";

            command.Parameters.AddWithValue("@UserId", userId.HasValue ? userId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@UserCode", userCode);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Action", action);
            command.Parameters.AddWithValue("@Module", module);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }

        // Same as AddActivityLog, but used inside an existing database transaction.
        // This keeps the main action and the log action together.
        private void AddActivityLogWithinTransaction(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int? userId,
            string userCode,
            string username,
            string action,
            string module,
            string description)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = @"
INSERT INTO ActivityLogs (
    UserId,
    UserCode,
    Username,
    Action,
    Module,
    Description,
    CreatedAt
)
VALUES (
    @UserId,
    @UserCode,
    @Username,
    @Action,
    @Module,
    @Description,
    @CreatedAt
);";

            command.Parameters.AddWithValue("@UserId", userId.HasValue ? userId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@UserCode", userCode);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Action", action);
            command.Parameters.AddWithValue("@Module", module);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }
        
        // Gets all active security questions.
        // This will be used by the User Registration screen dropdowns later.
        public List<SecurityQuestion> GetActiveSecurityQuestions()
        {
            List<SecurityQuestion> questions = new();

            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
        SELECT
            SecurityQuestionId,
            QuestionText,
            IsActive,
            CreatedAt
        FROM SecurityQuestions
        WHERE IsActive = 1
        ORDER BY SecurityQuestionId ASC;";

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                questions.Add(new SecurityQuestion
                {
                    SecurityQuestionId = Convert.ToInt32(reader["SecurityQuestionId"]),
                    QuestionText = reader["QuestionText"].ToString() ?? string.Empty,
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()!)
                });
            }

            return questions;
        }

        // Helper method for count queries.
        private int CountByQuery(string sql)
        {
            using SqliteConnection connection = DatabaseService.GetConnection();
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;

            long count = (long)command.ExecuteScalar()!;
            return (int)count;
        }

        // Converts one database row from the Users table into a User object.
        // This prevents repeating reader["ColumnName"] code everywhere.
        private User MapReaderToUser(SqliteDataReader reader)
        {
            return new User
            {
                UserId = Convert.ToInt32(reader["UserId"]),
                UserCode = reader["UserCode"].ToString() ?? string.Empty,

                FirstName = reader["FirstName"].ToString() ?? string.Empty,
                MiddleName = reader["MiddleName"].ToString() ?? string.Empty,
                LastName = reader["LastName"].ToString() ?? string.Empty,

                ContactNumber = reader["ContactNumber"].ToString() ?? string.Empty,
                Username = reader["Username"].ToString() ?? string.Empty,

                PasswordHash = reader["PasswordHash"].ToString() ?? string.Empty,
                PasswordSalt = reader["PasswordSalt"].ToString() ?? string.Empty,

                Role = reader["Role"].ToString() ?? string.Empty,

                SecurityQuestionId1 = Convert.ToInt32(reader["SecurityQuestionId1"]),
                SecurityQuestionId2 = Convert.ToInt32(reader["SecurityQuestionId2"]),
                SecurityQuestionId3 = Convert.ToInt32(reader["SecurityQuestionId3"]),

                SecurityQuestion1 = reader["SecurityQuestion1"].ToString() ?? string.Empty,
                SecurityAnswerHash1 = reader["SecurityAnswerHash1"].ToString() ?? string.Empty,
                SecurityAnswerSalt1 = reader["SecurityAnswerSalt1"].ToString() ?? string.Empty,

                SecurityQuestion2 = reader["SecurityQuestion2"].ToString() ?? string.Empty,
                SecurityAnswerHash2 = reader["SecurityAnswerHash2"].ToString() ?? string.Empty,
                SecurityAnswerSalt2 = reader["SecurityAnswerSalt2"].ToString() ?? string.Empty,

                SecurityQuestion3 = reader["SecurityQuestion3"].ToString() ?? string.Empty,
                SecurityAnswerHash3 = reader["SecurityAnswerHash3"].ToString() ?? string.Empty,
                SecurityAnswerSalt3 = reader["SecurityAnswerSalt3"].ToString() ?? string.Empty,

                IsActive = Convert.ToInt32(reader["IsActive"]) == 1,

                CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()!),

                UpdatedAt = string.IsNullOrWhiteSpace(reader["UpdatedAt"].ToString())
                    ? null
                    : DateTime.Parse(reader["UpdatedAt"].ToString()!),

                LastLoginAt = string.IsNullOrWhiteSpace(reader["LastLoginAt"].ToString())
                    ? null
                    : DateTime.Parse(reader["LastLoginAt"].ToString()!)
            };
        }
    }
}