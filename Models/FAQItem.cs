using System.ComponentModel;

namespace CruzNeryClinic.Models
{
    public class FaqItem : INotifyPropertyChanged
    {
        private bool _isExpanded;

        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
