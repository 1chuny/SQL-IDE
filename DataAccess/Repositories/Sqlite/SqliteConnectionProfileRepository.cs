// File: DataAccess/Repositories/Sqlite/SqliteConnectionProfileRepository.cs
using Microsoft.Data.Sqlite;
using SqlIdeProject.Models.Internal;
using SqlIdeProject.Services; // Потрібно для ConnectionString
using System.Collections.Generic;

namespace SqlIdeProject.DataAccess.Repositories.Sqlite
{
    // Цей клас реалізує наш "контракт" (інтерфейс)
    public class SqliteConnectionProfileRepository : IConnectionProfileRepository
    {
        // Ми беремо рядок підключення з нашого сервісу
        private readonly string _connectionString = InternalDatabaseService.ConnectionString;

        public void Add(ConnectionProfile entity)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = 
                    "INSERT INTO ConnectionProfiles (ProfileName, DatabaseType, ConnectionString, LastUsed) " +
                    "VALUES ($name, $type, $connString, $lastUsed)";

                command.Parameters.AddWithValue("$name", entity.ProfileName);
                command.Parameters.AddWithValue("$type", entity.DatabaseType);
                command.Parameters.AddWithValue("$connString", entity.ConnectionString);
                command.Parameters.AddWithValue("$lastUsed", System.DateTime.Now);

                command.ExecuteNonQuery();
            }
        }

        public IEnumerable<ConnectionProfile> GetAll()
        {
            var profiles = new List<ConnectionProfile>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, ProfileName, DatabaseType, ConnectionString FROM ConnectionProfiles";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        profiles.Add(new ConnectionProfile
                        {
                            Id = reader.GetInt32(0),
                            ProfileName = reader.GetString(1),
                            DatabaseType = reader.GetString(2),
                            ConnectionString = reader.GetString(3)
                        });
                    }
                }
            }
            return profiles;
        }

        // Ми поки що не будемо реалізовувати всі методи,
        // щоб не писати забагато коду
        public ConnectionProfile? GetByName(string name)
         {
            ConnectionProfile? profile = null;
            using (var connection = new SqliteConnection(_connectionString))
            {
               connection.Open();
               var command = connection.CreateCommand();
               command.CommandText = "SELECT Id, ProfileName, DatabaseType, ConnectionString FROM ConnectionProfiles WHERE ProfileName = $name";
               command.Parameters.AddWithValue("$name", name);

               using (var reader = command.ExecuteReader())
               {
                  if (reader.Read()) // шукаємо лише один
                  {
                     profile = new ConnectionProfile
                     {
                        Id = reader.GetInt32(0),
                        ProfileName = reader.GetString(1),
                        DatabaseType = reader.GetString(2),
                        ConnectionString = reader.GetString(3)
                     };
                  }
               }
            }
            return profile; // Поверне null, якщо нічого не знайдено
         }

        public void Delete(ConnectionProfile entity)
        {
            throw new System.NotImplementedException();
        }

        public ConnectionProfile GetById(int id)
        {
            throw new System.NotImplementedException();
        }

        public void Update(ConnectionProfile entity)
        {
            throw new System.NotImplementedException();
        }
    }
}