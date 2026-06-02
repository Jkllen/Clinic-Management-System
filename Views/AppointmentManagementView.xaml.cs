using CruzNeryClinic.ViewModels;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    public partial class AppointmentManagementView : UserControl
    {
        public AppointmentManagementView()
        {
            InitializeComponent();
        }

        private void UploadAppointmentDetailsImage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AppointmentManagementViewModel viewModel)
                return;

            OpenFileDialog dialog = new()
            {
                Title = "Upload teeth image(s)",
                Multiselect = true,
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (string path in dialog.FileNames)
                    viewModel.AddAppointmentDetailsImage(path);
            }
        }
    }
}
