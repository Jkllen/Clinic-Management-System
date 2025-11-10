using System;
using System.Data.SQLite;
using System.IO;

namespace ClinicManagementSystem.Services
{
    public class DatabaseService
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clinic.db");
        private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        public DatabaseService()
        {
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
                InitializeDatabase();
            }
        }

        private void InitializeDatabase()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string createUserTable = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    full_name TEXT
                );
            ";

            using var cmd = new SQLiteCommand(createUserTable, conn);
            cmd.ExecuteNonQuery();
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }
}
