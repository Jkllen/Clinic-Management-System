using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Clinic_Management_System.Appointment_Functions;

namespace Clinic_Management_System
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Appointment> Appointments { get; set; }
        private ICollectionView _appointmentsView;

        public MainWindow()
        {
            InitializeComponent();

            Appointments = new ObservableCollection<Appointment>();
            _appointmentsView = CollectionViewSource.GetDefaultView(Appointments);
            AppointmentDataGrid.ItemsSource = _appointmentsView;

            // Optional: allow DataGrid to be editable
            AppointmentDataGrid.IsReadOnly = false;
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string search = SearchBox.Text.ToLower();

            _appointmentsView.Filter = obj =>
            {
                if (obj is Appointment a)
                {
                    return a.PatientID.ToLower().Contains(search) ||
                           a.PatientName.ToLower().Contains(search) ||
                           a.Treatment.ToLower().Contains(search) ||
                           a.Date.ToLower().Contains(search) ||
                           a.Time.ToLower().Contains(search) ||
                           a.Type.ToLower().Contains(search) ||
                           a.Status.ToLower().Contains(search);
                }
                return false;
            };
        }

        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            AddAppointmentWindow addWindow = new AddAppointmentWindow();

            if (addWindow.ShowDialog() == true)
            {
                Appointments.Add(addWindow.NewAppointment!);
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            FilterWindow filter = new FilterWindow();

            if (filter.ShowDialog() == true)
            {
                _appointmentsView.Filter = obj =>
                {
                    if (obj is Appointment a)
                    {
                        bool typeMatch = filter.SelectedType == "Any" || a.Type == filter.SelectedType;
                        bool statusMatch = filter.SelectedStatus == "Any" || a.Status == filter.SelectedStatus;
                        return typeMatch && statusMatch;
                    }
                    return false;
                };
            }
        }

        private void SortBy_Click(object sender, RoutedEventArgs e)
        {
            SortWindow sortWindow = new SortWindow();

            if (sortWindow.ShowDialog() == true)
            {
                _appointmentsView.SortDescriptions.Clear();
                _appointmentsView.SortDescriptions.Add(new SortDescription(nameof(Appointment.PatientID),
                    sortWindow.IsAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
            }
        }
    }

    public class Appointment : INotifyPropertyChanged
    {
        private string _patientID = string.Empty;
        private string _patientName = string.Empty;
        private string _treatment = string.Empty;
        private string _date = string.Empty;
        private string _time = string.Empty;
        private string _type = string.Empty;
        private string _status = "Pending";

        public string PatientID
        {
            get => _patientID;
            set { _patientID = value; OnPropertyChanged(nameof(PatientID)); }
        }

        public string PatientName
        {
            get => _patientName;
            set { _patientName = value; OnPropertyChanged(nameof(PatientName)); }
        }

        public string Treatment
        {
            get => _treatment;
            set { _treatment = value; OnPropertyChanged(nameof(Treatment)); }
        }

        public string Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(nameof(Date)); }
        }

        public string Time
        {
            get => _time;
            set { _time = value; OnPropertyChanged(nameof(Time)); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
