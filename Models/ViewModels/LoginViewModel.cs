using ClinicManagementSystem.Models.Entities;
using ClinicManagementSystem.Services;
using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ClinicManagementSystem.Models.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private string _username = string.Empty;
        private string _password = string.Empty;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public LoginViewModel()
        {
            _dbService = new DatabaseService();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool AuthenticateUser()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please enter username and password.", "Warning");
                return false;
            }

            string hashedPassword = EncryptionService.ComputeSHA256(Password);

            using var conn = _dbService.GetConnection();
            conn.Open();

            string query = "SELECT * FROM users WHERE username=@username AND password_hash=@password";
            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", Username);
            cmd.Parameters.AddWithValue("@password", hashedPassword);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string fullName = reader["full_name"].ToString() ?? Username;
                MessageBox.Show($"Welcome back, {fullName}!", "Login Successful");
                return true;
            }

            MessageBox.Show("Invalid username or password.", "Login Failed");
            return false;
        }
    }
}
