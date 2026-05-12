using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace CruzNeryClinic.Data
{
    public static class DatabaseService
    {
        private static readonly string AppFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CruzNeryClinic");

        private static readonly string DatabasePath =
            Path.Combine(AppFolder, "cruz_nery_clinic.db");

        public static string ConnectionString => $"Data Source={DatabasePath}";

        public static SqliteConnection GetConnection()
        {
            if (!Directory.Exists(AppFolder))
                Directory.CreateDirectory(AppFolder);

            return new SqliteConnection(ConnectionString);
        }

        public static string GetDatabasePath()
        {
            if (!Directory.Exists(AppFolder))
                Directory.CreateDirectory(AppFolder);

            return DatabasePath;
        }
    }
}