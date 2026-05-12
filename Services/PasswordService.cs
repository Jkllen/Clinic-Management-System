using System;
using System.Security.Cryptography;
using System.Text;

namespace CruzNeryClinic.Services
{
    public static class PasswordService
    {
        public static string GenerateSalt()
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(saltBytes);
        }

        public static string HashPassword(string password, string salt)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");

            string combined = password + salt;

            byte[] bytes = Encoding.UTF8.GetBytes(combined);
            byte[] hashBytes = SHA256.HashData(bytes);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string enteredPassword, string storedSalt, string storedHash)
        {
            string enteredHash = HashPassword(enteredPassword, storedSalt);
            return enteredHash == storedHash;
        }

        public static string NormalizeSecurityAnswer(string answer)
        {
            return answer.Trim().ToLowerInvariant();
        }

        public static string HashSecurityAnswer(string answer, string salt)
        {
            string normalizedAnswer = NormalizeSecurityAnswer(answer);
            return HashPassword(normalizedAnswer, salt);
        }
    }
}