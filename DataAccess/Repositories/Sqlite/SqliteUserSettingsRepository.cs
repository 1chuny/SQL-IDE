using Microsoft.Data.Sqlite;
using SqlIdeProject.Services;

namespace SqlIdeProject.DataAccess.Repositories.Sqlite
{
    public class SqliteUserSettingsRepository : IUserSettingsRepository
    {
        private readonly string _connectionString = InternalDatabaseService.ConnectionString;

        public string? GetSetting(string key)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT SettingValue FROM UserSettings WHERE SettingKey = $key";
                command.Parameters.AddWithValue("$key", key);

                // ExecuteScalar повертає перший стовпець першого рядка (або null)
                var result = command.ExecuteScalar();
                return result as string;
            }
        }

        public void SaveSetting(string key, string value)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                
                // "UPSERT": Оновити запис, якщо він існує, або вставити, якщо ні.
                command.CommandText = "INSERT OR REPLACE INTO UserSettings (SettingKey, SettingValue) " +
                                      "VALUES ($key, $value)";
                
                command.Parameters.AddWithValue("$key", key);
                command.Parameters.AddWithValue("$value", value);
                command.ExecuteNonQuery();
            }
        }
    }
}