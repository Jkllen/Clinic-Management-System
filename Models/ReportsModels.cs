namespace CruzNeryClinic.Models
{
    public class ChartDataPoint
    {
        public string Label { get; set; } = "";
        public double Value { get; set; }
    }

    public class DualChartDataPoint
    {
        public string Label { get; set; } = "";
        public double Value1 { get; set; }
        public double Value2 { get; set; }
    }

    public class PieChartSlice
    {
        public string Label { get; set; } = "";
        public double Value { get; set; }
        public string HexColor { get; set; } = "#223357";
        public double Percentage { get; set; }
    }

    public class PatientVisitReportItem
    {
        public string Date { get; set; } = "";
        public string PatientCode { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string VisitType { get; set; } = "";
        public string Service { get; set; } = "";
        public string Dentist { get; set; } = "";
    }

    public class TransactionReportItem
    {
        public string Date { get; set; } = "";
        public string ReceiptNumber { get; set; } = "";
        public string PatientCode { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string Service { get; set; } = "";
        public double Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
    }

    public class InventoryReportItem
    {
        public string ItemName { get; set; } = "";
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public string LastRestocked { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusColor { get; set; } = "#333333";
    }

    public class ActivityLogReportItem
    {
        public string Timestamp { get; set; } = "";
        public string Role { get; set; } = "";
        public string Name { get; set; } = "";
        public string Action { get; set; } = "";
        public string ActionColor { get; set; } = "#333333";
        public string Module { get; set; } = "";
        public string Details { get; set; } = "";
    }
}
