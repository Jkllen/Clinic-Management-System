using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CruzNeryClinic.Services
{
    public static class AutoBackupService
    {
        private const int AutoRetentionCount = 14;

        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CruzNeryClinic");

        public static string DefaultBackupFolder => Path.Combine(AppFolder, "Backups");

        private static string AutoBackupStatePath => Path.Combine(AppFolder, "autobackup-state.txt");

        private static string AutoBackupSecretPath => Path.Combine(AppFolder, "autobackup-secret.bin");

        public static void RunDailyStartupBackup()
        {
            Directory.CreateDirectory(DefaultBackupFolder);

            if (HasAutoBackupForToday())
            {
                BackupRetentionService.Apply(DefaultBackupFolder, AutoRetentionCount);
                return;
            }

            string password = GetOrCreateAutoBackupPassword();
            BackupPackageService backupPackageService = new();
            backupPackageService.CreateEncryptedBackup(DefaultBackupFolder, password, "Auto");

            File.WriteAllText(AutoBackupStatePath, DateTime.Today.ToString("yyyy-MM-dd"), Encoding.UTF8);
            BackupRetentionService.Apply(DefaultBackupFolder, AutoRetentionCount);
        }

        public static bool IsAutoBackupFile(string filePath)
            => Path.GetFileName(filePath).Contains("_AutoBackup_", StringComparison.OrdinalIgnoreCase);

        public static string GetOrCreateAutoBackupPassword()
        {
            Directory.CreateDirectory(AppFolder);

            if (File.Exists(AutoBackupSecretPath))
            {
                string existingSecret = LoadProtectedAutoBackupPassword();

                if (!string.IsNullOrWhiteSpace(existingSecret))
                    return existingSecret;
            }

            string newSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            SaveProtectedAutoBackupPassword(newSecret);
            return newSecret;
        }

        private static string LoadProtectedAutoBackupPassword()
        {
            try
            {
                byte[] protectedBytes = File.ReadAllBytes(AutoBackupSecretPath);
                byte[] plainBytes = ProtectedData.Unprotect(
                    protectedBytes,
                    optionalEntropy: null,
                    scope: DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(plainBytes).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void SaveProtectedAutoBackupPassword(string password)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(password);
            byte[] protectedBytes = ProtectedData.Protect(
                plainBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);

            File.WriteAllBytes(AutoBackupSecretPath, protectedBytes);
        }

        private static bool HasAutoBackupForToday()
        {
            if (File.Exists(AutoBackupStatePath))
            {
                string lastBackupDate = File.ReadAllText(AutoBackupStatePath, Encoding.UTF8).Trim();

                if (lastBackupDate == DateTime.Today.ToString("yyyy-MM-dd"))
                    return true;
            }

            string todayPrefix = $"CruzNeryClinic_AutoBackup_{DateTime.Today:yyyyMMdd}_";

            return Directory.Exists(DefaultBackupFolder) &&
                   Directory.GetFiles(DefaultBackupFolder, $"{todayPrefix}*.cnbak").Length > 0;
        }
    }
}
