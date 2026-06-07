using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CruzNeryClinic.Models
{
    public class AppointmentToothOption : INotifyPropertyChanged
    {
        private bool isSelected;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ToothNumber { get; set; }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected == value)
                    return;

                isSelected = value;
                OnPropertyChanged();
            }
        }

        public string DisplayText => ToothNumber.ToString();

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
