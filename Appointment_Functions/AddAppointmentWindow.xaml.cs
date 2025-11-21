using System.Windows;
using System.Windows.Controls;

namespace Clinic_Management_System.Appointment_Functions
{
    public partial class AddAppointmentWindow : Window
    {
        public Appointment? NewAppointment { get; private set; }

        public AddAppointmentWindow()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPatientID.Text) || 
                string.IsNullOrWhiteSpace(txtPatientName.Text))
            {
                MessageBox.Show("Patient ID and Name are required.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewAppointment = new Appointment
            {
                PatientID = txtPatientID.Text.Trim(),
                PatientName = txtPatientName.Text.Trim(),
                Treatment = txtTreatment.Text.Trim(),
                Date = dpDate.SelectedDate.Value.ToString("MM/dd/yy"),   // Formatted nicely
                Time = txtTime.Text.Trim(),
                Type = (cmbType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Walk-in",
                Status = (cmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Pending"
            };

            DialogResult = true;
            Close();
        }
    }
}
