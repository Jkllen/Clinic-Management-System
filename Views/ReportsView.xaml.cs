using CruzNeryClinic.Models;
using CruzNeryClinic.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
                _vm.ChartDataRefreshed -= OnChartDataRefreshed;

            _vm = e.NewValue as ReportsViewModel;

            if (_vm != null)
                _vm.ChartDataRefreshed += OnChartDataRefreshed;
        }

        private void OnChartDataRefreshed()
            => Dispatcher.InvokeAsync(RenderActiveChart);

        private void RenderActiveChart()
        {
            if (_vm == null) return;

            if (_vm.IsPatientVisitsSelected)
                RenderLineChart(PatientVisitCanvas, _vm.PatientVisitTrend, "#0EA5E9", "#50C878");
            else if (_vm.IsTransactionReportsSelected)
            {
                RenderAreaChart(RevenueTrendCanvas, _vm.RevenueTrend, "#50C878");
                RenderBarChart(DailyTransactionsCanvas, _vm.DailyTransactionCounts, "#2F98D0");
            }
            else if (_vm.IsInventoryReportsSelected)
                RenderBarChart(InventoryBarCanvas, _vm.InventoryChartData, "#FF981D");
            else if (_vm.IsUserActivityLogSelected)
            {
                RenderPieChart(ActivityPieCanvas, _vm.ActivityByType);
                RenderBarChart(ActivityModuleCanvas, _vm.ActivityByModule, "#A855F7");
            }
        }

        // ── Canvas SizeChanged handlers ────────────────────────────────────────

        private void PatientVisitCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsPatientVisitsSelected == true)
                RenderLineChart(PatientVisitCanvas, _vm.PatientVisitTrend, "#0EA5E9", "#50C878");
        }

        private void RevenueTrendCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsTransactionReportsSelected == true)
                RenderAreaChart(RevenueTrendCanvas, _vm.RevenueTrend, "#50C878");
        }

        private void DailyTransactionsCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsTransactionReportsSelected == true)
                RenderBarChart(DailyTransactionsCanvas, _vm.DailyTransactionCounts, "#2F98D0");
        }

        private void InventoryBarCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsInventoryReportsSelected == true)
                RenderBarChart(InventoryBarCanvas, _vm.InventoryChartData, "#FF981D");
        }

        private void ActivityPieCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsUserActivityLogSelected == true)
                RenderPieChart(ActivityPieCanvas, _vm.ActivityByType);
        }

        private void ActivityModuleCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm?.IsUserActivityLogSelected == true)
                RenderBarChart(ActivityModuleCanvas, _vm.ActivityByModule, "#A855F7");
        }

        // ── Bar chart ──────────────────────────────────────────────────────────

        private static void RenderBarChart(Canvas canvas, List<ChartDataPoint> data, string hexColor)
        {
            canvas.Children.Clear();
            if (data == null || data.Count == 0)
            {
                DrawEmptyMessage(canvas, "No data available");
                return;
            }

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            const double padLeft = 44;
            const double padRight = 12;
            const double padTop = 12;
            const double padBottom = 36;

            double chartW = w - padLeft - padRight;
            double chartH = h - padTop - padBottom;

            double maxVal = data.Max(d => d.Value);
            if (maxVal == 0) maxVal = 1;

            var barBrush = BrushFromHex(hexColor);
            var gridBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
            var textBrush = new SolidColorBrush(Color.FromRgb(130, 130, 130));

            // Y-axis grid lines (4 lines)
            for (int i = 1; i <= 4; i++)
            {
                double y = padTop + chartH - (i / 4.0) * chartH;
                var line = new Line
                {
                    X1 = padLeft, Y1 = y,
                    X2 = padLeft + chartW, Y2 = y,
                    Stroke = gridBrush,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 4 },
                };
                canvas.Children.Add(line);

                double labelVal = maxVal * i / 4;
                var yLabel = MakeText(FormatValue(labelVal), 9, textBrush);
                Canvas.SetRight(yLabel, w - padLeft + 2);
                Canvas.SetTop(yLabel, y - 7);
                canvas.Children.Add(yLabel);
            }

            // X axis baseline
            var baseline = new Line
            {
                X1 = padLeft, Y1 = padTop + chartH,
                X2 = padLeft + chartW, Y2 = padTop + chartH,
                Stroke = gridBrush,
                StrokeThickness = 1,
            };
            canvas.Children.Add(baseline);

            double totalBarAreaW = chartW / data.Count;
            double barWidth = Math.Max(4, totalBarAreaW * 0.55);
            double barSpacing = totalBarAreaW;

            for (int i = 0; i < data.Count; i++)
            {
                double barH = data[i].Value / maxVal * chartH;
                double x = padLeft + i * barSpacing + (barSpacing - barWidth) / 2;
                double y = padTop + chartH - barH;

                var rect = new Rectangle
                {
                    Width = barWidth,
                    Height = Math.Max(1, barH),
                    Fill = barBrush,
                    RadiusX = 3,
                    RadiusY = 3,
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                canvas.Children.Add(rect);

                // X axis label
                var label = MakeText(data[i].Label, 9, textBrush);
                Canvas.SetLeft(label, x + barWidth / 2 - 12);
                Canvas.SetTop(label, padTop + chartH + 4);
                canvas.Children.Add(label);
            }
        }

        // ── Area / line chart ──────────────────────────────────────────────────

        private static void RenderAreaChart(Canvas canvas, List<ChartDataPoint> data, string hexColor)
        {
            canvas.Children.Clear();
            if (data == null || data.Count == 0)
            {
                DrawEmptyMessage(canvas, "No data available");
                return;
            }

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            const double padLeft = 54;
            const double padRight = 12;
            const double padTop = 12;
            const double padBottom = 36;

            double chartW = w - padLeft - padRight;
            double chartH = h - padTop - padBottom;

            double maxVal = data.Max(d => d.Value);
            if (maxVal == 0) maxVal = 1;

            var lineBrush = BrushFromHex(hexColor);
            var areaColor = BrushFromHex(hexColor, 45);
            var gridBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
            var textBrush = new SolidColorBrush(Color.FromRgb(130, 130, 130));

            // Y grid lines
            for (int i = 1; i <= 4; i++)
            {
                double y = padTop + chartH - (i / 4.0) * chartH;
                var line = new Line
                {
                    X1 = padLeft, Y1 = y,
                    X2 = padLeft + chartW, Y2 = y,
                    Stroke = gridBrush,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 4 },
                };
                canvas.Children.Add(line);

                double labelVal = maxVal * i / 4;
                var yLabel = MakeText(FormatValue(labelVal), 9, textBrush);
                Canvas.SetRight(yLabel, w - padLeft + 2);
                Canvas.SetTop(yLabel, y - 7);
                canvas.Children.Add(yLabel);
            }

            // Baseline
            var baseline = new Line
            {
                X1 = padLeft, Y1 = padTop + chartH,
                X2 = padLeft + chartW, Y2 = padTop + chartH,
                Stroke = gridBrush,
                StrokeThickness = 1,
            };
            canvas.Children.Add(baseline);

            // Compute points
            double step = data.Count > 1 ? chartW / (data.Count - 1) : chartW;
            var linePoints = new PointCollection();
            var areaPoints = new PointCollection();

            // Area starts at baseline left
            areaPoints.Add(new Point(padLeft, padTop + chartH));

            for (int i = 0; i < data.Count; i++)
            {
                double x = padLeft + i * step;
                double y = padTop + chartH - data[i].Value / maxVal * chartH;
                linePoints.Add(new Point(x, y));
                areaPoints.Add(new Point(x, y));

                // X label (every Nth to avoid overlap)
                if (i == 0 || i == data.Count - 1 || data.Count <= 8 || i % Math.Max(1, data.Count / 6) == 0)
                {
                    var lbl = MakeText(data[i].Label, 9, textBrush);
                    Canvas.SetLeft(lbl, x - 12);
                    Canvas.SetTop(lbl, padTop + chartH + 4);
                    canvas.Children.Add(lbl);
                }
            }

            // Area closes at baseline right
            areaPoints.Add(new Point(padLeft + (data.Count - 1) * step, padTop + chartH));

            // Filled area
            var area = new Polygon
            {
                Points = areaPoints,
                Fill = areaColor,
                Stroke = Brushes.Transparent,
            };
            canvas.Children.Add(area);

            // Line on top
            var polyline = new Polyline
            {
                Points = linePoints,
                Stroke = lineBrush,
                StrokeThickness = 2.2,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            };
            canvas.Children.Add(polyline);

            // Dots at data points
            foreach (var pt in linePoints)
            {
                var dot = new Ellipse { Width = 5, Height = 5, Fill = lineBrush };
                Canvas.SetLeft(dot, pt.X - 2.5);
                Canvas.SetTop(dot, pt.Y - 2.5);
                canvas.Children.Add(dot);
            }
        }

        // ── Dual-series line chart ─────────────────────────────────────────────

        private static void RenderLineChart(Canvas canvas, List<DualChartDataPoint> data, string color1, string color2)
        {
            canvas.Children.Clear();
            if (data == null || data.Count == 0)
            {
                DrawEmptyMessage(canvas, "No data available");
                return;
            }

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            const double padLeft = 44;
            const double padRight = 20;
            const double padTop = 12;
            const double padBottom = 48; // extra for legend

            double chartW = w - padLeft - padRight;
            double chartH = h - padTop - padBottom;

            double maxVal = data.Max(d => Math.Max(d.Value1, d.Value2));
            if (maxVal == 0) maxVal = 1;

            var brush1 = BrushFromHex(color1);
            var brush2 = BrushFromHex(color2);
            var gridBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
            var textBrush = new SolidColorBrush(Color.FromRgb(130, 130, 130));

            // Y grid lines
            for (int i = 1; i <= 4; i++)
            {
                double y = padTop + chartH - (i / 4.0) * chartH;
                canvas.Children.Add(new Line
                {
                    X1 = padLeft, Y1 = y,
                    X2 = padLeft + chartW, Y2 = y,
                    Stroke = gridBrush,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 4 },
                });
                double labelVal = maxVal * i / 4;
                var yLabel = MakeText(FormatValue(labelVal), 9, textBrush);
                Canvas.SetRight(yLabel, w - padLeft + 2);
                Canvas.SetTop(yLabel, y - 7);
                canvas.Children.Add(yLabel);
            }

            canvas.Children.Add(new Line
            {
                X1 = padLeft, Y1 = padTop + chartH,
                X2 = padLeft + chartW, Y2 = padTop + chartH,
                Stroke = gridBrush,
                StrokeThickness = 1,
            });

            double step = data.Count > 1 ? chartW / (data.Count - 1) : chartW;
            var pts1 = new PointCollection();
            var pts2 = new PointCollection();

            for (int i = 0; i < data.Count; i++)
            {
                double x = padLeft + i * step;
                pts1.Add(new Point(x, padTop + chartH - data[i].Value1 / maxVal * chartH));
                pts2.Add(new Point(x, padTop + chartH - data[i].Value2 / maxVal * chartH));

                // X label
                bool showLabel = i == 0 || i == data.Count - 1 || data.Count <= 8 || i % Math.Max(1, data.Count / 6) == 0;
                if (showLabel)
                {
                    var lbl = MakeText(data[i].Label, 9, textBrush);
                    Canvas.SetLeft(lbl, x - 12);
                    Canvas.SetTop(lbl, padTop + chartH + 4);
                    canvas.Children.Add(lbl);
                }
            }

            // Series 1 (Scheduled)
            canvas.Children.Add(new Polyline
            {
                Points = pts1,
                Stroke = brush1,
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round,
            });
            // Series 2 (Walk-in)
            canvas.Children.Add(new Polyline
            {
                Points = pts2,
                Stroke = brush2,
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round,
            });

            // Dots
            foreach (var pt in pts1)
            {
                var dot = new Ellipse { Width = 5, Height = 5, Fill = brush1 };
                Canvas.SetLeft(dot, pt.X - 2.5);
                Canvas.SetTop(dot, pt.Y - 2.5);
                canvas.Children.Add(dot);
            }
            foreach (var pt in pts2)
            {
                var dot = new Ellipse { Width = 5, Height = 5, Fill = brush2 };
                Canvas.SetLeft(dot, pt.X - 2.5);
                Canvas.SetTop(dot, pt.Y - 2.5);
                canvas.Children.Add(dot);
            }

            // Legend
            double legendY = padTop + chartH + 26;
            double legendX = padLeft;

            DrawLegendItem(canvas, legendX, legendY, color1, "Scheduled");
            DrawLegendItem(canvas, legendX + 110, legendY, color2, "Walk-in");
        }

        // ── Pie chart ──────────────────────────────────────────────────────────

        private static void RenderPieChart(Canvas canvas, List<PieChartSlice> slices)
        {
            canvas.Children.Clear();
            if (slices == null || slices.Count == 0)
            {
                DrawEmptyMessage(canvas, "No data available");
                return;
            }

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            double legendH = Math.Min(slices.Count * 18 + 10, h * 0.35);
            double pieH = h - legendH;

            double cx = w / 2;
            double cy = pieH / 2;
            double r = Math.Min(w / 2.4, pieH / 2.2);

            double total = slices.Sum(s => s.Value);
            if (total == 0) return;

            double currentAngle = -90.0;

            foreach (var slice in slices)
            {
                double sweep = slice.Value / total * 360.0;
                if (sweep < 0.5) { currentAngle += sweep; continue; }

                double startRad = DegToRad(currentAngle);
                double endRad = DegToRad(currentAngle + sweep);

                var startPt = new Point(cx + r * Math.Cos(startRad), cy + r * Math.Sin(startRad));
                var endPt = new Point(cx + r * Math.Cos(endRad), cy + r * Math.Sin(endRad));

                var fig = new PathFigure { StartPoint = new Point(cx, cy), IsClosed = true };
                fig.Segments.Add(new LineSegment(startPt, true));
                fig.Segments.Add(new ArcSegment(endPt, new Size(r, r), 0, sweep > 180, SweepDirection.Clockwise, true));

                var geo = new PathGeometry();
                geo.Figures.Add(fig);

                var path = new Path
                {
                    Data = geo,
                    Fill = BrushFromHex(slice.HexColor),
                    Stroke = Brushes.White,
                    StrokeThickness = 1.5,
                };
                canvas.Children.Add(path);

                // Percentage label inside slice
                if (sweep >= 18)
                {
                    double midRad = DegToRad(currentAngle + sweep / 2);
                    double lr = r * 0.64;
                    var pctLabel = MakeText($"{slice.Percentage}%", 10, Brushes.White, bold: true);
                    Canvas.SetLeft(pctLabel, cx + lr * Math.Cos(midRad) - 14);
                    Canvas.SetTop(pctLabel, cy + lr * Math.Sin(midRad) - 7);
                    canvas.Children.Add(pctLabel);
                }

                currentAngle += sweep;
            }

            // Legend below pie
            double lx = 10;
            double ly = pieH;
            for (int i = 0; i < slices.Count; i++)
            {
                var dot = new Ellipse { Width = 10, Height = 10, Fill = BrushFromHex(slices[i].HexColor) };
                Canvas.SetLeft(dot, lx);
                Canvas.SetTop(dot, ly + i * 18 + 4);
                canvas.Children.Add(dot);

                string labelText = $"{slices[i].Label} {slices[i].Percentage}%";
                var txt = MakeText(labelText, 10, new SolidColorBrush(Color.FromRgb(80, 80, 80)));
                Canvas.SetLeft(txt, lx + 14);
                Canvas.SetTop(txt, ly + i * 18 + 2);
                canvas.Children.Add(txt);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void DrawLegendItem(Canvas canvas, double x, double y, string hexColor, string label)
        {
            var line = new Line
            {
                X1 = x, Y1 = y + 6,
                X2 = x + 20, Y2 = y + 6,
                Stroke = BrushFromHex(hexColor),
                StrokeThickness = 2.5,
            };
            canvas.Children.Add(line);

            var txt = MakeText(label, 10, new SolidColorBrush(Color.FromRgb(80, 80, 80)));
            Canvas.SetLeft(txt, x + 24);
            Canvas.SetTop(txt, y);
            canvas.Children.Add(txt);
        }

        private static void DrawEmptyMessage(Canvas canvas, string message)
        {
            var tb = MakeText(message, 13, new SolidColorBrush(Color.FromRgb(160, 160, 160)));
            Canvas.SetLeft(tb, 0);
            Canvas.SetTop(tb, 0);
            tb.Width = double.NaN;
            canvas.Children.Add(tb);
            Canvas.SetLeft(tb, (canvas.ActualWidth - 120) / 2);
            Canvas.SetTop(tb, (canvas.ActualHeight - 20) / 2);
        }

        private static TextBlock MakeText(string text, double fontSize, Brush foreground, bool bold = false)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                Foreground = foreground,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            };
        }

        private static SolidColorBrush BrushFromHex(string hex, byte alpha = 255)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                color.A = alpha;
                return new SolidColorBrush(color);
            }
            catch
            {
                return Brushes.Gray;
            }
        }

        private static double DegToRad(double degrees)
            => degrees * Math.PI / 180.0;

        private static string FormatValue(double val)
        {
            if (val >= 1_000_000) return $"{val / 1_000_000:F1}M";
            if (val >= 1_000) return $"{val / 1_000:F1}K";
            return val % 1 == 0 ? ((int)val).ToString() : $"{val:F1}";
        }
    }
}
