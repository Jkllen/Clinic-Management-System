using CruzNeryClinic.ViewModels;
using CruzNeryClinic.Views.Charts;
using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    public partial class ReportsView : UserControl
    {
        private ReportsViewModel? _vm;

        public ReportsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_vm != null)
            {
                _vm.ChartDataRefreshed -= OnChartDataRefreshed;
                _vm.PropertyChanged -= OnViewModelPropertyChanged;
            }

            _vm = e.NewValue as ReportsViewModel;

            if (_vm != null)
            {
                _vm.ChartDataRefreshed += OnChartDataRefreshed;
                _vm.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnChartDataRefreshed()
            => Dispatcher.InvokeAsync(RenderActiveChart);

        private void RenderActiveChart()
        {
            if (_vm == null) return;

            if (_vm.IsPatientVisitsSelected)
                ChartRenderer.RenderLineChart(PatientVisitCanvas, _vm.PatientVisitTrend, "#0EA5E9", "#50C878");
            else if (_vm.IsTransactionReportsSelected)
            {
                ChartRenderer.RenderAreaChart(RevenueTrendCanvas, _vm.RevenueTrend, "#50C878");
                ChartRenderer.RenderBarChart(DailyTransactionsCanvas, _vm.DailyTransactionCounts, "#2F98D0");
            }
            else if (_vm.IsInventoryReportsSelected)
                ChartRenderer.RenderBarChart(InventoryBarCanvas, _vm.InventoryChartData, "#FF981D");
            else if (_vm.IsUserActivityLogSelected)
            {
                ChartRenderer.RenderPieChart(ActivityPieCanvas, _vm.ActivityByType);
                ChartRenderer.RenderBarChart(ActivityModuleCanvas, _vm.ActivityByModule, "#A855F7");
            }
        }

        // ── Print preview (WebView2) ─────────────────────────────────────────────

        private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_vm == null || e.PropertyName != nameof(ReportsViewModel.ReportPrintPreviewUri))
                return;

            if (_vm.ReportPrintPreviewUri == null)
                return;

            try
            {
                await ReportPrintWebView.EnsureCoreWebView2Async();
                ReportPrintWebView.Source = _vm.ReportPrintPreviewUri;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load report preview: {ex.Message}",
                    "Report Preview",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void OpenReportPrintDialog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ReportPrintWebView.EnsureCoreWebView2Async();

                CoreWebView2? webView = ReportPrintWebView.CoreWebView2;

                if (webView == null)
                {
                    MessageBox.Show(
                        "Print preview is not ready yet. Please try again.",
                        "Print Preview",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                webView.ShowPrintUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open print preview: {ex.Message}",
                    "Print Preview",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ── Canvas SizeChanged handlers ────────────────────────────────────────

        private void PatientVisitCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsPatientVisitsSelected == true)
                ChartRenderer.RenderLineChart(PatientVisitCanvas, _vm.PatientVisitTrend, "#0EA5E9", "#50C878");
        }

        private void RevenueTrendCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsTransactionReportsSelected == true)
                ChartRenderer.RenderAreaChart(RevenueTrendCanvas, _vm.RevenueTrend, "#50C878");
        }

        private void DailyTransactionsCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsTransactionReportsSelected == true)
                ChartRenderer.RenderBarChart(DailyTransactionsCanvas, _vm.DailyTransactionCounts, "#2F98D0");
        }

        private void InventoryBarCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsInventoryReportsSelected == true)
                ChartRenderer.RenderBarChart(InventoryBarCanvas, _vm.InventoryChartData, "#FF981D");
        }

        private void ActivityPieCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsUserActivityLogSelected == true)
                ChartRenderer.RenderPieChart(ActivityPieCanvas, _vm.ActivityByType);
        }

        private void ActivityModuleCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsUserActivityLogSelected == true)
                ChartRenderer.RenderBarChart(ActivityModuleCanvas, _vm.ActivityByModule, "#A855F7");
        }
    }
}
