using System;

namespace CruzNeryClinic.Models.Dashboard
{
    // Represents a single day cell in the dashboard Appointment List day-strip.
    public class DashboardDayItem
    {
        public DateTime Date { get; set; }

        // Day number, e.g. "5".
        public string DayNumber => Date.Day.ToString();

        // Short weekday name, e.g. "Sun".
        public string DayName => Date.ToString("ddd");

        // True when this day is the currently selected date (highlighted in the strip).
        public bool IsSelected { get; set; }
    }
}
