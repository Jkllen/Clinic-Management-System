using CruzNeryClinic.Data;
using Microsoft.Data.Sqlite;
using System;

namespace CruzNeryClinic.Services
{
    // Central helper for recording user activity into the ActivityLogs table.
    // It reads the currently logged-in user from SessionService, so callers only
    // need to supply the action, the module, and a human-readable description.
    //
    // Logging is best-effort: any failure here is swallowed so that recording an
    // activity can never break the primary operation the user performed.
    public static class ActivityLogService
    {
        public static void Log(string action, string module, string description)
        {
            try
            {
                var user = SessionService.CurrentUser;

                using var connection = DatabaseService.GetConnection();
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = @"
INSERT INTO ActivityLogs (
    UserId, Username, Action, Module, Description, CreatedAt
) VALUES (
    @UserId, @Username, @Action, @Module, @Description, @CreatedAt
);";

                command.Parameters.AddWithValue("@UserId", (object?)user?.UserId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Username", user?.Username ?? "System");
                command.Parameters.AddWithValue("@Action", action ?? string.Empty);
                command.Parameters.AddWithValue("@Module", module ?? string.Empty);
                command.Parameters.AddWithValue("@Description", description ?? string.Empty);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
            }
            catch
            {
                // Activity logging is non-critical; never let it surface to the caller.
            }
        }
    }
}
