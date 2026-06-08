using System;
using System.IO;
using System.Linq;

namespace CruzNeryClinic.Services
{
    public static class BackupRetentionService
    {
        public static int? GetKeepCount(string retentionOption)
            => retentionOption switch
            {
                "Keep last 7 backups" => 7,
                "Keep last 14 backups" => 14,
                "Keep last 30 backups" => 30,
                "Keep all backups" => null,
                _ => 14
            };

        public static void Apply(string backupFolder, int? keepCount)
        {
            if (keepCount == null ||
                string.IsNullOrWhiteSpace(backupFolder) ||
                !Directory.Exists(backupFolder))
            {
                return;
            }

            FileInfo[] backupFiles = new DirectoryInfo(backupFolder)
                .GetFiles("*.cnbak")
                .OrderByDescending(file => file.CreationTime)
                .ToArray();

            foreach (FileInfo oldBackup in backupFiles.Skip(keepCount.Value))
                oldBackup.Delete();
        }
    }
}
