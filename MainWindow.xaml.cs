using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Clinic_Management_System.Appointment_Functions;

namespace Clinic_Management_System
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Appointment> Appointments { get; set; }

        public MainWindow()
        {
            InitializeComponent();

          Appointments = new ObservableCollection<Appointment>();
            AppointmentDataGrid.ItemsSource = Appointments;
        }

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

        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            AddAppointmentWindow addWindow = new AddAppointmentWindow();

            if (addWindow.ShowDialog() == true)
            {
                Appointments.Add(addWindow.NewAppointment!);
                AppointmentDataGrid.ItemsSource = Appointments;
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            FilterWindow filter = new FilterWindow();

            if (filter.ShowDialog() == true)
            {
                var filtered = Appointments.Where(a =>
                    (filter.SelectedType == "Any" || a.Type == filter.SelectedType) &&
                    (filter.SelectedStatus == "Any" || a.Status == filter.SelectedStatus)
                ).ToList();

                AppointmentDataGrid.ItemsSource = filtered;
            }
        }

        private void SortBy_Click(object sender, RoutedEventArgs e)
        {
            SortWindow sortWindow = new SortWindow();

            if (sortWindow.ShowDialog() == true)
            {
                var sorted = sortWindow.IsAscending
                    ? Appointments.OrderBy(a => a.PatientID).ToList()
                    : Appointments.OrderByDescending(a => a.PatientID).ToList();

                AppointmentDataGrid.ItemsSource = sorted;
            }
        }
    }

    public class Appointment
    {
        public string PatientID { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
