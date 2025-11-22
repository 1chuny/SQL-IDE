// File: DataAccess/Repositories/Sqlite/SqliteQueryHistoryRepository.cs
using Microsoft.Data.Sqlite;
using SqlIdeProject.Models.Internal;
using SqlIdeProject.Services;
using System;
using System.Collections.Generic;

namespace SqlIdeProject.DataAccess.Repositories.Sqlite
{
    public class SqliteQueryHistoryRepository : IQueryHistoryRepository
    {
        private readonly string _connectionString = InternalDatabaseService.ConnectionString;

        public void Add(QueryHistory entity)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = 
                    "INSERT INTO QueryHistory (QueryText, ExecutionTime, WasSuccessful, ConnectionProfileId) " +
                    "VALUES ($query, $time, $success, $profileId)";

                command.Parameters.AddWithValue("$query", entity.QueryText);
                command.Parameters.AddWithValue("$time", entity.ExecutionTime);
                command.Parameters.AddWithValue("$success", entity.WasSuccessful);
                // Дозволяємо ID профілю бути NULL, якщо це не було збережене підключення
                command.Parameters.AddWithValue("$profileId", (object)entity.ConnectionProfileId ?? DBNull.Value);

                command.ExecuteNonQuery();
            }
        }

        public IEnumerable<QueryHistory> GetAll()
        {
            var history = new List<QueryHistory>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // Отримуємо останні 100 записів
                command.CommandText = "SELECT Id, QueryText, ExecutionTime, WasSuccessful, ConnectionProfileId " +
                                      "FROM QueryHistory ORDER BY ExecutionTime DESC LIMIT 100";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        history.Add(new QueryHistory
                        {
                            Id = reader.GetInt32(0),
                            QueryText = reader.GetString(1),
                            ExecutionTime = reader.GetDateTime(2),
                            WasSuccessful = reader.GetBoolean(3),
                            ConnectionProfileId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                        });
                    }
                }
            }
            return history;
        }

        public IEnumerable<QueryHistory> GetHistoryForProfile(int profileId)
        {
            throw new NotImplementedException();
        }

        public QueryHistory GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(QueryHistory entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(QueryHistory entity)
        {
            throw new NotImplementedException();
        }
    }
}