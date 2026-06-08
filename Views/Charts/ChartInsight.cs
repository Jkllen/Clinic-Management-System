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

        private static string Fmt(double value, string prefix = "")
            => prefix == "₱" ? $"₱{value:N2}" : $"{prefix}{value:N0}";
    }
}
