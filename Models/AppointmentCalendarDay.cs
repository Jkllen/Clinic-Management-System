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
            !IsCurrentMonth ? new SolidColorBrush(Color.FromRgb(193, 199, 209)) :
            IsSelected ? Brushes.White :
            IsToday ? new SolidColorBrush(Color.FromRgb(7, 60, 152)) :
            new SolidColorBrush(Color.FromRgb(45, 51, 64));

        public Brush DayBackground =>
            IsSelected ? new SolidColorBrush(Color.FromRgb(7, 60, 152)) :
            HasAppointments ? new SolidColorBrush(Color.FromRgb(227, 240, 255)) :
            Brushes.Transparent;

        // Today is shown with a brand-blue ring when it is not the selected day.
        public Brush DayBorderBrush =>
            IsSelected ? new SolidColorBrush(Color.FromRgb(7, 60, 152)) :
            IsToday ? new SolidColorBrush(Color.FromRgb(7, 60, 152)) :
            Brushes.Transparent;
    }
}