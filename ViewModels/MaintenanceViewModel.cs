using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models.Maintenance;
using CruzNeryClinic.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    public class MaintenanceViewModel : BaseViewModel
    {
        #region Dependencies and Backing Fields

        private readonly BackupPackageService backupPackageService;

        private string primaryBackupLocation = string.Empty;
        private string secondaryBackupLocation = string.Empty;
        private string selectedRestoreFilePath = string.Empty;
        private string selectedRetentionOption = "Keep last 14 backups";

        private string lastBackupTime = "--:--";
        private string lastBackupDate = "No backup yet";
        private string totalBackupsDisplay = "0";
        private string backupLocationDisplay = "Not set";

        private string promptTitle = string.Empty;
        private string promptMessage = string.Empty;
        private bool isPromptVisible;

        #endregion

        #region Collections

        public ObservableCollection<string> BackupRetentionOptions { get; }

        // Main card: shows only the most recent backup.
        public ObservableCollection<BackupHistoryItem> BackupHistoryItems { get; }

        // "View All" overlay: shows the current page of the full backup list.
        public ObservableCollection<BackupHistoryItem> BackupHistoryPageItems { get; }

        // Full backup list backing the overlay pagination.
        private readonly List<BackupHistoryItem> allBackupHistory = new();

        private const int BackupHistoryPageSize = 5;

        #endregion

        #region Properties

        public string PrimaryBackupLocation
        {
            get => primaryBackupLocation;
            set
            {
                if (SetProperty(ref primaryBackupLocation, value))
                {
                    BackupLocationDisplay = string.IsNullOrWhiteSpace(value)
                        ? "Not set"
                        : value;

                    RefreshBackupHistory();
                }
            }
        }

        public string SecondaryBackupLocation
        {
            get => secondaryBackupLocation;
            set => SetProperty(ref secondaryBackupLocation, value);
        }

        public string SelectedRestoreFilePath
        {
            get => selectedRestoreFilePath;
            set => SetProperty(ref selectedRestoreFilePath, value);
        }

        public string SelectedRetentionOption
        {
            get => selectedRetentionOption;
            set => SetProperty(ref selectedRetentionOption, value);
        }

        public string LastBackupTime
        {
            get => lastBackupTime;
            set => SetProperty(ref lastBackupTime, value);
        }

        public string LastBackupDate
        {
            get => lastBackupDate;
            set => SetProperty(ref lastBackupDate, value);
        }

        public string TotalBackupsDisplay
        {
            get => totalBackupsDisplay;
            set => SetProperty(ref totalBackupsDisplay, value);
        }

        public string BackupLocationDisplay
        {
            get => backupLocationDisplay;
            set => SetProperty(ref backupLocationDisplay, value);
        }

        public string PromptTitle
        {
            get => promptTitle;
            set => SetProperty(ref promptTitle, value);
        }

        public string PromptMessage
        {
            get => promptMessage;
            set => SetProperty(ref promptMessage, value);
        }

        public bool IsPromptVisible
        {
            get => isPromptVisible;
            set => SetProperty(ref isPromptVisible, value);
        }

        // ── Backup history pagination (BackupHistoryPageSize rows per page) ────
        private int backupPage = 1;
        public int BackupPage
        {
            get => backupPage;
            private set { if (SetProperty(ref backupPage, value)) OnPropertyChanged(nameof(BackupPageInfo)); }
        }

        public int BackupTotalPages =>
            Math.Max(1, (int)Math.Ceiling(allBackupHistory.Count / (double)BackupHistoryPageSize));

        public string BackupPageInfo => $"Page {BackupPage} of {BackupTotalPages}";

        public bool BackupHasMultiplePages => allBackupHistory.Count > BackupHistoryPageSize;

        private bool isBackupHistoryOpen;
        public bool IsBackupHistoryOpen
        {
            get => isBackupHistoryOpen;
            set => SetProperty(ref isBackupHistoryOpen, value);
        }

        #endregion

        #region Commands

        public ICommand BrowsePrimaryBackupLocationCommand { get; }

        public ICommand BrowseSecondaryBackupLocationCommand { get; }

        public ICommand BrowseRestoreFileCommand { get; }

        public ICommand CreateManualBackupCommand { get; }

        public ICommand RestoreSelectedBackupCommand { get; }

        public ICommand RestoreBackupHistoryItemCommand { get; }

        public ICommand OpenBackupLocationCommand { get; }

        public ICommand ClosePromptCommand { get; }

        public IRelayCommand BackupNextPageCommand { get; }

        public IRelayCommand BackupPrevPageCommand { get; }

        public ICommand OpenBackupHistoryCommand { get; }

        public ICommand CloseBackupHistoryCommand { get; }

        #endregion

        #region Constructor

        public MaintenanceViewModel()
        {
            backupPackageService = new BackupPackageService();

            BackupRetentionOptions = new ObservableCollection<string>
            {
                "Keep last 7 backups",
                "Keep last 14 backups",
                "Keep last 30 backups",
                "Keep all backups"
            };

            BackupHistoryItems = new ObservableCollection<BackupHistoryItem>();
            BackupHistoryPageItems = new ObservableCollection<BackupHistoryItem>();

            BrowsePrimaryBackupLocationCommand = new RelayCommand(BrowsePrimaryBackupLocation);
            BrowseSecondaryBackupLocationCommand = new RelayCommand(BrowseSecondaryBackupLocation);
            BrowseRestoreFileCommand = new RelayCommand(BrowseRestoreFile);

            CreateManualBackupCommand = new RelayCommand(CreateManualBackup);
            RestoreSelectedBackupCommand = new RelayCommand(RestoreSelectedBackup);
            RestoreBackupHistoryItemCommand = new RelayCommand<BackupHistoryItem>(RestoreBackupHistoryItem);

            OpenBackupLocationCommand = new RelayCommand<BackupHistoryItem>(OpenBackupLocation);
            ClosePromptCommand = new RelayCommand(ClosePrompt);

            BackupNextPageCommand = new RelayCommand(BackupNextPage, () => BackupPage < BackupTotalPages);
            BackupPrevPageCommand = new RelayCommand(BackupPrevPage, () => BackupPage > 1);
            OpenBackupHistoryCommand = new RelayCommand(OpenBackupHistory);
            CloseBackupHistoryCommand = new RelayCommand(CloseBackupHistory);

            LoadDefaultBackupLocation();
            RefreshBackupHistory();
        }

        #endregion

        #region Browse Methods

        private void BrowsePrimaryBackupLocation()
        {
            string? selectedFolder = SelectFolder("Select primary backup location");

            if (string.IsNullOrWhiteSpace(selectedFolder))
                return;

            PrimaryBackupLocation = selectedFolder;
        }

        private void BrowseSecondaryBackupLocation()
        {
            string? selectedFolder = SelectFolder("Select secondary backup location");

            if (string.IsNullOrWhiteSpace(selectedFolder))
                return;

            SecondaryBackupLocation = selectedFolder;
        }

        private void BrowseRestoreFile()
        {
            Microsoft.Win32.OpenFileDialog dialog = new()
            {
                Title = "Select Dental Clinic Management System backup file",
                Filter = "Dental Clinic Management System Backup Files (*.cnbak)|*.cnbak|All Files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
                SelectedRestoreFilePath = dialog.FileName;
        }

        private string? SelectFolder(string description)
        {
            using FolderBrowserDialog dialog = new()
            {
                Description = description,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            DialogResult result = dialog.ShowDialog();

            return result == DialogResult.OK
                ? dialog.SelectedPath
                : null;
        }

        #endregion

        #region Backup Methods

        private void CreateManualBackup()
        {
            if (string.IsNullOrWhiteSpace(PrimaryBackupLocation))
            {
                ShowPrompt("Backup Required", "Please select a primary backup location first.");
                return;
            }

            try
            {
                string backupPassword = PromptForBackupPassword();

                if (string.IsNullOrWhiteSpace(backupPassword))
                {
                    ShowPrompt("Backup Cancelled", "Backup was cancelled because no backup password was entered.");
                    return;
                }

                string backupFilePath = backupPackageService.CreateEncryptedBackup(
                    PrimaryBackupLocation,
                    backupPassword
                );

                CopyToSecondaryBackupLocationIfAvailable(backupFilePath);
                ApplyBackupRetentionPolicy();
                RefreshBackupHistory();

                ShowPrompt(
                    "Backup Successful",
                    $"Encrypted backup was created successfully.\n\nLocation:\n{backupFilePath}"
                );
            }
            catch (Exception ex)
            {
                ShowPrompt("Backup Failed", $"Failed to create backup: {ex.Message}");
            }
        }

        private void RestoreSelectedBackup()
        {
            if (string.IsNullOrWhiteSpace(SelectedRestoreFilePath))
            {
                ShowPrompt("Restore Required", "Please select a backup file to restore.");
                return;
            }

            RestoreBackupFile(SelectedRestoreFilePath);
        }

        private void RestoreBackupHistoryItem(BackupHistoryItem? item)
        {
            if (item == null)
                return;

            if (string.IsNullOrWhiteSpace(item.FileLocation) || !File.Exists(item.FileLocation))
            {
                ShowPrompt("Restore Failed", "The selected backup file no longer exists.");
                return;
            }

            RestoreBackupFile(item.FileLocation);
        }

        private void RestoreBackupFile(string backupFilePath)
        {
            try
            {
                DialogResult confirmResult = MessageBox.Show(
                    "Restoring this backup will replace the current database and AES-GCM key.\n\n" +
                    "Make sure you have created a recent backup before continuing.\n\n" +
                    "Do you want to continue?",
                    "Confirm Restore",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (confirmResult != DialogResult.Yes)
                    return;

                string backupPassword = PromptForBackupPassword();

                if (string.IsNullOrWhiteSpace(backupPassword))
                {
                    ShowPrompt("Restore Cancelled", "Restore was cancelled because no backup password was entered.");
                    return;
                }

                backupPackageService.RestoreEncryptedBackup(backupFilePath, backupPassword);

                RefreshBackupHistory();

                ShowPrompt(
                    "Restore Successful",
                    "Backup was restored successfully. Please restart the application to reload the restored database."
                );
            }
            catch (CryptographicException)
            {
                ShowPrompt("Restore Failed", "Invalid backup password or corrupted backup file.");
            }
            catch (Exception ex)
            {
                ShowPrompt("Restore Failed", $"Failed to restore backup: {ex.Message}");
            }
        }

        private void CopyToSecondaryBackupLocationIfAvailable(string primaryBackupFilePath)
        {
            if (string.IsNullOrWhiteSpace(SecondaryBackupLocation))
                return;

            if (!Directory.Exists(SecondaryBackupLocation))
                Directory.CreateDirectory(SecondaryBackupLocation);

            string fileName = Path.GetFileName(primaryBackupFilePath);
            string secondaryFilePath = Path.Combine(SecondaryBackupLocation, fileName);

            File.Copy(primaryBackupFilePath, secondaryFilePath, true);
        }

        #endregion

        #region Backup History and Retention

        private void LoadDefaultBackupLocation()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            PrimaryBackupLocation = Path.Combine(
                appDataPath,
                "CruzNeryClinic",
                "Backups"
            );

            Directory.CreateDirectory(PrimaryBackupLocation);
        }

        private void RefreshBackupHistory()
        {
            allBackupHistory.Clear();

            if (string.IsNullOrWhiteSpace(PrimaryBackupLocation) ||
                !Directory.Exists(PrimaryBackupLocation))
            {
                LastBackupTime = "--:--";
                LastBackupDate = "No backup yet";
                TotalBackupsDisplay = "0";
                BackupHistoryItems.Clear();
                BackupPage = 1;
                RefreshBackupPage();
                return;
            }

            FileInfo[] backupFiles = new DirectoryInfo(PrimaryBackupLocation)
                .GetFiles("*.cnbak")
                .OrderByDescending(file => file.CreationTime)
                .ToArray();

            foreach (FileInfo file in backupFiles)
            {
                allBackupHistory.Add(new BackupHistoryItem
                {
                    CreatedAt = file.CreationTime,
                    BackupType = "Manual",
                    FileLocation = file.FullName,
                    SizeDisplay = FormatFileSize(file.Length),
                    Status = "Success"
                });
            }

            TotalBackupsDisplay = backupFiles.Length.ToString();

            // Main card shows only the most recent backup.
            BackupHistoryItems.Clear();
            if (allBackupHistory.Count > 0)
                BackupHistoryItems.Add(allBackupHistory[0]);

            BackupPage = 1;
            RefreshBackupPage();

            if (backupFiles.Length == 0)
            {
                LastBackupTime = "--:--";
                LastBackupDate = "No backup yet";
                return;
            }

            FileInfo latestBackup = backupFiles[0];

            LastBackupTime = latestBackup.CreationTime.ToString("HH:mm");
            LastBackupDate = latestBackup.CreationTime.ToString("yyyy-MM-dd");
        }

        // Rebuilds BackupHistoryPageItems (the "View All" overlay) to show the current page's slice.
        private void RefreshBackupPage()
        {
            if (BackupPage > BackupTotalPages) BackupPage = BackupTotalPages;
            if (BackupPage < 1) BackupPage = 1;

            BackupHistoryPageItems.Clear();
            foreach (var item in allBackupHistory
                .Skip((BackupPage - 1) * BackupHistoryPageSize)
                .Take(BackupHistoryPageSize))
            {
                BackupHistoryPageItems.Add(item);
            }

            OnPropertyChanged(nameof(BackupTotalPages));
            OnPropertyChanged(nameof(BackupPageInfo));
            OnPropertyChanged(nameof(BackupHasMultiplePages));
            BackupNextPageCommand.NotifyCanExecuteChanged();
            BackupPrevPageCommand.NotifyCanExecuteChanged();
        }

        private void BackupNextPage()
        {
            if (BackupPage < BackupTotalPages) { BackupPage++; RefreshBackupPage(); }
        }

        private void BackupPrevPage()
        {
            if (BackupPage > 1) { BackupPage--; RefreshBackupPage(); }
        }

        private void OpenBackupHistory()
        {
            BackupPage = 1;
            RefreshBackupPage();
            IsBackupHistoryOpen = true;
        }

        private void CloseBackupHistory()
        {
            IsBackupHistoryOpen = false;
        }

        private void ApplyBackupRetentionPolicy()
        {
            if (string.IsNullOrWhiteSpace(PrimaryBackupLocation) ||
                !Directory.Exists(PrimaryBackupLocation))
            {
                return;
            }

            int? keepCount = SelectedRetentionOption switch
            {
                "Keep last 7 backups" => 7,
                "Keep last 14 backups" => 14,
                "Keep last 30 backups" => 30,
                "Keep all backups" => null,
                _ => 14
            };

            if (keepCount == null)
                return;

            FileInfo[] backupFiles = new DirectoryInfo(PrimaryBackupLocation)
                .GetFiles("*.cnbak")
                .OrderByDescending(file => file.CreationTime)
                .ToArray();

            foreach (FileInfo oldBackup in backupFiles.Skip(keepCount.Value))
                oldBackup.Delete();
        }

        private void OpenBackupLocation(BackupHistoryItem? item)
        {
            try
            {
                string folderPath;

                if (item != null && !string.IsNullOrWhiteSpace(item.FileLocation))
                    folderPath = Path.GetDirectoryName(item.FileLocation) ?? PrimaryBackupLocation;
                else
                    folderPath = PrimaryBackupLocation;

                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                    ShowPrompt("Location Not Found", "Backup location does not exist.");
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowPrompt("Open Location Failed", $"Failed to open backup location: {ex.Message}");
            }
        }

        private string FormatFileSize(long bytes)
        {
            double size = bytes;

            if (size < 1024)
                return $"{size:N0} B";

            size /= 1024;

            if (size < 1024)
                return $"{size:N1} KB";

            size /= 1024;

            if (size < 1024)
                return $"{size:N1} MB";

            size /= 1024;

            return $"{size:N1} GB";
        }

        #endregion

        #region Prompt Helpers

        private string PromptForBackupPassword()
        {
            // This is temporary for backend testing.
            // Later, we can replace this with a custom styled password overlay
            // that matches your modern prompt design.
            using Form form = new()
            {
                Width = 420,
                Height = 190,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Backup Password",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label label = new()
            {
                Left = 20,
                Top = 20,
                Width = 360,
                Text = "Enter backup password:"
            };

            TextBox passwordBox = new()
            {
                Left = 20,
                Top = 50,
                Width = 360,
                PasswordChar = '●'
            };

            Button okButton = new()
            {
                Text = "OK",
                Left = 220,
                Width = 75,
                Top = 90,
                DialogResult = DialogResult.OK
            };

            Button cancelButton = new()
            {
                Text = "Cancel",
                Left = 305,
                Width = 75,
                Top = 90,
                DialogResult = DialogResult.Cancel
            };

            form.Controls.Add(label);
            form.Controls.Add(passwordBox);
            form.Controls.Add(okButton);
            form.Controls.Add(cancelButton);

            form.AcceptButton = okButton;
            form.CancelButton = cancelButton;

            DialogResult result = form.ShowDialog();

            return result == DialogResult.OK
                ? passwordBox.Text
                : string.Empty;
        }

        private void ShowPrompt(string title, string message)
        {
            PromptTitle = title;
            PromptMessage = message;
            IsPromptVisible = true;
        }

        private void ClosePrompt()
        {
            IsPromptVisible = false;
            PromptTitle = string.Empty;
            PromptMessage = string.Empty;
        }

        #endregion
    }
}
