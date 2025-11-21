using System.Windows;
using System.Windows.Controls;

namespace Clinic_Management_System.Appointment_Functions
{
    public partial class FilterWindow : Window
    {
        public string SelectedType { get; private set; } = "Any";
        public string SelectedStatus { get; private set; } = "Any";

        public FilterWindow()
        {
            InitializeComponent();
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            SelectedType = (cmbType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Any";
            SelectedStatus = (cmbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Any";

            DialogResult = true;
            Close();
        }
    }
}
