using CommunityToolkit.Mvvm.Input;
using CruzNeryClinic.Models;
using CruzNeryClinic.Repositories;
using CruzNeryClinic.Services;
using CruzNeryClinic.Views.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace CruzNeryClinic.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly ReportsRepository _repository = new();

        public event Action? ChartDataRefreshed;

        // ── Tab selection ──────────────────────────────────────────────────────

        private bool _isPatientVisitsSelected = true;
        private bool _isTransactionReportsSelected;
        private bool _isInventoryReportsSelected;
        private bool _isUserActivityLogSelected;

        public bool IsPatientVisitsSelected
        {
            get => _isPatientVisitsSelected;
            set => SetProperty(ref _isPatientVisitsSelected, value);
        }
        public bool IsTransactionReportsSelected
        {
            get => _isTransactionReportsSelected;
            set => SetProperty(ref _isTransactionReportsSelected, value);
        }
        public bool IsInventoryReportsSelected
        {
            get => _isInventoryReportsSelected;
            set => SetProperty(ref _isInventoryReportsSelected, value);
        }
        public bool IsUserActivityLogSelected
        {
            get => _isUserActivityLogSelected;
            set => SetProperty(ref _isUserActivityLogSelected, value);
        }

        // ── Filter state ───────────────────────────────────────────────────────

        private bool _isThisMonthSelected = true;
        private bool _isThisYearSelected;
        private bool _isCustomRangeSelected;
        private DateTime? _filterFromDate;
        private DateTime? _filterToDate;
        private string _showingText = "Showing: Monthly Report";
        private bool _isDateRangeVisible;
        private int _selectedFilterMonthIndex = DateTime.Now.Month - 1;
        private int _selectedFilterYear = DateTime.Now.Year;

        public List<string> MonthNames { get; } = new List<string>
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        public List<int> AvailableYears { get; } = new List<int>(
            System.Linq.Enumerable.Range(2020, DateTime.Now.Year - 2020 + 2));

        public int SelectedFilterMonthIndex
        {
            get => _selectedFilterMonthIndex;
            set
            {
                SetProperty(ref _selectedFilterMonthIndex, value);
                if (IsThisMonthSelected) UpdateMonthRange();
            }
        }

        public int SelectedFilterYear
        {
            get => _selectedFilterYear;
            set
            {
                SetProperty(ref _selectedFilterYear, value);
                if (IsThisMonthSelected) UpdateMonthRange();
                else if (IsThisYearSelected) UpdateYearRange();
            }
        }

        public bool IsThisMonthSelected
        {
            get => _isThisMonthSelected;
            set => SetProperty(ref _isThisMonthSelected, value);
        }
        public bool IsThisYearSelected
        {
            get => _isThisYearSelected;
            set => SetProperty(ref _isThisYearSelected, value);
        }
        public bool IsCustomRangeSelected
        {
            get => _isCustomRangeSelected;
            set => SetProperty(ref _isCustomRangeSelected, value);
        }
        public DateTime? FilterFromDate
        {
            get => _filterFromDate;
            set
            {
                SetProperty(ref _filterFromDate, value);
                if (IsCustomRangeSelected && _filterFromDate.HasValue && _filterToDate.HasValue)
                {
                    ShowingText = $"Showing: {_filterFromDate:MMM dd} – {_filterToDate:MMM dd, yyyy}";
                    LoadData();
                }
            }
        }
        public DateTime? FilterToDate
        {
            get => _filterToDate;
            set
            {
                SetProperty(ref _filterToDate, value);
                if (IsCustomRangeSelected && _filterFromDate.HasValue && _filterToDate.HasValue)
                {
                    ShowingText = $"Showing: {_filterFromDate:MMM dd} – {_filterToDate:MMM dd, yyyy}";
                    LoadData();
                }
            }
        }
        public bool IsDateRangeVisible
        {
            get => _isDateRangeVisible;
            set => SetProperty(ref _isDateRangeVisible, value);
        }
        public string ShowingText
        {
            get => _showingText;
            set => SetProperty(ref _showingText, value);
        }

        // ── Print preview state ────────────────────────────────────────────────

        private bool _isReportPrintPreviewOpen;
        public bool IsReportPrintPreviewOpen
        {
            get => _isReportPrintPreviewOpen;
            set => SetProperty(ref _isReportPrintPreviewOpen, value);
        }

        // File URI of the generated report HTML, navigated to in the preview WebView2.
        // Each print writes a uniquely-named temp file so the preview always reloads
        // and the printout footer shows a short path instead of a giant data URI.
        private Uri? _reportPrintPreviewUri;
        public Uri? ReportPrintPreviewUri
        {
            get => _reportPrintPreviewUri;
            private set => SetProperty(ref _reportPrintPreviewUri, value);
        }

        // Title shown on the preview overlay, reflecting the active report tab.
        public string ReportPrintPreviewTitle => $"{GetActiveReportTitle()} — Print Preview";

        // ── Chart & table data ─────────────────────────────────────────────────

        public ObservableCollection<PatientVisitReportItem> PatientVisitsItems { get; } = new();
        public List<DualChartDataPoint> PatientVisitTrend { get; private set; } = new();

        public ObservableCollection<TransactionReportItem> TransactionItems { get; } = new();
        public List<ChartDataPoint> RevenueTrend { get; private set; } = new();
        public List<ChartDataPoint> DailyTransactionCounts { get; private set; } = new();

        public ObservableCollection<InventoryReportItem> InventoryItems { get; } = new();
        public List<ChartDataPoint> InventoryChartData { get; private set; } = new();

        public ObservableCollection<ActivityLogReportItem> ActivityLogItems { get; } = new();
        public List<PieChartSlice> ActivityByType { get; private set; } = new();
        public List<ChartDataPoint> ActivityByModule { get; private set; } = new();

        // Plain-language interpretation shown under each chart.
        private string _patientVisitInsight = "";
        public string PatientVisitInsight
        {
            get => _patientVisitInsight;
            private set => SetProperty(ref _patientVisitInsight, value);
        }

        private string _revenueInsight = "";
        public string RevenueInsight
        {
            get => _revenueInsight;
            private set => SetProperty(ref _revenueInsight, value);
        }

        private string _dailyTransactionsInsight = "";
        public string DailyTransactionsInsight
        {
            get => _dailyTransactionsInsight;
            private set => SetProperty(ref _dailyTransactionsInsight, value);
        }

        private string _inventoryInsight = "";
        public string InventoryInsight
        {
            get => _inventoryInsight;
            private set => SetProperty(ref _inventoryInsight, value);
        }

        private string _activityByTypeInsight = "";
        public string ActivityByTypeInsight
        {
            get => _activityByTypeInsight;
            private set => SetProperty(ref _activityByTypeInsight, value);
        }

        private string _activityByModuleInsight = "";
        public string ActivityByModuleInsight
        {
            get => _activityByModuleInsight;
            private set => SetProperty(ref _activityByModuleInsight, value);
        }

        // ── Commands ───────────────────────────────────────────────────────────

        public ICommand SelectPatientVisitsCommand { get; }
        public ICommand SelectTransactionReportsCommand { get; }
        public ICommand SelectInventoryReportsCommand { get; }
        public ICommand SelectUserActivityLogCommand { get; }
        public ICommand ApplyThisMonthCommand { get; }
        public ICommand ApplyThisYearCommand { get; }
        public ICommand ApplyCustomRangeCommand { get; }
        public ICommand PrintReportCommand { get; }
        public ICommand CloseReportPrintPreviewCommand { get; }

        public ReportsViewModel()
        {
            SelectPatientVisitsCommand = new RelayCommand(SelectPatientVisits);
            SelectTransactionReportsCommand = new RelayCommand(SelectTransactionReports);
            SelectInventoryReportsCommand = new RelayCommand(SelectInventoryReports);
            SelectUserActivityLogCommand = new RelayCommand(SelectUserActivityLog);
            ApplyThisMonthCommand = new RelayCommand(ApplyThisMonth);
            ApplyThisYearCommand = new RelayCommand(ApplyThisYear);
            ApplyCustomRangeCommand = new RelayCommand(ApplyCustomRange);
            PrintReportCommand = new RelayCommand(PrintReport);
            CloseReportPrintPreviewCommand = new RelayCommand(() => IsReportPrintPreviewOpen = false);

            ApplyThisMonth();
        }

        // ── Tab switching ──────────────────────────────────────────────────────

        private void SelectPatientVisits()
        {
            IsPatientVisitsSelected = true;
            IsTransactionReportsSelected = false;
            IsInventoryReportsSelected = false;
            IsUserActivityLogSelected = false;
            LoadData();
        }

        private void SelectTransactionReports()
        {
            IsPatientVisitsSelected = false;
            IsTransactionReportsSelected = true;
            IsInventoryReportsSelected = false;
            IsUserActivityLogSelected = false;
            LoadData();
        }

        private void SelectInventoryReports()
        {
            IsPatientVisitsSelected = false;
            IsTransactionReportsSelected = false;
            IsInventoryReportsSelected = true;
            IsUserActivityLogSelected = false;
            LoadData();
        }

        private void SelectUserActivityLog()
        {
            IsPatientVisitsSelected = false;
            IsTransactionReportsSelected = false;
            IsInventoryReportsSelected = false;
            IsUserActivityLogSelected = true;
            LoadData();
        }

        // ── Filter switching ───────────────────────────────────────────────────

        private void ApplyThisMonth()
        {
            IsThisMonthSelected = true;
            IsThisYearSelected = false;
            IsCustomRangeSelected = false;
            _selectedFilterMonthIndex = DateTime.Now.Month - 1;
            _selectedFilterYear = DateTime.Now.Year;
            OnPropertyChanged(nameof(SelectedFilterMonthIndex));
            OnPropertyChanged(nameof(SelectedFilterYear));
            UpdateMonthRange();
        }

        private void ApplyThisYear()
        {
            IsThisMonthSelected = false;
            IsThisYearSelected = true;
            IsCustomRangeSelected = false;
            _selectedFilterYear = DateTime.Now.Year;
            OnPropertyChanged(nameof(SelectedFilterYear));
            UpdateYearRange();
        }

        private void ApplyCustomRange()
        {
            IsThisMonthSelected = false;
            IsThisYearSelected = false;
            IsCustomRangeSelected = true;
            ShowingText = "Showing: Custom Range";
        }

        private void UpdateMonthRange()
        {
            int month = _selectedFilterMonthIndex + 1;
            var first = new DateTime(_selectedFilterYear, month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            _filterFromDate = first;
            _filterToDate = last;
            OnPropertyChanged(nameof(FilterFromDate));
            OnPropertyChanged(nameof(FilterToDate));
            ShowingText = $"Showing: {first:MMMM yyyy}";
            LoadData();
        }

        private void UpdateYearRange()
        {
            _filterFromDate = new DateTime(_selectedFilterYear, 1, 1);
            _filterToDate = new DateTime(_selectedFilterYear, 12, 31);
            OnPropertyChanged(nameof(FilterFromDate));
            OnPropertyChanged(nameof(FilterToDate));
            ShowingText = $"Showing: {_selectedFilterYear}";
            LoadData();
        }

        // ── Data loading ───────────────────────────────────────────────────────

        private (string from, string to) GetDateRange()
        {
            if (_filterFromDate.HasValue && _filterToDate.HasValue)
                return (_filterFromDate.Value.ToString("yyyy-MM-dd"), _filterToDate.Value.ToString("yyyy-MM-dd"));

            var now = DateTime.Now;
            var first = new DateTime(now.Year, now.Month, 1);
            var last = first.AddMonths(1).AddDays(-1);
            return (first.ToString("yyyy-MM-dd"), last.ToString("yyyy-MM-dd"));
        }

        // ── Print (WebView2 preview) ─────────────────────────────────────────────

        private string GetActiveReportTitle()
        {
            if (IsTransactionReportsSelected) return "Transaction Report";
            if (IsInventoryReportsSelected) return "Inventory Report";
            if (IsUserActivityLogSelected) return "User Activity Log";
            return "Patient Visits Report";
        }

        // Builds an HTML version of the active report (charts + table) and opens
        // it in the WebView2 print-preview overlay.
        private void PrintReport()
        {
            string html = BuildReportHtml();

            // Unique file name per print: forces a reload and keeps the printed
            // footer URL short instead of a long base64 data URI.
            string filePath = Path.Combine(
                Path.GetTempPath(),
                $"ClinicReport_{DateTime.Now:yyyyMMddHHmmssfff}.html");

            File.WriteAllText(filePath, html, Encoding.UTF8);

            ReportPrintPreviewUri = new Uri(filePath);
            OnPropertyChanged(nameof(ReportPrintPreviewTitle));
            IsReportPrintPreviewOpen = true;
        }

        private string BuildReportHtml()
        {
            string title = GetActiveReportTitle();

            string charts;
            string headerCells;
            StringBuilder rows = new();
            var summary = new List<(string Label, string Value)>();

            if (IsTransactionReportsSelected)
            {
                charts = BarChartSvg("Revenue Trend", RevenueTrend.Select(p => (p.Label, p.Value)), "#50C878", "₱")
                       + BarChartSvg("Daily Transactions", DailyTransactionCounts.Select(p => (p.Label, p.Value)), "#2F98D0", "");

                headerCells = Th("Transaction ID", "Date", "Patient ID", "Patient Name", "Description", "Amount", "Payment Method");
                foreach (TransactionReportItem t in TransactionItems)
                    rows.Append(Tr(t.ReceiptNumber, t.Date, t.PatientCode, t.PatientName, t.Service, $"₱ {t.Amount:N2}", t.PaymentMethod));

                decimal totalRevenue = TransactionItems.Sum(t => (decimal)t.Amount);
                summary.Add(("Total Transactions", TransactionItems.Count.ToString("N0")));
                summary.Add(("Total Revenue", $"₱ {totalRevenue:N2}"));
                summary.Add(("Patients Billed", TransactionItems.Select(t => t.PatientCode).Distinct().Count().ToString("N0")));

                // Subtotals per payment method.
                foreach (var grp in TransactionItems
                             .GroupBy(t => string.IsNullOrWhiteSpace(t.PaymentMethod) ? "Unspecified" : t.PaymentMethod)
                             .OrderByDescending(g => g.Sum(t => t.Amount)))
                {
                    summary.Add(($"{grp.Key}", $"₱ {grp.Sum(t => (decimal)t.Amount):N2}"));
                }
            }
            else if (IsInventoryReportsSelected)
            {
                charts = BarChartSvg("Stock Levels", InventoryChartData.Select(p => (p.Label, p.Value)), "#FF981D", "");

                headerCells = Th("Item Name", "Quantity on Hand", "Reorder Level", "Last Restocked", "Status");
                foreach (InventoryReportItem i in InventoryItems)
                    rows.Append(Tr(i.ItemName, i.CurrentStock.ToString("N0"), i.Threshold.ToString("N0"), i.LastRestocked, i.Status));

                int lowStock = InventoryItems.Count(i => i.CurrentStock <= i.Threshold);
                summary.Add(("Total Items", InventoryItems.Count.ToString("N0")));
                summary.Add(("Low / Out of Stock", lowStock.ToString("N0")));
                summary.Add(("Units in Stock", InventoryItems.Sum(i => i.CurrentStock).ToString("N0")));
            }
            else if (IsUserActivityLogSelected)
            {
                charts = PieChartSvg("Activity by Type", ActivityByType)
                       + BarChartSvg("Activity by Module", ActivityByModule.Select(p => (p.Label, p.Value)), "#A855F7", "");

                headerCells = Th("Date / Time", "User", "Role", "Action", "Module", "Details");
                foreach (ActivityLogReportItem a in ActivityLogItems)
                    rows.Append(Tr(a.Timestamp, a.Name, a.Role, a.Action, a.Module, a.Details));

                string mostActive = ActivityLogItems
                    .GroupBy(a => a.Name)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "—";
                string topAction = ActivityLogItems
                    .GroupBy(a => a.Action)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "—";

                summary.Add(("Total Actions Logged", ActivityLogItems.Count.ToString("N0")));
                summary.Add(("Most Active User", string.IsNullOrWhiteSpace(mostActive) ? "—" : mostActive));
                summary.Add(("Most Frequent Action", string.IsNullOrWhiteSpace(topAction) ? "—" : topAction));
            }
            else
            {
                charts = GroupedBarChartSvg(
                    "Patient Visits Trend",
                    PatientVisitTrend.Select(p => (p.Label, p.Value1, p.Value2)),
                    "Scheduled", "#0EA5E9",
                    "Walk-in", "#50C878");

                headerCells = Th("Date", "Patient ID", "Patient Name", "Visit Type", "Service", "Dentist");
                foreach (PatientVisitReportItem p in PatientVisitsItems)
                    rows.Append(Tr(p.Date, p.PatientCode, p.PatientName, p.VisitType, p.Service, p.Dentist));

                double scheduled = PatientVisitTrend.Sum(p => p.Value1);
                double walkIn = PatientVisitTrend.Sum(p => p.Value2);

                // Busiest day/period from the trend.
                var busiest = PatientVisitTrend
                    .OrderByDescending(p => p.Value1 + p.Value2)
                    .FirstOrDefault();
                string busiestLabel = busiest != null && (busiest.Value1 + busiest.Value2) > 0
                    ? busiest.Label
                    : "—";

                summary.Add(("Total Visits", PatientVisitsItems.Count.ToString("N0")));
                summary.Add(("Scheduled", scheduled.ToString("N0")));
                summary.Add(("Walk-in", walkIn.ToString("N0")));
                summary.Add(("Busiest Day / Period", busiestLabel));
            }

            int columnCount = headerCells.Split("<th").Length - 1;
            if (rows.Length == 0)
                rows.Append($"<tr><td colspan=\"{columnCount}\" class=\"empty\">No records for the selected period.</td></tr>");

            StringBuilder cards = new();
            foreach (var (label, value) in summary)
                cards.Append($@"<li><span class=""sum-lbl"">{HtmlEncode(label)}:</span> <span class=""sum-val"">{HtmlEncode(value)}</span></li>");

            string generatedBy = SessionService.GetCurrentUserFullName();
            if (string.IsNullOrWhiteSpace(generatedBy)) generatedBy = "—";

            string generatedOn = DateTime.Now.ToString("MMMM dd, yyyy h:mm tt");

            Version? v = Assembly.GetExecutingAssembly().GetName().Version;
            string appVersion = v != null ? $"v{v.Major}.{v.Minor}.{v.Build}" : "v1.0";

            return $@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"" />
<style>
    * {{ box-sizing: border-box; }}
    body {{ font-family: 'Segoe UI', Arial, sans-serif; color: #1f2430; margin: 26px 32px 70px 32px; }}
    .report-header {{ display: flex; align-items: flex-start; border-bottom: 3px solid #073C98; padding-bottom: 12px; margin-bottom: 16px; }}
    .logo {{ width: 46px; height: 46px; border-radius: 10px; background: #073C98; color: #fff; font-weight: 700; font-size: 20px; display: flex; align-items: center; justify-content: center; margin-right: 14px; }}
    .logo-img {{ width: 48px; height: 48px; object-fit: contain; margin-right: 14px; }}
    .head-text {{ flex: 1; }}
    .clinic {{ font-size: 20px; font-weight: 700; color: #073C98; }}
    .report-title {{ font-size: 17px; font-weight: 700; margin-top: 2px; }}
    .head-meta {{ text-align: right; font-size: 11px; color: #666; line-height: 1.5; }}
    .head-meta b {{ color: #333; }}
    h2.section {{ font-size: 14px; color: #223357; margin: 18px 0 8px 0; }}
    .sumlist {{ list-style: none; margin: 0; padding: 0; columns: 2; column-gap: 40px; font-size: 13px; line-height: 2; }}
    .sumlist li {{ break-inside: avoid; border-bottom: 1px dotted #e2e2e2; }}
    .sum-lbl {{ color: #223357; font-weight: 600; }}
    .sum-val {{ font-weight: 700; float: right; }}
    table {{ width: 100%; border-collapse: collapse; font-size: 11px; table-layout: fixed; }}
    th {{ background: #223357; color: #fff; text-align: left; padding: 7px 9px; }}
    td {{ padding: 6px 9px; border-bottom: 1px solid #e2e2e2; word-wrap: break-word; overflow-wrap: anywhere; }}
    tr:nth-child(even) td {{ background: #f6f8fb; }}
    .empty {{ text-align: center; color: #888; padding: 24px; font-style: italic; }}
    .charts {{ display: flex; flex-wrap: nowrap; gap: 18px; align-items: stretch; }}
    .chart {{ border: 1px solid #e8e8e8; border-radius: 8px; padding: 12px 14px; flex: 1 1 0; min-width: 0; }}
    .chart h2 {{ font-size: 13px; margin: 0 0 8px 0; color: #223357; }}
    .legend {{ font-size: 11px; color: #555; margin-top: 6px; }}
    .legend span {{ display: inline-block; width: 10px; height: 10px; border-radius: 2px; margin: 0 4px 0 12px; vertical-align: middle; }}
    .pie-legend {{ font-size: 11px; color: #555; }}
    .pie-leg {{ margin: 3px 0; }}
    .pie-leg span {{ display: inline-block; width: 10px; height: 10px; border-radius: 2px; margin-right: 6px; vertical-align: middle; }}
    .report-footer {{ position: fixed; bottom: 0; left: 0; right: 0; border-top: 1px solid #ddd; padding: 8px 32px; font-size: 10px; color: #888; display: flex; justify-content: space-between; background: #fff; }}
    @media print {{ body {{ margin: 0 0 70px 0; padding: 26px 32px; }} .chart, .stat {{ break-inside: avoid; }} thead {{ display: table-header-group; }} tr {{ break-inside: avoid; }} }}
</style>
</head>
<body>
    <div class=""report-header"">
        {GetLogoHtml()}
        <div class=""head-text"">
            <div class=""clinic"">Cruz Nery Dental Clinic</div>
            <div class=""report-title"">{HtmlEncode(title)}</div>
        </div>
        <div class=""head-meta"">
            <div><b>Period:</b> {HtmlEncode(ShowingText.Replace("Showing: ", string.Empty))}</div>
            <div><b>Generated by:</b> {HtmlEncode(generatedBy)}</div>
            <div><b>Date:</b> {HtmlEncode(generatedOn)}</div>
        </div>
    </div>

    <h2 class=""section"">Overview</h2>
    <div class=""charts"">{charts}</div>

    <h2 class=""section"">Detailed Records</h2>
    <table>
        <thead><tr>{headerCells}</tr></thead>
        <tbody>{rows}</tbody>
    </table>

    <h2 class=""section"">Summary</h2>
    <ul class=""sumlist"">{cards}</ul>

    <div class=""report-footer"">
        <span>Cruz Nery Clinic Management System {appVersion}</span>
        <span>{HtmlEncode(title)} &nbsp;•&nbsp; {HtmlEncode(generatedOn)}</span>
    </div>
</body>
</html>";
        }

        // Inline SVG bar chart for a single data series.
        private static string BarChartSvg(string title, IEnumerable<(string Label, double Value)> data, string color, string unitPrefix)
        {
            var points = data.ToList();

            if (points.Count == 0)
                return $@"<div class=""chart""><h2>{HtmlEncode(title)}</h2><div class=""empty"">No chart data.</div></div>";

            double max = points.Max(p => p.Value);
            if (max <= 0) max = 1;

            const int width = 460, height = 200, padLeft = 8, padBottom = 34, padTop = 8;
            int plotH = height - padBottom - padTop;
            double slot = (double)(width - padLeft) / points.Count;
            double barW = Math.Max(6, slot * 0.6);

            StringBuilder bars = new();
            for (int i = 0; i < points.Count; i++)
            {
                double v = points[i].Value;
                double h = v / max * plotH;
                double x = padLeft + i * slot + (slot - barW) / 2;
                double y = padTop + (plotH - h);

                string valueText = unitPrefix == "₱" ? $"₱{v:N0}" : Inv(v);

                bars.Append($@"<rect x=""{Inv(x)}"" y=""{Inv(y)}"" width=""{Inv(barW)}"" height=""{Inv(h)}"" fill=""{color}"" rx=""2"" />");
                bars.Append($@"<text x=""{Inv(x + barW / 2)}"" y=""{Inv(y - 3)}"" font-size=""8"" fill=""#444"" text-anchor=""middle"">{HtmlEncode(valueText)}</text>");
                bars.Append($@"<text x=""{Inv(x + barW / 2)}"" y=""{height - padBottom + 12}"" font-size=""8"" fill=""#777"" text-anchor=""middle"">{HtmlEncode(ShortLabel(points[i].Label))}</text>");
            }

            return $@"<div class=""chart""><h2>{HtmlEncode(title)}</h2>
<svg viewBox=""0 0 {width} {height}"" width=""100%"" preserveAspectRatio=""xMidYMid meet"">
<line x1=""{padLeft}"" y1=""{height - padBottom}"" x2=""{width}"" y2=""{height - padBottom}"" stroke=""#ddd"" />
{bars}
</svg></div>";
        }

        // Inline SVG grouped bar chart for a two-series (e.g. Scheduled vs Walk-in) trend.
        private static string GroupedBarChartSvg(
            string title,
            IEnumerable<(string Label, double Value1, double Value2)> data,
            string name1, string color1,
            string name2, string color2)
        {
            var points = data.ToList();

            if (points.Count == 0)
                return $@"<div class=""chart""><h2>{HtmlEncode(title)}</h2><div class=""empty"">No chart data.</div></div>";

            double max = points.Max(p => Math.Max(p.Value1, p.Value2));
            if (max <= 0) max = 1;

            const int width = 460, height = 200, padLeft = 8, padBottom = 34, padTop = 8;
            int plotH = height - padBottom - padTop;
            double slot = (double)(width - padLeft) / points.Count;
            double barW = Math.Max(4, slot * 0.28);

            StringBuilder bars = new();
            for (int i = 0; i < points.Count; i++)
            {
                double h1 = points[i].Value1 / max * plotH;
                double h2 = points[i].Value2 / max * plotH;
                double groupX = padLeft + i * slot + (slot - barW * 2 - 3) / 2;

                double x1 = groupX, y1 = padTop + (plotH - h1);
                double x2 = groupX + barW + 3, y2 = padTop + (plotH - h2);

                bars.Append($@"<rect x=""{Inv(x1)}"" y=""{Inv(y1)}"" width=""{Inv(barW)}"" height=""{Inv(h1)}"" fill=""{color1}"" rx=""2"" />");
                bars.Append($@"<rect x=""{Inv(x2)}"" y=""{Inv(y2)}"" width=""{Inv(barW)}"" height=""{Inv(h2)}"" fill=""{color2}"" rx=""2"" />");
                bars.Append($@"<text x=""{Inv(groupX + barW)}"" y=""{height - padBottom + 12}"" font-size=""8"" fill=""#777"" text-anchor=""middle"">{HtmlEncode(ShortLabel(points[i].Label))}</text>");
            }

            return $@"<div class=""chart""><h2>{HtmlEncode(title)}</h2>
<svg viewBox=""0 0 {width} {height}"" width=""100%"" preserveAspectRatio=""xMidYMid meet"">
<line x1=""{padLeft}"" y1=""{height - padBottom}"" x2=""{width}"" y2=""{height - padBottom}"" stroke=""#ddd"" />
{bars}
</svg>
<div class=""legend""><span style=""background:{color1}""></span>{HtmlEncode(name1)}<span style=""background:{color2}""></span>{HtmlEncode(name2)}</div></div>";
        }

        // Inline SVG pie chart (used for Activity by Type).
        private static string PieChartSvg(string title, IEnumerable<PieChartSlice> slices)
        {
            var data = slices.Where(s => s.Value > 0).ToList();

            if (data.Count == 0)
                return $@"<div class=""chart""><h2>{HtmlEncode(title)}</h2><div class=""empty"">No chart data.</div></div>";

            double total = data.Sum(s => s.Value);
            const double cx = 90, cy = 100, r = 80;

            StringBuilder paths = new();
            StringBuilder legend = new();
            double angle = -90; // start at top

            foreach (PieChartSlice s in data)
            {
                double sweep = s.Value / total * 360.0;
                double end = angle + sweep;

                double x1 = cx + r * Math.Cos(angle * Math.PI / 180);
                double y1 = cy + r * Math.Sin(angle * Math.PI / 180);
                double x2 = cx + r * Math.Cos(end * Math.PI / 180);
                double y2 = cy + r * Math.Sin(end * Math.PI / 180);
                int largeArc = sweep > 180 ? 1 : 0;

                // A single full slice would collapse the arc; draw a full circle instead.
                if (data.Count == 1)
                    paths.Append($@"<circle cx=""{Inv(cx)}"" cy=""{Inv(cy)}"" r=""{Inv(r)}"" fill=""{s.HexColor}"" />");
                else
                    paths.Append($@"<path d=""M{Inv(cx)},{Inv(cy)} L{Inv(x1)},{Inv(y1)} A{Inv(r)},{Inv(r)} 0 {largeArc},1 {Inv(x2)},{Inv(y2)} Z"" fill=""{s.HexColor}"" />");

                double pct = s.Value / total * 100.0;
                legend.Append($@"<div class=""pie-leg""><span style=""background:{s.HexColor}""></span>{HtmlEncode(s.Label)} — {Inv(pct)}%</div>");

                angle = end;
            }

            return $@"<div class=""chart""><h2>{HtmlEncode(title)}</h2>
<div style=""display:flex;align-items:center;gap:16px;"">
<svg viewBox=""0 0 180 200"" width=""150"" height=""166"">{paths}</svg>
<div class=""pie-legend"">{legend}</div>
</div></div>";
        }

        // Trims long axis labels (e.g. dates) so the chart stays readable.
        private static string ShortLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return string.Empty;
            return label.Length > 6 ? label.Substring(label.Length - 5) : label;
        }

        // Embeds the clinic logo as a base64 data URI so it renders inside the
        // WebView2 NavigateToString preview. Falls back to a text badge.
        private static string GetLogoHtml()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "logo.png");
                if (File.Exists(path))
                {
                    string base64 = Convert.ToBase64String(File.ReadAllBytes(path));
                    return $@"<img class=""logo-img"" src=""data:image/png;base64,{base64}"" alt=""Clinic logo"" />";
                }
            }
            catch
            {
                // Fall through to the text badge.
            }

            return @"<div class=""logo"">CN</div>";
        }

        private static string Inv(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

        private static string Th(params string[] headers)
            => string.Concat(headers.Select(h => $"<th>{HtmlEncode(h)}</th>"));

        private static string Tr(params string[] cells)
            => $"<tr>{string.Concat(cells.Select(c => $"<td>{HtmlEncode(c)}</td>"))}</tr>";

        private static string HtmlEncode(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private void LoadData()
        {
            var (from, to) = GetDateRange();

            if (IsPatientVisitsSelected)
            {
                PatientVisitsItems.Clear();
                foreach (var item in _repository.GetPatientVisits(from, to))
                    PatientVisitsItems.Add(item);
                PatientVisitTrend = _repository.GetPatientVisitTrend(from, to);
                PatientVisitInsight = ChartInsight.SummarizeDual(PatientVisitTrend, "scheduled", "walk-in");
            }
            else if (IsTransactionReportsSelected)
            {
                TransactionItems.Clear();
                foreach (var item in _repository.GetTransactions(from, to))
                    TransactionItems.Add(item);
                RevenueTrend = _repository.GetRevenueTrend(from, to);
                DailyTransactionCounts = _repository.GetDailyTransactionCounts(from, to);
                RevenueInsight = ChartInsight.Summarize(RevenueTrend, "days", "₱");
                DailyTransactionsInsight = ChartInsight.Summarize(DailyTransactionCounts, "days");
            }
            else if (IsInventoryReportsSelected)
            {
                InventoryItems.Clear();
                foreach (var item in _repository.GetInventoryItems())
                    InventoryItems.Add(item);
                InventoryChartData = _repository.GetInventoryChartData();
                InventoryInsight = ChartInsight.Summarize(InventoryChartData, "items");
            }
            else if (IsUserActivityLogSelected)
            {
                ActivityLogItems.Clear();
                foreach (var item in _repository.GetActivityLogs(from, to))
                    ActivityLogItems.Add(item);
                ActivityByType = _repository.GetActivityByType(from, to);
                ActivityByModule = _repository.GetActivityByModule(from, to);
                ActivityByTypeInsight = ChartInsight.SummarizePie(ActivityByType);
                ActivityByModuleInsight = ChartInsight.Summarize(ActivityByModule, "modules");
            }

            ChartDataRefreshed?.Invoke();
        }
    }
}
