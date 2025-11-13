// File: DataAccess/ConnectionManager.cs
using SqlIdeProject.DataAccess.Drivers;
using System;
using System.Data;
using SqlIdeProject.Models.Schema;

namespace SqlIdeProject.DataAccess
{
    // МІСТ: Клас, що використовує абстракцію драйвера
    public class ConnectionManager
    {
        private readonly IDatabaseDriver _driver;

        // Конструктор приймає будь-який об'єкт, що реалізує IDatabaseDriver
        public ConnectionManager(IDatabaseDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        public void Connect(string connectionString)
        {
            _driver.Connect(connectionString);
        }

        public void Disconnect()
        {
            _driver.Disconnect();
        }

        public DataTable GetQueryResults(string query)
        {
            return _driver.ExecuteQuery(query);
        }

        public int RunCommand(string query)
        {
            return _driver.ExecuteNonQuery(query);
        }

        public SchemaRoot GetSchema() 
        {
            return _driver.GetSchema();
        }
    }
}