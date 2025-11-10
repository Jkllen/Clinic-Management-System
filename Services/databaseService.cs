using System.Data.SQLite;
using System.IO;

namespace Clinic_Management_System.Services
{
    public static class DatabaseService
    {
        private static readonly string dbFile = "clinic.db";
        private static readonly string connectionString = $"Data Source={dbFile};Version=3;";

        public static void InitializeDatabase()
        {
            bool dbExists = File.Exists(dbFile);

            if (!dbExists)
                SQLiteConnection.CreateFile(dbFile);

            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string createUserTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL
                );";

            using var cmd = new SQLiteCommand(createUserTable, conn);
            cmd.ExecuteNonQuery();

            // Seed default user if table empty
            cmd.CommandText = "SELECT COUNT(*) FROM Users;";
            long count = (long)cmd.ExecuteScalar()!;
            if (count == 0)
            {
                string defaultUsername = "admin";
                string defaultPasswordHash = EncryptionService.ComputeSHA256("admin123");

                cmd.CommandText = "INSERT INTO Users (Username, PasswordHash) VALUES (@u, @p)";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@u", defaultUsername);
                cmd.Parameters.AddWithValue("@p", defaultPasswordHash);
                cmd.ExecuteNonQuery();
            }
        }

        public static bool AuthenticateUser(string username, string password)
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            string query = "SELECT PasswordHash FROM Users WHERE Username=@u";
            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);

            object? result = cmd.ExecuteScalar();
            if (result is not string storedHash) // safe null check + type check
                return false;

            string inputHash = EncryptionService.ComputeSHA256(password);
            return storedHash == inputHash;
        }
    }
}
