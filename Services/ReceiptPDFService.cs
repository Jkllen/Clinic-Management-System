using CruzNeryClinic.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;

namespace CruzNeryClinic.Services
{
    public static class ReceiptPDFService
    {
        public static string GenerateReceiptPdf(BillingReceiptDetail receipt)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string receiptsFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Receipts"
            );

            Directory.CreateDirectory(receiptsFolder);

            string safeReceiptNumber = MakeSafeFileName(receipt.ReceiptNumber);
            string filePath = Path.Combine(receiptsFolder, $"{safeReceiptNumber}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(header =>
                    {
                        header.Background("#223357").Padding(18).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("CRUZ-NERY DENTAL CLINIC")
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.White);

                                col.Item().PaddingTop(4).Text("Dental Receipt")
                                    .FontSize(12)
                                    .FontColor(Colors.White);
                            });

                            row.ConstantItem(180).AlignRight().Column(col =>
                            {
                                col.Item().Text("Receipt Number")
                                    .FontSize(9)
                                    .FontColor(Colors.White);

                                col.Item().Text(receipt.ReceiptNumber)
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Colors.White);

                                col.Item().PaddingTop(8).Text("Transaction Date")
                                    .FontSize(9)
                                    .FontColor(Colors.White);

                                col.Item().Text(receipt.TransactionDateDisplay)
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(Colors.White);
                            });
                        });
                    });

                    page.Content().PaddingTop(24).Column(col =>
                    {
                        col.Spacing(18);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(box =>
                            {
                                InfoBox(
                                    box,
                                    "Patient Information",
                                    $"Patient ID: {receipt.PatientCode}\n" +
                                    $"Patient Name: {receipt.PatientName}\n" +
                                    $"Category: {receipt.PatientCategory}"
                                );
                            });

                            row.ConstantItem(24);

                            row.RelativeItem().Element(box =>
                            {
                                InfoBox(
                                    box,
                                    "Transaction Information",
                                    $"Payment Method: {receipt.PaymentMethod}\n" +
                                    $"Payment Status: {receipt.PaymentStatus}\n" +
                                    $"Billing Source: {receipt.BillingSource}"
                                );
                            });
                        });

                        col.Item().Text("Service / Treatment")
                            .FontSize(15)
                            .Bold()
                            .FontColor("#333333");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(TableHeaderCell).Text("Description");
                                header.Cell().Element(TableHeaderCell).AlignRight().Text("Amount");
                            });

                            table.Cell().Element(TableBodyCell).Text(
                                string.IsNullOrWhiteSpace(receipt.Description)
                                    ? receipt.ServiceName
                                    : receipt.Description
                            );

                            table.Cell().Element(TableBodyCell).AlignRight().Text(receipt.TotalAmountDisplay);
                        });

                        col.Item().AlignRight().Width(260).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            SummaryRow(table, "Gross Amount", receipt.TotalAmountDisplay);
                            
                            if (receipt.HasVatExemption)
                            {
                                SummaryRow(table, "VAT Exempt Sales", receipt.VatExemptSalesDisplay);
                            }

                            SummaryRow(table, $"Discount ({receipt.DiscountType})", receipt.DiscountAmountDisplay);
                            SummaryRow(table, "Billable Amount", receipt.SubtotalAfterDiscountDisplay);
                            SummaryRow(table, "Amount Paid", receipt.AmountPaidDisplay);
                            SummaryRow(table, "Remaining Balance", receipt.RemainingBalanceDisplay, true);
                        });

                        if (!string.IsNullOrWhiteSpace(receipt.Notes))
                        {
                            col.Item().PaddingTop(8).Column(notes =>
                            {
                                notes.Item().Text("Notes").Bold();
                                notes.Item().Text(receipt.Notes).FontSize(10).FontColor("#444444");
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("This receipt was generated by Cruz-Nery Dental Clinic Management System.")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf(filePath);

            return filePath;
        }

        public static void OpenPdf(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }

        private static void InfoBox(IContainer container, string title, string body)
        {
            container.Column(col =>
            {
                col.Item().Background("#F3F7FA").Padding(8).Text(title).Bold().FontSize(12);
                col.Item().Border(1).BorderColor("#DDDDDD").Padding(10).Text(body).FontSize(10);
            });
        }

        private static IContainer TableHeaderCell(IContainer container)
        {
            return container
                .Background("#EEF3FA")
                .Border(1)
                .BorderColor("#DDDDDD")
                .Padding(6);
        }

        private static IContainer TableBodyCell(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor("#E0E0E0")
                .Padding(6);
        }

        private static void SummaryRow(TableDescriptor table, string label, string value, bool isBold = false)
        {
            table.Cell().Element(c => SummaryCell(c, isBold)).Text(label);
            table.Cell().Element(c => SummaryCell(c, isBold)).AlignRight().Text(value);
        }

        private static IContainer SummaryCell(IContainer container, bool isBold)
        {
            IContainer styled = container
                .BorderBottom(1)
                .BorderColor("#DDDDDD")
                .PaddingVertical(6);

            return styled.DefaultTextStyle(x =>
                isBold ? x.Bold().FontSize(11) : x.FontSize(10)
            );
        }

        private static string MakeSafeFileName(string fileName)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidChar, '-');

            return fileName;
        }
    }
}