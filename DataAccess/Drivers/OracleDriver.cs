// File: DataAccess/Drivers/OracleDriver.cs
using Oracle.ManagedDataAccess.Client;
using SqlIdeProject.Models.Schema;
using System;
using System.Data;

namespace SqlIdeProject.DataAccess.Drivers
{
    public class OracleDriver : IDatabaseDriver
    {
        private OracleConnection? _connection;

        public void Connect(string connectionString)
        {
            _connection = new OracleConnection(connectionString);
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
            using (var command = new OracleCommand(query, _connection))
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
            using (var command = new OracleCommand(query, _connection))
            {
                return command.ExecuteNonQuery();
            }
        }

        // Отримання схеми для Oracle (показуємо таблиці поточного користувача)
        public SchemaRoot GetSchema()
        {
            if (_connection == null) throw new InvalidOperationException("Not connected to database.");
            var schema = new SchemaRoot();

            // 1. Отримуємо таблиці, що належать поточному користувачу
            string tablesQuery = "SELECT table_name FROM user_tables ORDER BY table_name";
            var tablesTable = ExecuteQuery(tablesQuery);

            foreach (DataRow row in tablesTable.Rows)
            {
                string tableName = row["table_name"].ToString()!;
                var tableElement = new TableElement(tableName);

                // 2. Для кожної таблиці отримуємо її стовпці
                string columnsQuery = $"SELECT column_name, data_type FROM user_tab_columns WHERE table_name = '{tableName}' ORDER BY column_id";
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