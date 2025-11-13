// File: Commands/ExecuteQueryCommand.cs
using SqlIdeProject.DataAccess;
using System.Data;

namespace SqlIdeProject.Commands
{
    public class ExecuteQueryCommand : ICommand
    {
        private readonly ConnectionManager _connectionManager;
        private readonly string _query;

        // Результат, який команда зберігає після виконання
        public DataTable? Result { get; private set; }

        public ExecuteQueryCommand(ConnectionManager connectionManager, string query)
        {
            _connectionManager = connectionManager;
            _query = query;
        }

        public void Execute()
        {
            Result = _connectionManager.GetQueryResults(_query);
        }
    }
}