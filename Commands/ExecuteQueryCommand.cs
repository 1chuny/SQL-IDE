// File: Commands/ExecuteQueryCommand.cs
using SqlIdeProject.DataAccess;
using System.Data;

namespace SqlIdeProject.Commands
{
    // Цей клас є "Конкретною Командою" (Concrete Command) у патерні Command.
    // Він реалізує спільний інтерфейс ICommand, щоб його міг використовувати будь-який "Виконавець" (Invoker).
    public class ExecuteQueryCommand : ICommand
    {
        // Receiver (Отримувач)
        private readonly ConnectionManager _connectionManager;

        // Стан команди: дані, необхідні для виконання дії (текст SQL-запиту).
        private readonly string _query;

        // Результат виконання команди.
        // Використовується для відображення результатів SELECT у таблиці.
        public DataTable? Result { get; private set; }

        // Конструктор: 
        public ExecuteQueryCommand(ConnectionManager connectionManager, string query)
        {
            _connectionManager = connectionManager;
            _query = query;
        }

        // Головний метод патерну Command.
        public void Execute()
        {
            // Команда передає управління Отримувачу (_connectionManager).
            // Результат роботи зберігається у властивості Result.
            Result = _connectionManager.GetQueryResults(_query);
        }
    }
}