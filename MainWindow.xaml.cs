using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

namespace Clinic_Management_System
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Appointment> Appointments { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Appointments = new ObservableCollection<Appointment>
            {
                new Appointment
                {
                    PatientID="0001",
                    PatientName="Augustine L. Barredo",
                    Treatment="Extraction",
                    Date="10/28/25",
                    Time="10:30 am",
                    Type="Walk-in",
                    Status="Cancelled"
                }
            };

            AppointmentDataGrid.ItemsSource = Appointments;
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Filter clicked!");
        }

        private void SortBy_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sort clicked!");
        }

        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add Appointment clicked!");
        }

        // ✅ THIS MUST BE INSIDE MainWindow CLASS
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string search = SearchBox.Text.ToLower();

            var filtered = Appointments.Where(a =>
                a.PatientID.ToLower().Contains(search) ||
                a.PatientName.ToLower().Contains(search) ||
                a.Treatment.ToLower().Contains(search) ||
                a.Date.ToLower().Contains(search) ||
                a.Time.ToLower().Contains(search) ||
                a.Type.ToLower().Contains(search) ||
                a.Status.ToLower().Contains(search)
            ).ToList();

            AppointmentDataGrid.ItemsSource = filtered;
        }
    }

    public class Appointment
    {
        public required string PatientID { get; set; }
        public required string PatientName { get; set; }
        public required string Treatment { get; set; }
        public required string Date { get; set; }
        public required string Time { get; set; }
        public required string Type { get; set; }
        public required string Status { get; set; }
    }
}
