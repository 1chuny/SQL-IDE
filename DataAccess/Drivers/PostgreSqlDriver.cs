// File: DataAccess/Drivers/PostgreSqlDriver.cs
using Npgsql;
using SqlIdeProject.Models.Schema;
using System;
using System.Collections.Generic;
using System.Data;

namespace SqlIdeProject.DataAccess.Drivers
{
    public class PostgreSqlDriver : IDatabaseDriver
    {
        private NpgsqlConnection? _connection;

        public void Connect(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
        }

        public void Disconnect()
        {
            _connection?.Close();
            _connection = null;
        }

        public DataTable ExecuteQuery(string query)
        {
            if (_connection == null) throw new InvalidOperationException("Not connected to database.");
            var dataTable = new DataTable();
            using (var command = new NpgsqlCommand(query, _connection))
            {
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
            using (var command = new NpgsqlCommand(query, _connection))
            {
                return command.ExecuteNonQuery();
            }
        }

        // Логіка отримання схеми для PostgreSQL ВІДРІЗНЯЄТЬСЯ
        public SchemaRoot GetSchema()
        {
            if (_connection == null) throw new InvalidOperationException("Not connected to database.");
            var schema = new SchemaRoot();

            // 1. Отримуємо таблиці з системної таблиці information_schema
            string tablesQuery = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name;";
            var tablesTable = ExecuteQuery(tablesQuery);

            foreach (DataRow row in tablesTable.Rows)
            {
                string tableName = row["table_name"].ToString()!;
                var tableElement = new TableElement(tableName);

                // 2. Для кожної таблиці отримуємо її стовпці
                string columnsQuery = $"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = '{tableName}' ORDER BY ordinal_position;";
                var columnsTable = ExecuteQuery(columnsQuery);

                foreach (DataRow colRow in columnsTable.Rows)
                {
                    string colName = colRow["column_name"].ToString()!;
                    string colType = colRow["data_type"].ToString()!;
                    tableElement.Columns.Add(new ColumnElement(colName, colType));
                }
                schema.Tables.Add(tableElement);
            }
            return schema;
        }
    }
}