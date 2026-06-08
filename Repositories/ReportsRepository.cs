using CruzNeryClinic.Data;
using CruzNeryClinic.Models;
using CruzNeryClinic.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CruzNeryClinic.Repositories
{
    public class ReportsRepository
    {
        // ── Patient Visits ─────────────────────────────────────────────────────

        public List<PatientVisitReportItem> GetPatientVisits(string fromDate, string toDate)
        {
            var items = new List<PatientVisitReportItem>();
            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    a.AppointmentDate,
                    p.PatientCode,
                    p.FirstName || ' ' || p.LastName AS PatientName,
                    a.AppointmentType,
                    a.ServiceName,
                    a.DentistName
                FROM Appointments a
                JOIN Patients p ON a.PatientId = p.PatientId
                WHERE a.AppointmentDate BETWEEN @From AND @To
                ORDER BY a.AppointmentDate DESC, a.AppointmentTime DESC";
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new PatientVisitReportItem
                {
                    Date = SafeString(reader, 0),
                    PatientCode = SafeString(reader, 1),
                    PatientName = SafeString(reader, 2),
                    VisitType = SafeString(reader, 3),
                    Service = SafeString(reader, 4),
                    Dentist = SafeString(reader, 5),
                });
            }
            return items;
        }

        public List<DualChartDataPoint> GetPatientVisitTrend(string fromDate, string toDate)
        {
            var dict = new SortedDictionary<string, (double scheduled, double walkIn)>();
            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    a.AppointmentDate,
                    a.AppointmentType,
                    COUNT(*) AS VisitCount
                FROM Appointments a
                WHERE a.AppointmentDate BETWEEN @From AND @To
                GROUP BY a.AppointmentDate, a.AppointmentType
                ORDER BY a.AppointmentDate";
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string date = SafeString(reader, 0);
                string type = SafeString(reader, 1);
                double count = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
                if (!dict.ContainsKey(date)) dict[date] = (0, 0);
                var existing = dict[date];
                dict[date] = type == "Scheduled"
                    ? (count, existing.walkIn)
                    : (existing.scheduled, count);
            }

            var result = new List<DualChartDataPoint>();
            foreach (var kv in dict)
            {
                string label = kv.Key.Length >= 10 ? kv.Key.Substring(5) : kv.Key;
                result.Add(new DualChartDataPoint
                {
                    Label = label,
                    Value1 = kv.Value.scheduled,
                    Value2 = kv.Value.walkIn,
                });
            }
            return result;
        }

        // ── Transaction Reports ────────────────────────────────────────────────

        public List<TransactionReportItem> GetTransactions(string fromDate, string toDate)
        {
            var items = new List<TransactionReportItem>();
            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    bt.TransactionDate,
                    bt.ReceiptNumber,
                    p.PatientCode,
                    p.FirstName || ' ' || p.LastName AS PatientName,
                    bt.ServiceName,
                    bt.AmountPaid,
                    COALESCE(
                        (SELECT pr.PaymentMethod
                         FROM PaymentRecords pr
                         WHERE pr.BillingId = bt.BillingId
                         ORDER BY pr.PaymentDate DESC LIMIT 1),
                        'N/A'
                    ) AS PaymentMethod
                FROM BillingTransactions bt
                JOIN Patients p ON bt.PatientId = p.PatientId
                WHERE bt.TransactionDate BETWEEN @From AND @To
                ORDER BY bt.TransactionDate DESC, bt.BillingId DESC";
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new TransactionReportItem
                {
                    Date = SafeString(reader, 0),
                    ReceiptNumber = SafeString(reader, 1),
                    PatientCode = SafeString(reader, 2),
                    PatientName = SafeString(reader, 3),
                    // ServiceName, AmountPaid and PaymentMethod are stored encrypted.
                    Service = CryptoService.DecryptString(SafeString(reader, 4)),
                    Amount = (double)SecureDecimal(reader, 5),
                    PaymentMethod = CryptoService.DecryptString(SafeString(reader, 6)),
                });
            }
            return items;
        }

        public List<ChartDataPoint> GetRevenueTrend(string fromDate, string toDate)
        {
            // Revenue is summed from BillingTransactions so the chart matches the
            // Transaction report's list and Total Revenue. AmountPaid is stored
            // encrypted, so it must be decrypted and aggregated in C# (SQL SUM()
            // can't operate on the encrypted text).
            var totals = new SortedDictionary<string, double>(StringComparer.Ordinal);

            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT TransactionDate, AmountPaid
                FROM BillingTransactions
                WHERE TransactionDate BETWEEN @From AND @To
                ORDER BY TransactionDate";
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string date = SafeString(reader, 0);
                if (string.IsNullOrEmpty(date)) continue;

                double amount = (double)SecureDecimal(reader, 1);
                totals[date] = totals.TryGetValue(date, out double existing) ? existing + amount : amount;
            }

            var items = new List<ChartDataPoint>();
            foreach (var kvp in totals)
            {
                string label = kvp.Key.Length >= 10 ? kvp.Key.Substring(5) : kvp.Key;
                items.Add(new ChartDataPoint { Label = label, Value = kvp.Value });
            }
            return items;
        }

        public List<ChartDataPoint> GetDailyTransactionCounts(string fromDate, string toDate)
        {
            var items = new List<ChartDataPoint>();
            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    TransactionDate,
                    COUNT(*) AS TxCount
                FROM BillingTransactions
                WHERE TransactionDate BETWEEN @From AND @To
                GROUP BY TransactionDate
                ORDER BY TransactionDate";
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string date = SafeString(reader, 0);
                string label = date.Length >= 10 ? date.Substring(5) : date;
                items.Add(new ChartDataPoint
                {
                    Label = label,
                    Value = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                });
            }
            return items;
        }

        // ── Inventory Reports ──────────────────────────────────────────────────

        public List<InventoryReportItem> GetInventoryItems()
        {
            var items = new List<InventoryReportItem>();
            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    ItemName,
                    Quantity,
                    MinimumThreshold,
                    COALESCE(LastRestock, '—'),
                    [Stock Status]
                FROM InventoryItems
                WHERE IsActive = 1
                ORDER BY ItemName";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string status = SafeString(reader, 4);
                string statusColor = status switch
                {
                    "Low Stock" => "#F0A500",
                    "Out of Stock" => "#E31D1D",
                    _ => "#50C878",
                };
                items.Add(new InventoryReportItem
                {
                    ItemName = SafeString(reader, 0),
                    CurrentStock = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    Threshold = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    LastRestocked = SafeString(reader, 3),
                    Status = status,
                    StatusColor = statusColor,
                });
            }
            return items;
        }

        public List<ChartDataPoint> GetInventoryChartData()
        {
            var items = new List<ChartDataPoint>();
            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT ItemName, Quantity
                FROM InventoryItems
                WHERE IsActive = 1
                ORDER BY ItemName
                LIMIT 10";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string name = SafeString(reader, 0);
                if (name.Length > 14) name = name.Substring(0, 12) + "..";
                items.Add(new ChartDataPoint
                {
                    Label = name,
                    Value = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                });
            }
            return items;
        }

        // ── User Activity Log ──────────────────────────────────────────────────

        public List<ActivityLogReportItem> GetActivityLogs(string fromDate, string toDate)
        {
            var items = new List<ActivityLogReportItem>();
            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    al.CreatedAt,
                    COALESCE(u.Role, 'System') AS Role,
                    COALESCE(u.FirstName, al.Username, 'System') AS Name,
                    al.Action,
                    al.Module,
                    al.Description
                FROM ActivityLogs al
                LEFT JOIN Users u ON al.UserId = u.UserId
                WHERE strftime('%Y-%m-%d', al.CreatedAt) BETWEEN @From AND @To
                ORDER BY al.CreatedAt DESC";
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string action = SafeString(reader, 3);
                string actionColor = ResolveActionColor(action);
                items.Add(new ActivityLogReportItem
                {
                    Timestamp = SafeString(reader, 0),
                    Role = SafeString(reader, 1),
                    Name = SafeString(reader, 2),
                    Action = action,
                    ActionColor = actionColor,
                    Module = SafeString(reader, 4),
                    Details = SafeString(reader, 5),
                });
            }
            return items;
        }

        public List<PieChartSlice> GetActivityByType(string fromDate, string toDate)
        {
            var raw = new List<(string action, double count)>();
            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Action, COUNT(*) AS ActionCount
                FROM ActivityLogs
                WHERE strftime('%Y-%m-%d', CreatedAt) BETWEEN @From AND @To
                GROUP BY Action
                ORDER BY ActionCount DESC";
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                raw.Add((SafeString(reader, 0), reader.IsDBNull(1) ? 0 : reader.GetDouble(1)));

            double total = raw.Sum(r => r.count);
            if (total == 0) return new List<PieChartSlice>();

            string[] colors = { "#50C878", "#2F98D0", "#E53935", "#FF981D", "#A855F7", "#223357" };
            var slices = new List<PieChartSlice>();
            for (int i = 0; i < raw.Count; i++)
            {
                slices.Add(new PieChartSlice
                {
                    Label = raw[i].action,
                    Value = raw[i].count,
                    HexColor = colors[i % colors.Length],
                    Percentage = Math.Round(raw[i].count / total * 100, 0),
                });
            }
            return slices;
        }

        public List<ChartDataPoint> GetActivityByModule(string fromDate, string toDate)
        {
            var items = new List<ChartDataPoint>();
            using var conn = DatabaseService.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Module, COUNT(*) AS ModuleCount
                FROM ActivityLogs
                WHERE strftime('%Y-%m-%d', CreatedAt) BETWEEN @From AND @To
                GROUP BY Module
                ORDER BY ModuleCount DESC";
            cmd.Parameters.AddWithValue("@From", fromDate);
            cmd.Parameters.AddWithValue("@To", toDate);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                items.Add(new ChartDataPoint
                {
                    Label = SafeString(reader, 0),
                    Value = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                });
            return items;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string SafeString(SqliteDataReader reader, int col)
            => reader.IsDBNull(col) ? "" : reader.GetString(col);

        // Reads a possibly-encrypted (or plain numeric) amount column safely.
        private static decimal SecureDecimal(SqliteDataReader reader, int col)
        {
            if (reader.IsDBNull(col))
                return 0m;

            string raw = reader.GetValue(col)?.ToString() ?? string.Empty;
            return CryptoService.DecryptDecimal(raw, 0m);
        }

        private static string ResolveActionColor(string action)
        {
            string lower = action.ToLowerInvariant();
            if (lower.Contains("login") || lower.Contains("create") || lower.Contains("add"))
                return "#50C878";
            if (lower.Contains("update") || lower.Contains("edit") || lower.Contains("modify"))
                return "#2F98D0";
            if (lower.Contains("delete") || lower.Contains("archive") || lower.Contains("remove"))
                return "#E53935";
            return "#555555";
        }
    }
}
