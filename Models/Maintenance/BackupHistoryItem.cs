using System;

namespace CruzNeryClinic.Models.Maintenance
{
    public class BackupHistoryItem
    {
        public DateTime CreatedAt { get; set; }

        public string BackupType { get; set; } = "Manual";

        public string FileLocation { get; set; } = string.Empty;

        public string SizeDisplay { get; set; } = string.Empty;

        public string Status { get; set; } = "Success";

        public string CreatedAtDisplay => CreatedAt.ToString("yyyy-MM-dd HH:mm");
    }
}