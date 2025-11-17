// File: Services/InternalDatabaseService.cs
using Microsoft.Data.Sqlite;

namespace SqlIdeProject.Services
{
    // Цей клас відповідає за створення та підготовку нашої ВНУТРІШНЬОЇ БД
    public class InternalDatabaseService
    {
        // Назва нашого файлу налаштувань
        private const string DbName = "ide_settings.db";
        public static string ConnectionString => $"Data Source={DbName}";

        // Цей метод ми викличемо один раз при запуску програми
        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();

                // 1. Створюємо таблицю для Профілів
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ConnectionProfiles (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProfileName TEXT NOT NULL UNIQUE,
                        DatabaseType TEXT NOT NULL,
                        ConnectionString TEXT NOT NULL,
                        LastUsed DATETIME
                    );";
                command.ExecuteNonQuery();

                // 2. Створюємо таблицю для Історії
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS QueryHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        QueryText TEXT NOT NULL,
                        ExecutionTime DATETIME NOT NULL,
                        WasSuccessful BOOLEAN NOT NULL,
                        ConnectionProfileId INTEGER NULLABLE,
                        FOREIGN KEY (ConnectionProfileId) REFERENCES ConnectionProfiles (Id)
                    );";
                command.ExecuteNonQuery();

                // 3. Створюємо таблицю для Налаштувань
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS UserSettings (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SettingKey TEXT NOT NULL UNIQUE,
                        SettingValue TEXT
                    );";
                command.ExecuteNonQuery();
            }
        }
    }
}