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
                        header.BorderBottom(1).BorderColor("#222222").PaddingBottom(10).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(left =>
                                {
                                    left.Item().Text("CRUZ-NERY DENTAL CLINIC")
                                        .FontSize(18)
                                        .Bold()
                                        .FontColor("#111111");

                                    left.Item().PaddingTop(2).Text("Registered Name: Cruz-Nery Dental Clinic")
                                        .FontSize(9)
                                        .FontColor("#333333");

                                    left.Item().Text("Address: Rodriguez, Rizal")
                                        .FontSize(9)
                                        .FontColor("#333333");

                                    left.Item().Text("TIN: Not configured")
                                        .FontSize(9)
                                        .FontColor("#333333");

                                    left.Item().Text("Registration Type: Non-VAT / VAT status not configured")
                                        .FontSize(9)
                                        .FontColor("#333333");
                                });

                                row.ConstantItem(190).AlignRight().Column(right =>
                                {
                                    right.Item().AlignRight().Text("INVOICE")
                                        .FontSize(24)
                                        .Bold()
                                        .FontColor("#111111");

                                    right.Item().PaddingTop(4).Text($"Invoice No.: {receipt.ReceiptNumber}")
                                        .FontSize(10)
                                        .Bold()
                                        .FontColor("#111111");

                                    right.Item().Text($"Date: {receipt.TransactionDateDisplay}")
                                        .FontSize(10)
                                        .FontColor("#333333");

                                    right.Item().PaddingTop(4).Text("☑ CASH SALES")
                                        .FontSize(9)
                                        .FontColor("#333333");

                                    right.Item().Text("☐ CHARGE SALES")
                                        .FontSize(9)
                                        .FontColor("#333333");
                                });
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
                                    "SOLD TO",
                                    $"Customer / Patient Name: {receipt.PatientName}\n" +
                                    $"Patient ID: {receipt.PatientCode}\n" +
                                    $"TIN: N/A\n" +
                                    $"Business Address: N/A"
                                );
                            });

                            row.ConstantItem(24);

                            row.RelativeItem().Element(box =>
                            {
                                InfoBox(
                                    box,
                                    "INVOICE DETAILS",
                                    $"Category: {receipt.PatientCategory}\n" +
                                    $"Payment Status: {receipt.PaymentStatus}\n" +
                                    $"Billing Source: {receipt.BillingSource}"
                                );
                            });
                        });
                        col.Item().Text("Invoice Items")
                            .FontSize(15)
                            .Bold()
                            .FontColor("#333333");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.6f); // Description / Nature of Service
                                columns.RelativeColumn(0.7f); // Quantity
                                columns.RelativeColumn(1.1f); // Unit Cost
                                columns.RelativeColumn(1.1f); // Amount
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(TableHeaderCell).Text("Description / Nature of Service");
                                header.Cell().Element(TableHeaderCell).AlignCenter().Text("Qty");
                                header.Cell().Element(TableHeaderCell).AlignRight().Text("Unit Cost");
                                header.Cell().Element(TableHeaderCell).AlignRight().Text("Amount");
                            });

                            if (receipt.InvoiceItems != null && receipt.InvoiceItems.Count > 0)
                            {
                                foreach (BillingTransactionItem item in receipt.InvoiceItems)
                                {
                                    string description = string.IsNullOrWhiteSpace(item.ItemDescription)
                                        ? item.ServiceName
                                        : $"{item.ServiceName} - {item.ItemDescription}";

                                    if (item.TreatmentDate.HasValue)
                                        description = $"{description}\nDate: {item.TreatmentDateDisplay}";

                                    string unitCost = item.IsIncluded
                                        ? "Included"
                                        : $"₱{item.Amount:N2}";

                                    string amount = item.IsIncluded
                                        ? "₱0.00"
                                        : $"₱{item.Amount:N2}";

                                    table.Cell().Element(TableBodyCell).Text(description);
                                    table.Cell().Element(TableBodyCell).AlignCenter().Text("1");
                                    table.Cell().Element(TableBodyCell).AlignRight().Text(unitCost);
                                    table.Cell().Element(TableBodyCell).AlignRight().Text(amount);
                                }
                            }
                            else
                            {
                                // Fallback for older billing records that do not have BillingTransactionItems yet.
                                table.Cell().Element(TableBodyCell).Text(
                                    string.IsNullOrWhiteSpace(receipt.Description)
                                        ? receipt.ServiceName
                                        : receipt.Description
                                );

                                table.Cell().Element(TableBodyCell).AlignCenter().Text("1");
                                table.Cell().Element(TableBodyCell).AlignRight().Text(receipt.TotalAmountDisplay);
                                table.Cell().Element(TableBodyCell).AlignRight().Text(receipt.TotalAmountDisplay);
                            }
                        });

                        col.Item().PaddingTop(4).Text("Payment History")
                            .FontSize(15)
                            .Bold()
                            .FontColor("#333333");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.2f); // Payment Date
                                columns.RelativeColumn(1.0f); // Method
                                columns.RelativeColumn(2.0f); // Notes
                                columns.RelativeColumn(1.1f); // Amount
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(TableHeaderCell).Text("Payment Date");
                                header.Cell().Element(TableHeaderCell).Text("Method");
                                header.Cell().Element(TableHeaderCell).Text("Notes");
                                header.Cell().Element(TableHeaderCell).AlignRight().Text("Amount");
                            });

                            if (receipt.PaymentHistory != null && receipt.PaymentHistory.Count > 0)
                            {
                                foreach (PaymentRecord payment in receipt.PaymentHistory)
                                {
                                    table.Cell().Element(TableBodyCell).Text(payment.PaymentDate.ToString("MM/dd/yyyy"));

                                    table.Cell().Element(TableBodyCell).Text(
                                        string.IsNullOrWhiteSpace(payment.PaymentMethod)
                                            ? "Cash"
                                            : payment.PaymentMethod
                                    );

                                    table.Cell().Element(TableBodyCell).Text(
                                        string.IsNullOrWhiteSpace(payment.Notes)
                                            ? "-"
                                            : payment.Notes
                                    );

                                    table.Cell().Element(TableBodyCell).AlignRight().Text($"₱{payment.AmountPaid:N2}");
                                }
                            }
                            else
                            {
                                table.Cell().Element(TableBodyCell).Text("-");
                                table.Cell().Element(TableBodyCell).Text("-");
                                table.Cell().Element(TableBodyCell).Text("No payment recorded yet.");
                                table.Cell().Element(TableBodyCell).AlignRight().Text("₱0.00");
                            }
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

                    page.Footer().Column(footer =>
                    {
                        footer.Item().PaddingTop(8).BorderTop(1).BorderColor("#CCCCCC").PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text("PERMIT / AUTHORITY DETAILS")
                                    .FontSize(7)
                                    .Bold()
                                    .FontColor("#333333");

                                left.Item().Text("Permit to Use / ATP No.: Not configured")
                                    .FontSize(7)
                                    .FontColor("#555555");

                                left.Item().Text("BIR Permit No.: Not configured")
                                    .FontSize(7)
                                    .FontColor("#555555");

                                left.Item().Text("Approved Series: Not configured")
                                    .FontSize(7)
                                    .FontColor("#555555");
                            });

                            row.RelativeItem().AlignRight().Column(right =>
                            {
                                right.Item().Text("“THIS DOCUMENT IS NOT VALID FOR CLAIM OF INPUT TAX.”")
                                    .FontSize(7)
                                    .Bold()
                                    .FontColor("#333333");

                                right.Item().PaddingTop(4).Text("This invoice was generated by Dental Clinic Management System.")
                                    .FontSize(7)
                                    .FontColor("#777777");
                            });
                        });
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
