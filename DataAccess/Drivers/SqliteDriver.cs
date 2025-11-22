// File: DataAccess/Drivers/SqliteDriver.cs
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using SqlIdeProject.Models.Schema; 
using System.Collections.Generic;

namespace SqlIdeProject.DataAccess.Drivers
{
    
    public class SqliteDriver : IDatabaseDriver
    {
        private SqliteConnection? _connection;

        public void Connect(string connectionString)
        {
            try
            {
                _connection = new SqliteConnection(connectionString);
                _connection.Open();
                Console.WriteLine("SQLite connection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQLite connection failed: {ex.Message}");
                _connection = null;
            }
        }

        public void Disconnect()
        {
            _connection?.Close();
            _connection = null;
            Console.WriteLine("SQLite connection closed.");
        }

        public DataTable ExecuteQuery(string query)
        {
            if (_connection == null) throw new InvalidOperationException("Not connected to database.");
            
            var dataTable = new DataTable();
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = query;
                using (var reader = command.ExecuteReader())
                {
                    dataTable.Load(reader);
                }
            }
            return dataTable;
        }

        public int ExecuteNonQuery(string query)
        {
            if (_connection == null) throw new InvalidOperationException("Not connected to database.");

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = query;
                return command.ExecuteNonQuery();
            }
        }
        
        public SchemaRoot GetSchema()
        {
            if (_connection == null) throw new InvalidOperationException("Not connected to database.");

            var schema = new SchemaRoot();
            var tableNames = new List<string>();

            // 1. Отримуємо імена всіх таблиць
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tableNames.Add(reader.GetString(0));
                    }
                }
            }

            // 2. Для кожної таблиці отримуємо її стовпці
            foreach (var tableName in tableNames)
            {
                var tableElement = new TableElement(tableName);
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = $"PRAGMA table_info('{tableName}');";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string colName = reader.GetString(1);
                            string colType = reader.GetString(2);
                            tableElement.Columns.Add(new ColumnElement(colName, colType));
                        }
                    }
                }
                schema.Tables.Add(tableElement);
            }

            return schema;
        }
    }
}