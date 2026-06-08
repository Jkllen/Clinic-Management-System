using CruzNeryClinic.Models;
using System.Collections.Generic;
using System.Linq;

namespace CruzNeryClinic.Views.Charts
{
    // Builds short, plain-language interpretation sentences for the dashboard and reports
    // charts, derived from the same data the charts are drawn from. Shown as a line of text
    // beneath each chart.
    public static class ChartInsight
    {
        private const string EmptyMessage = "No data for the selected period.";

        // Single-series charts (revenue, daily transactions, stock levels, activity by module).
        public static string Summarize(IReadOnlyList<ChartDataPoint> data, string noun, string prefix = "")
        {
            if (data == null || data.Count == 0) return EmptyMessage;

            double total = data.Sum(d => d.Value);
            var peak = data.Aggregate((a, b) => b.Value > a.Value ? b : a);
            var low = data.Aggregate((a, b) => b.Value < a.Value ? b : a);

            return $"Across {data.Count} {noun}, the total is {Fmt(total, prefix)}, " +
                   $"with the highest at {peak.Label} ({Fmt(peak.Value, prefix)}) " +
                   $"and the lowest at {low.Label} ({Fmt(low.Value, prefix)}).";
        }

        // Dual-series chart (patient visits: scheduled vs walk-in).
        public static string SummarizeDual(IReadOnlyList<DualChartDataPoint> data, string l1, string l2)
        {
            if (data == null || data.Count == 0) return EmptyMessage;

            double t1 = data.Sum(d => d.Value1);
            double t2 = data.Sum(d => d.Value2);
            var busiest = data.Aggregate((a, b) => (b.Value1 + b.Value2) > (a.Value1 + a.Value2) ? b : a);
            double busiestTotal = busiest.Value1 + busiest.Value2;

            return $"There were {Fmt(t1 + t2)} visits in total, made up of {Fmt(t1)} {l1} " +
                   $"and {Fmt(t2)} {l2}, and the busiest was {busiest.Label} with {Fmt(busiestTotal)}.";
        }

        // Pie chart (activity by type).
        public static string SummarizePie(IReadOnlyList<PieChartSlice> data)
        {
            if (data == null || data.Count == 0) return EmptyMessage;

            double total = data.Sum(d => d.Value);
            var top = data.Aggregate((a, b) => b.Value > a.Value ? b : a);
            string category = data.Count == 1 ? "category" : "categories";

            return $"{top.Label} accounts for the largest share at {top.Percentage}% " +
                   $"({Fmt(top.Value)} of {Fmt(total)}), out of {data.Count} {category}.";
        }

        // ── Chart detail panel helpers ──────────────────────────────────────────

        // One-line "what this shows" description per chart key.
        public static string Description(string key) => key switch
        {
            "patientVisit" => "Daily scheduled vs. walk-in patient visits over the selected period.",
            "revenue" => "Total revenue collected per day over the selected period.",
            "dailyTx" => "Number of billed transactions per day over the selected period.",
            "inventory" => "Current stock levels of inventory items.",
            "activityType" => "Breakdown of user activity by action type.",
            "activityModule" => "User activity volume per system module.",
            _ => string.Empty,
        };

        // Key figures for single-series charts (revenue, daily transactions, stock, modules).
        public static List<KeyFigure> KeyFigures(IReadOnlyList<ChartDataPoint> data, string prefix = "")
        {
            if (data == null || data.Count == 0) return new();

            double total = data.Sum(d => d.Value);
            var peak = data.Aggregate((a, b) => b.Value > a.Value ? b : a);
            var low = data.Aggregate((a, b) => b.Value < a.Value ? b : a);

            return new List<KeyFigure>
            {
                new("Total", Fmt(total, prefix)),
                new("Highest", $"{peak.Label} ({Fmt(peak.Value, prefix)})"),
                new("Lowest", $"{low.Label} ({Fmt(low.Value, prefix)})"),
            };
        }

        // Key figures for the dual-series patient-visits chart.
        public static List<KeyFigure> KeyFigures(IReadOnlyList<DualChartDataPoint> data, string l1, string l2)
        {
            if (data == null || data.Count == 0) return new();

            double t1 = data.Sum(d => d.Value1);
            double t2 = data.Sum(d => d.Value2);
            var busiest = data.Aggregate((a, b) => (b.Value1 + b.Value2) > (a.Value1 + a.Value2) ? b : a);

            return new List<KeyFigure>
            {
                new("Total visits", Fmt(t1 + t2)),
                new(Capitalize(l1), Fmt(t1)),
                new(Capitalize(l2), Fmt(t2)),
                new("Busiest", $"{busiest.Label} ({Fmt(busiest.Value1 + busiest.Value2)})"),
            };
        }

        // Key figures for the pie (activity by type) chart.
        public static List<KeyFigure> KeyFigures(IReadOnlyList<PieChartSlice> data)
        {
            if (data == null || data.Count == 0) return new();

            double total = data.Sum(d => d.Value);
            var top = data.Aggregate((a, b) => b.Value > a.Value ? b : a);

            return new List<KeyFigure>
            {
                new("Total", Fmt(total)),
                new("Leading", $"{top.Label} ({top.Percentage}%)"),
                new("Categories", data.Count.ToString("N0")),
            };
        }

        private static string Capitalize(string s)
            => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);

        private static string Fmt(double value, string prefix = "")
            => prefix == "₱" ? $"₱{value:N2}" : $"{prefix}{value:N0}";
    }

    // A single label/value figure shown in the chart detail panel.
    public record KeyFigure(string Label, string Value);
}
