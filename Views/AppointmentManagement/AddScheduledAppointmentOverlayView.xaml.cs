using CruzNeryClinic.ViewModels;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views.AppointmentManagement
{
    public partial class AddScheduledAppointmentOverlayView : UserControl
    {
        public AddScheduledAppointmentOverlayView()
        {
            InitializeComponent();
        }

        private void UploadTeethImage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AppointmentManagementViewModel viewModel)
                return;

            OpenFileDialog dialog = new()
            {
                Title = "Select teeth image(s)",
                Multiselect = true,
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (string path in dialog.FileNames)
                    viewModel.AddTeethImage(path);
            }
        }
    }
}
