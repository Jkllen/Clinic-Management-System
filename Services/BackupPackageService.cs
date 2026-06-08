using CruzNeryClinic.Data;
using CruzNeryClinic.Models.Maintenance;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CruzNeryClinic.Services
{
    public class BackupPackageService
    {
        private const int SaltSizeBytes = 16;
        private const int NonceSizeBytes = 12;
        private const int TagSizeBytes = 16;
        private const int KeySizeBytes = 32;
        private const int Pbkdf2Iterations = 200_000;

        private const string BackupMagic = "CNBAK1";

        public string CreateEncryptedBackup(string outputFolder, string backupPassword, string backupType = "Manual")
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
                throw new InvalidOperationException("Backup location is required.");

            if (string.IsNullOrWhiteSpace(backupPassword) || backupPassword.Length < 8)
                throw new InvalidOperationException("Backup password must be at least 8 characters.");

            Directory.CreateDirectory(outputFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupName = backupType == "Auto" ? "AutoBackup" : "Backup";
            string backupFilePath = Path.Combine(outputFolder, $"CruzNeryClinic_{backupName}_{timestamp}.cnbak");

            string tempFolder = Path.Combine(Path.GetTempPath(), $"CruzNeryClinic_Backup_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempFolder);

            try
            {
                string databasePath = DatabaseService.GetDatabasePath();

                if (!File.Exists(databasePath))
                    throw new FileNotFoundException("Database file was not found.", databasePath);

                string databaseFileName = Path.GetFileName(databasePath);
                string databaseCopyPath = Path.Combine(tempFolder, databaseFileName);

                File.Copy(databasePath, databaseCopyPath, true);

                byte[] rawAesKey = CryptoService.ExportRawKeyForBackup();
                byte[] encryptedRawAesKey = EncryptBackupBytes(rawAesKey, backupPassword);
                string keyPath = Path.Combine(tempFolder, "clinic_data.key.enc");
                File.WriteAllBytes(keyPath, encryptedRawAesKey);

                BackupMetadata metadata = new()
                {
                    BackupType = backupType,
                    CreatedAt = DateTime.Now,
                    DatabaseFileName = databaseFileName,
                    KeyFileName = "clinic_data.key.enc"
                };

                string metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(Path.Combine(tempFolder, "metadata.json"), metadataJson, Encoding.UTF8);

                string zipPath = Path.Combine(Path.GetTempPath(), $"CruzNeryClinic_Backup_{Guid.NewGuid()}.zip");

                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                ZipFile.CreateFromDirectory(tempFolder, zipPath, CompressionLevel.Optimal, false);

                byte[] plainPackage = File.ReadAllBytes(zipPath);
                byte[] encryptedPackage = EncryptBackupBytes(plainPackage, backupPassword);

                File.WriteAllBytes(backupFilePath, encryptedPackage);

                File.Delete(zipPath);

                ActivityLogService.Log(
                    "Backup",
                    "Maintenance",
                    $"Created {backupType.ToLowerInvariant()} database backup '{Path.GetFileName(backupFilePath)}'");

                return backupFilePath;
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);
            }
        }

        public void RestoreEncryptedBackup(string backupFilePath, string backupPassword)
        {
            if (string.IsNullOrWhiteSpace(backupFilePath) || !File.Exists(backupFilePath))
                throw new FileNotFoundException("Backup file was not found.", backupFilePath);

            if (string.IsNullOrWhiteSpace(backupPassword))
                throw new InvalidOperationException("Backup password is required.");

            string tempFolder = Path.Combine(Path.GetTempPath(), $"CruzNeryClinic_Restore_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempFolder);

            try
            {
                byte[] encryptedPackage = File.ReadAllBytes(backupFilePath);
                byte[] plainPackage = DecryptBackupBytes(encryptedPackage, backupPassword);

                string zipPath = Path.Combine(tempFolder, "restore.zip");
                File.WriteAllBytes(zipPath, plainPackage);

                string extractedFolder = Path.Combine(tempFolder, "extracted");
                ZipFile.ExtractToDirectory(zipPath, extractedFolder);

                string metadataPath = Path.Combine(extractedFolder, "metadata.json");

                if (!File.Exists(metadataPath))
                    throw new InvalidOperationException("Invalid backup package. Metadata is missing.");

                string metadataJson = File.ReadAllText(metadataPath, Encoding.UTF8);
                BackupMetadata? metadata = JsonSerializer.Deserialize<BackupMetadata>(metadataJson);

                if (metadata == null)
                    throw new InvalidOperationException("Invalid backup metadata.");

                string restoredDatabasePath = Path.Combine(extractedFolder, metadata.DatabaseFileName);
                string restoredKeyPath = Path.Combine(extractedFolder, metadata.KeyFileName);

                if (!File.Exists(restoredDatabasePath))
                    throw new InvalidOperationException("Backup package is missing the database file.");

                if (!File.Exists(restoredKeyPath))
                    throw new InvalidOperationException("Backup package is missing the AES key file.");

                string currentDatabasePath = DatabaseService.GetDatabasePath();

                string currentDatabaseFolder = Path.GetDirectoryName(currentDatabasePath)!;
                Directory.CreateDirectory(currentDatabaseFolder);

                File.Copy(restoredDatabasePath, currentDatabasePath, true);

                byte[] encryptedRawKey = File.ReadAllBytes(restoredKeyPath);
                byte[] rawKey = DecryptBackupBytes(encryptedRawKey, backupPassword);
                CryptoService.ImportRawKeyFromBackup(rawKey);

                ActivityLogService.Log(
                    "Restore",
                    "Maintenance",
                    $"Restored database from backup '{Path.GetFileName(backupFilePath)}'");
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);
            }
        }

        private static byte[] EncryptBackupBytes(byte[] plainBytes, string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
            byte[] key = DeriveBackupKey(password, salt);

            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[TagSizeBytes];

            using AesGcm aesGcm = new(key, TagSizeBytes);
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

            byte[] magicBytes = Encoding.UTF8.GetBytes(BackupMagic);

            byte[] output = new byte[
                magicBytes.Length +
                SaltSizeBytes +
                NonceSizeBytes +
                TagSizeBytes +
                cipherBytes.Length
            ];

            int offset = 0;

            Buffer.BlockCopy(magicBytes, 0, output, offset, magicBytes.Length);
            offset += magicBytes.Length;

            Buffer.BlockCopy(salt, 0, output, offset, salt.Length);
            offset += salt.Length;

            Buffer.BlockCopy(nonce, 0, output, offset, nonce.Length);
            offset += nonce.Length;

            Buffer.BlockCopy(tag, 0, output, offset, tag.Length);
            offset += tag.Length;

            Buffer.BlockCopy(cipherBytes, 0, output, offset, cipherBytes.Length);

            return output;
        }

        private static byte[] DecryptBackupBytes(byte[] encryptedBytes, string password)
        {
            byte[] magicBytes = Encoding.UTF8.GetBytes(BackupMagic);

            if (encryptedBytes.Length < magicBytes.Length + SaltSizeBytes + NonceSizeBytes + TagSizeBytes)
                throw new InvalidOperationException("Invalid backup file.");

            string magic = Encoding.UTF8.GetString(encryptedBytes, 0, magicBytes.Length);

            if (magic != BackupMagic)
                throw new InvalidOperationException("Invalid backup file format.");

            int offset = magicBytes.Length;

            byte[] salt = new byte[SaltSizeBytes];
            Buffer.BlockCopy(encryptedBytes, offset, salt, 0, SaltSizeBytes);
            offset += SaltSizeBytes;

            byte[] nonce = new byte[NonceSizeBytes];
            Buffer.BlockCopy(encryptedBytes, offset, nonce, 0, NonceSizeBytes);
            offset += NonceSizeBytes;

            byte[] tag = new byte[TagSizeBytes];
            Buffer.BlockCopy(encryptedBytes, offset, tag, 0, TagSizeBytes);
            offset += TagSizeBytes;

            byte[] cipherBytes = new byte[encryptedBytes.Length - offset];
            Buffer.BlockCopy(encryptedBytes, offset, cipherBytes, 0, cipherBytes.Length);

            byte[] key = DeriveBackupKey(password, salt);
            byte[] plainBytes = new byte[cipherBytes.Length];

            using AesGcm aesGcm = new(key, TagSizeBytes);
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return plainBytes;
        }

        private static byte[] DeriveBackupKey(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: Pbkdf2Iterations,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: KeySizeBytes
            );
        }
    }
}
