using Microsoft.Data.Sqlite;
using System;

namespace CruzNeryClinic.Data
{
    public static class SqliteCommandExtensions
    {
        public static SqliteCommand CreateCommand(this SqliteConnection connection, string commandText)
        {
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            return command;
        }

        public static SqliteCommand CreateCommand(
            this SqliteConnection connection,
            string commandText,
            SqliteTransaction transaction)
        {
            SqliteCommand command = connection.CreateCommand(commandText);
            command.Transaction = transaction;
            return command;
        }

        public static SqliteCommand AddParameter(this SqliteCommand command, string name, object? value)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            return command;
        }

        public static SqliteCommand AddTextParameter(this SqliteCommand command, string name, string? value)
            => command.AddParameter(name, value ?? string.Empty);

        public static SqliteCommand AddNullableParameter(this SqliteCommand command, string name, object? value)
            => command.AddParameter(name, value ?? DBNull.Value);

        public static SqliteCommand AddIntParameter(this SqliteCommand command, string name, int value)
            => command.AddParameter(name, value);

        public static SqliteCommand AddBoolParameter(this SqliteCommand command, string name, bool value)
            => command.AddParameter(name, value ? 1 : 0);

        public static SqliteCommand AddDateParameter(this SqliteCommand command, string name, DateTime value)
            => command.AddParameter(name, value.ToString("yyyy-MM-dd"));

        public static SqliteCommand AddDateTimeParameter(this SqliteCommand command, string name, DateTime value)
            => command.AddParameter(name, value.ToString("yyyy-MM-dd HH:mm:ss"));

        public static SqliteCommand AddLikeParameter(this SqliteCommand command, string name, string? value)
            => command.AddParameter(name, $"%{(value ?? string.Empty).Trim()}%");
    }
}
