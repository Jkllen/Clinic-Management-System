using CruzNeryClinic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CruzNeryClinic.Views.Charts
{
    // Shared canvas-based chart renderer used by both the Reports screen and the
    // Dashboard. All charts draw directly onto a WPF Canvas so no external charting
    // package is required. Curves are smoothed with Catmull-Rom → Bézier conversion.
    public static class ChartRenderer
    {
        // ── Bar chart ──────────────────────────────────────────────────────────

        public static void RenderBarChart(Canvas canvas, List<ChartDataPoint> data, string hexColor)
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
                    ToolTip = MakeToolTip($"{data[i].Label}: {FormatValue(data[i].Value)}"),
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

        public static void RenderAreaChart(Canvas canvas, List<ChartDataPoint> data, string hexColor)
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
            var areaColor = MakeVerticalFade(hexColor, 120, 8);
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
            double baselineY = padTop + chartH;
            var points = new List<Point>();

            for (int i = 0; i < data.Count; i++)
            {
                double x = padLeft + i * step;
                double y = baselineY - data[i].Value / maxVal * chartH;
                points.Add(new Point(x, y));

                // X label (every Nth to avoid overlap)
                if (i == 0 || i == data.Count - 1 || data.Count <= 8 || i % Math.Max(1, data.Count / 6) == 0)
                {
                    var lbl = MakeText(data[i].Label, 9, textBrush);
                    Canvas.SetLeft(lbl, x - 12);
                    Canvas.SetTop(lbl, padTop + chartH + 4);
                    canvas.Children.Add(lbl);
                }
            }

            // Filled area under a smooth curve (gradient fade to baseline)
            var areaFig = new PathFigure { StartPoint = new Point(points[0].X, baselineY), IsClosed = true, IsFilled = true };
            areaFig.Segments.Add(new LineSegment(points[0], false));
            foreach (var seg in SmoothSegments(points))
                areaFig.Segments.Add(seg);
            areaFig.Segments.Add(new LineSegment(new Point(points[points.Count - 1].X, baselineY), false));
            var areaGeo = new PathGeometry();
            areaGeo.Figures.Add(areaFig);
            canvas.Children.Add(new Path { Data = areaGeo, Fill = areaColor });

            // Smooth line on top
            var lineFig = new PathFigure { StartPoint = points[0], IsClosed = false, IsFilled = false };
            foreach (var seg in SmoothSegments(points))
                lineFig.Segments.Add(seg);
            var lineGeo = new PathGeometry();
            lineGeo.Figures.Add(lineFig);
            canvas.Children.Add(new Path
            {
                Data = lineGeo,
                Stroke = lineBrush,
                StrokeThickness = 2.4,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            });

            // Transparent hit-targets so hovering near a point reveals its value.
            for (int i = 0; i < points.Count; i++)
            {
                var hit = new Ellipse
                {
                    Width = 14,
                    Height = 14,
                    Fill = Brushes.Transparent,
                    ToolTip = MakeToolTip($"{data[i].Label}: {FormatValue(data[i].Value)}"),
                };
                Canvas.SetLeft(hit, points[i].X - 7);
                Canvas.SetTop(hit, points[i].Y - 7);
                canvas.Children.Add(hit);
            }
        }

        // ── Dual-series line chart ─────────────────────────────────────────────

        public static void RenderLineChart(Canvas canvas, List<DualChartDataPoint> data, string color1, string color2,
            string legend1 = "Scheduled", string legend2 = "Walk-in")
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
            var pts1 = new List<Point>();
            var pts2 = new List<Point>();

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

            // Smooth series lines
            canvas.Children.Add(SmoothLinePath(pts1, brush1, 2.4));
            canvas.Children.Add(SmoothLinePath(pts2, brush2, 2.4));

            // Markers (filled circle with white ring) with value tooltips
            for (int i = 0; i < pts1.Count; i++)
                AddMarker(canvas, pts1[i], brush1, $"{data[i].Label} — {legend1}: {FormatValue(data[i].Value1)}");
            for (int i = 0; i < pts2.Count; i++)
                AddMarker(canvas, pts2[i], brush2, $"{data[i].Label} — {legend2}: {FormatValue(data[i].Value2)}");

            // Legend
            double legendY = padTop + chartH + 26;
            double legendX = padLeft;

            DrawLegendItem(canvas, legendX, legendY, color1, legend1);
            DrawLegendItem(canvas, legendX + 110, legendY, color2, legend2);
        }

        // ── Pie chart ──────────────────────────────────────────────────────────

        public static void RenderPieChart(Canvas canvas, List<PieChartSlice> slices)
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
                    ToolTip = MakeToolTip($"{slice.Label}: {FormatValue(slice.Value)} ({slice.Percentage}%)"),
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

        /// <summary>Catmull-Rom → Bézier smoothing: builds curved segments through the given points.</summary>
        private static List<PathSegment> SmoothSegments(IReadOnlyList<Point> pts)
        {
            var segments = new List<PathSegment>();
            if (pts.Count < 3)
            {
                for (int i = 1; i < pts.Count; i++)
                    segments.Add(new LineSegment(pts[i], true));
                return segments;
            }

            for (int i = 0; i < pts.Count - 1; i++)
            {
                Point p0 = pts[Math.Max(i - 1, 0)];
                Point p1 = pts[i];
                Point p2 = pts[i + 1];
                Point p3 = pts[Math.Min(i + 2, pts.Count - 1)];

                var c1 = new Point(p1.X + (p2.X - p0.X) / 6.0, p1.Y + (p2.Y - p0.Y) / 6.0);
                var c2 = new Point(p2.X - (p3.X - p1.X) / 6.0, p2.Y - (p3.Y - p1.Y) / 6.0);
                segments.Add(new BezierSegment(c1, c2, p2, true));
            }
            return segments;
        }

        /// <summary>A stroked smooth-curve Path through the given points.</summary>
        private static Path SmoothLinePath(IReadOnlyList<Point> pts, Brush stroke, double thickness)
        {
            var fig = new PathFigure { StartPoint = pts[0], IsClosed = false, IsFilled = false };
            foreach (var seg in SmoothSegments(pts))
                fig.Segments.Add(seg);
            var geo = new PathGeometry();
            geo.Figures.Add(fig);
            return new Path
            {
                Data = geo,
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            };
        }

        /// <summary>Draws a filled data-point marker with a white ring.</summary>
        private static void AddMarker(Canvas canvas, Point pt, Brush fill, string? tooltip = null)
        {
            var dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = fill,
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
            };
            if (tooltip != null) dot.ToolTip = MakeToolTip(tooltip);
            Canvas.SetLeft(dot, pt.X - 4);
            Canvas.SetTop(dot, pt.Y - 4);
            canvas.Children.Add(dot);
        }

        /// <summary>Top-to-bottom gradient of a colour, fading from <paramref name="topAlpha"/> to <paramref name="bottomAlpha"/>.</summary>
        private static LinearGradientBrush MakeVerticalFade(string hex, byte topAlpha, byte bottomAlpha)
        {
            Color baseColor;
            try { baseColor = (Color)ColorConverter.ConvertFromString(hex); }
            catch { baseColor = Colors.Gray; }

            var top = Color.FromArgb(topAlpha, baseColor.R, baseColor.G, baseColor.B);
            var bottom = Color.FromArgb(bottomAlpha, baseColor.R, baseColor.G, baseColor.B);

            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(top, 0),
                    new GradientStop(bottom, 1),
                },
            };
        }

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

        /// <summary>Builds a snappy tooltip shown on chart shapes.</summary>
        private static ToolTip MakeToolTip(string text)
        {
            var tip = new ToolTip
            {
                Content = text,
                FontSize = 12,
            };
            ToolTipService.SetInitialShowDelay(tip, 150);
            return tip;
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
