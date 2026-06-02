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
                _vm.ChartDataRefreshed -= OnChartDataRefreshed;

            _vm = e.NewValue as DashboardViewModel;

            if (_vm != null)
                _vm.ChartDataRefreshed += OnChartDataRefreshed;
        }

        private void OnChartDataRefreshed()
            => Dispatcher.InvokeAsync(RenderCharts);

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
