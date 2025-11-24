// File: Services/ConnectionService.cs
using SqlIdeProject.DataAccess;
using SqlIdeProject.DataAccess.Drivers;
using System;
using System.Collections.Generic;

namespace SqlIdeProject.Services
{
    /// <summary>
    /// Сервіс-ОДИНАК для керування активними підключеннями до БД.
    /// </summary>
    public sealed class ConnectionService
    {
        // 1. Статичне поле, що зберігає єдиний екземпляр класу.
        private static ConnectionService? _instance;

        // Об'єкт для потокобезпечної ініціалізації.
        private static readonly object _lock = new object();

        // Словник для зберігання активних підключень. Ключ - унікальний ID, значення - менеджер.
        private readonly Dictionary<string, ConnectionManager> _activeConnections = new();

        // 2. Приватний конструктор, щоб ніхто не міг створити екземпляр ззовні.
        private ConnectionService() { }

        // 3. Публічна статична властивість для доступу до єдиного екземпляра.
        public static ConnectionService Instance
        {
            get
            {
                // Потокобезпечна перевірка, щоб екземпляр створювався лише один раз.
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ConnectionService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Створює або повертає існуючий ConnectionManager за його ID.
        /// </summary>
        public ConnectionManager GetOrCreateConnection(string connectionId, string dbType, string connectionString)
        {
            if (_activeConnections.ContainsKey(connectionId))
            {
                return _activeConnections[connectionId];
            }

            // Логіка вибору драйвера на основі dbType
            IDatabaseDriver driver = dbType switch
				{
					"PostgreSQL" => new PostgreSqlDriver(),
					"MySQL" => new MySqlDriver(),
					"Oracle" => new OracleDriver(),
					"SQLite" => new SqliteDriver(),
					_ => throw new NotSupportedException($"Database type '{dbType}' is not supported.")
				};

            var connectionManager = new ConnectionManager(driver);
            connectionManager.Connect(connectionString);
            _activeConnections[connectionId] = connectionManager;
            return connectionManager;
        }
		  public void Disconnect(string connectionId)
        {
            if (_activeConnections.ContainsKey(connectionId))
            {
                // 1. Фізично закриваємо з'єднання
                try 
                { 
                    _activeConnections[connectionId].Disconnect(); 
                } 
                catch { /* ігноруємо помилки при відключенні */ }

                // 2. Видаляємо його зі списку пам'яті
                _activeConnections.Remove(connectionId);
            }
        }
    }
}