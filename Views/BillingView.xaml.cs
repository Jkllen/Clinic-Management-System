using CruzNeryClinic.ViewModels;
using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CruzNeryClinic.Views
{
    public partial class BillingView : UserControl
    {
        public BillingView()
        {
            InitializeComponent();

            DataContextChanged += BillingView_DataContextChanged;
            Unloaded += BillingView_Unloaded;
        }

        private void BillingView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is BillingViewModel oldViewModel)
                oldViewModel.PropertyChanged -= BillingViewModel_PropertyChanged;

            if (e.NewValue is BillingViewModel newViewModel)
                newViewModel.PropertyChanged += BillingViewModel_PropertyChanged;
        }

        private void BillingView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BillingViewModel viewModel)
                viewModel.PropertyChanged -= BillingViewModel_PropertyChanged;
        }

        private async void BillingViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not BillingViewModel viewModel)
                return;

            if (e.PropertyName != nameof(BillingViewModel.ReceiptPrintPreviewUri))
                return;

            if (viewModel.ReceiptPrintPreviewUri == null)
                return;

            try
            {
                await ReceiptPrintWebView.EnsureCoreWebView2Async();
                ReceiptPrintWebView.Source = viewModel.ReceiptPrintPreviewUri;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load receipt preview: {ex.Message}",
                    "Receipt Preview",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void OpenReceiptPrintDialog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ReceiptPrintWebView.EnsureCoreWebView2Async();

                CoreWebView2? webView = ReceiptPrintWebView.CoreWebView2;

                if (webView == null)
                {
                    MessageBox.Show(
                        "Print preview is not ready yet. Please try again.",
                        "Print Preview",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

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
                    MessageBoxImage.Error
                );
            }
        }
    }
}