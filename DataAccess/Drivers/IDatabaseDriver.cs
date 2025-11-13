// File: DataAccess/Drivers/IDatabaseDriver.cs
using System.Data;
using SqlIdeProject.Models.Schema;

namespace SqlIdeProject.DataAccess.Drivers
{
    // АБСТРАКЦІЯ: Описує загальні можливості для будь-якого драйвера БД
    public interface IDatabaseDriver
    {
        // Метод для підключення до бази даних
        void Connect(string connectionString);

        // Метод для відключення
        void Disconnect();

        // Метод для виконання запиту, що повертає дані (SELECT)
        DataTable ExecuteQuery(string query);

        // Метод для виконання команд, що не повертають дані (INSERT, CREATE, etc.)
        int ExecuteNonQuery(string query);

        SchemaRoot GetSchema();
    }
}