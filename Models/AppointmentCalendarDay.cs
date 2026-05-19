using System;
using System.Windows;
using System.Windows.Media;

namespace CruzNeryClinic.Models
{
    public class AppointmentCalendarDay
    {
        public DateTime Date { get; set; }

        public int DayNumber => Date.Day;

        public int AppointmentCount { get; set; }

        public bool IsCurrentMonth { get; set; }

        public bool IsToday { get; set; }

        public bool IsSelected { get; set; }

        public bool HasAppointments => AppointmentCount > 0;

        public Visibility AppointmentCountVisibility =>
            HasAppointments ? Visibility.Visible : Visibility.Collapsed;

        public Brush DayForeground =>
            !IsCurrentMonth ? Brushes.Gray :
            IsSelected ? Brushes.White :
            Brushes.Black;

        public Brush DayBackground =>
            IsSelected ? new SolidColorBrush(Color.FromRgb(7, 60, 152)) :
            HasAppointments ? new SolidColorBrush(Color.FromRgb(220, 242, 255)) :
            Brushes.Transparent;

        public Brush DayBorderBrush =>
            IsToday ? new SolidColorBrush(Color.FromRgb(47, 152, 208)) :
            Brushes.Transparent;
    }
}