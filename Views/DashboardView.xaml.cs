using CruzNeryClinic.ViewModels;
using CruzNeryClinic.Views.Charts;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    // DashboardView now contains only the dashboard content.
    // The reusable sidebar is handled by MainShellView.
    public partial class DashboardView : UserControl
    {
        private DashboardViewModel? _vm;

        public DashboardView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_vm != null)
            {
                _vm.ChartDataRefreshed -= OnChartDataRefreshed;
                _vm.ChartDetailRequested -= OnChartDetailRequested;
            }

            _vm = e.NewValue as DashboardViewModel;

            if (_vm != null)
            {
                _vm.ChartDataRefreshed += OnChartDataRefreshed;
                _vm.ChartDetailRequested += OnChartDetailRequested;
            }
        }

        private void OnChartDataRefreshed()
            => Dispatcher.InvokeAsync(RenderCharts);

        // Opens the right-side detail panel for the clicked chart.
        private void Chart_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_vm != null && sender is FrameworkElement fe && fe.Tag is string key)
                _vm.OpenChartDetailCommand.Execute(key);
        }

        private void OnChartDetailRequested()
            => Dispatcher.InvokeAsync(RenderChartDetail);

        private void ChartDetailCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
            => RenderChartDetail();

        // Renders the selected chart into the detail panel's canvas.
        private void RenderChartDetail()
        {
            if (_vm == null || !_vm.IsChartDetailOpen) return;

            switch (_vm.SelectedChartKey)
            {
                case "patientVisit":
                    ChartRenderer.RenderLineChart(ChartDetailCanvas, _vm.PatientVisitTrend, "#0EA5E9", "#50C878");
                    break;
                case "revenue":
                    ChartRenderer.RenderAreaChart(ChartDetailCanvas, _vm.RevenueTrend, "#50C878");
                    break;
            }
        }

        private void RenderCharts()
        {
            if (_vm == null || !_vm.CanViewAdminDashboardAnalytics) return;

            ChartRenderer.RenderLineChart(DashPatientVisitCanvas, _vm.PatientVisitTrend, "#0EA5E9", "#50C878");
            ChartRenderer.RenderAreaChart(DashRevenueCanvas, _vm.RevenueTrend, "#50C878");
        }

        private void DashPatientVisitCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.CanViewAdminDashboardAnalytics == true)
                ChartRenderer.RenderLineChart(DashPatientVisitCanvas, _vm.PatientVisitTrend, "#0EA5E9", "#50C878");
        }

        private void DashRevenueCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.CanViewAdminDashboardAnalytics == true)
                ChartRenderer.RenderAreaChart(DashRevenueCanvas, _vm.RevenueTrend, "#50C878");
        }
    }
}
