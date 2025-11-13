// File: DataAccess/Drivers/MySqlDriver.cs
using MySql.Data.MySqlClient;
using SqlIdeProject.Models.Schema;
using System;
using System.Data;

namespace SqlIdeProject.DataAccess.Drivers
{
    public class MySqlDriver : IDatabaseDriver
    {
        private MySqlConnection? _connection;

        public void Connect(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
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
            using (var command = new MySqlCommand(query, _connection))
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
            using (var command = new MySqlCommand(query, _connection))
            {
                return command.ExecuteNonQuery();
            }
        }

        // Получение схемы для MySQL (используем information_schema)
        public SchemaRoot GetSchema()
        {
            if (_connection == null) throw new InvalidOperationException("Not connected to database.");
            var schema = new SchemaRoot();

            // 1. Получаем таблицы для текущей базы данных
            string tablesQuery = $"SELECT table_name FROM information_schema.tables WHERE table_schema = '{_connection.Database}' ORDER BY table_name;";
            var tablesTable = ExecuteQuery(tablesQuery);

            foreach (DataRow row in tablesTable.Rows)
            {
                string tableName = row["table_name"].ToString()!;
                var tableElement = new TableElement(tableName);

                // 2. Для каждой таблицы получаем ее столбцы
                string columnsQuery = $"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = '{tableName}' AND table_schema = '{_connection.Database}' ORDER BY ordinal_position;";
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