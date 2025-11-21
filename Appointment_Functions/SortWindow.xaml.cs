using System.Windows;

namespace Clinic_Management_System.Appointment_Functions
{
    public partial class SortWindow : Window
    {
        // Property used by MainWindow to know the sort order
        public bool IsAscending { get; private set; } = true;

        public SortWindow()
        {
            InitializeComponent();
        }

        private void Ascending_Click(object sender, RoutedEventArgs e)
        {
            IsAscending = true;
            DialogResult = true;
            Close();
        }

        private void Descending_Click(object sender, RoutedEventArgs e)
        {
            IsAscending = false;
            DialogResult = true;
            Close();
        }
    }
}
