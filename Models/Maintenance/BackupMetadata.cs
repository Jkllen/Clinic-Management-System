using System;

namespace CruzNeryClinic.Models.Maintenance
{
    public class BackupMetadata
    {
        public string BackupId { get; set; } = Guid.NewGuid().ToString();

        public string AppName { get; set; } = "Cruz-Nery Dental Clinic Management System";

        public string BackupType { get; set; } = "Manual";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string DatabaseFileName { get; set; } = string.Empty;

        public string KeyFileName { get; set; } = "clinic_data.key";

        public string Version { get; set; } = "1.0";
    }
}