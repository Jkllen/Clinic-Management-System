using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CruzNeryClinic.ViewModels
{
    // BaseViewModel is the parent class for all ViewModels.
    // It allows the UI to update automatically when a property changes.
    public class BaseViewModel : INotifyPropertyChanged
    {
        // This event tells the UI that a property value has changed.
        public event PropertyChangedEventHandler? PropertyChanged;

        // This method triggers the PropertyChanged event.
        // The CallerMemberName automatically gets the property name.
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // This helper updates a property only when the value actually changes.
        // It prevents unnecessary UI refreshes.
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}