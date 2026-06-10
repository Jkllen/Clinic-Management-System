using CruzNeryClinic.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    public partial class MaintenanceView : UserControl
    {
        private bool isClearingBackupPasswordInput;

        public MaintenanceView()
        {
            InitializeComponent();
        }

        private void BackupPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (isClearingBackupPasswordInput)
                return;

            if (DataContext is MaintenanceViewModel viewModel)
                viewModel.BackupPasswordPromptPassword = BackupPasswordInput.Password;
        }

        private void BackupPasswordPromptOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (BackupPasswordInput == null)
                return;

            if (e.NewValue is true)
            {
                isClearingBackupPasswordInput = true;
                BackupPasswordInput.Password = string.Empty;
                isClearingBackupPasswordInput = false;

                BackupPasswordInput.Focus();
            }
            else
            {
                isClearingBackupPasswordInput = true;
                BackupPasswordInput.Password = string.Empty;
                isClearingBackupPasswordInput = false;
            }
        }
    }
}
