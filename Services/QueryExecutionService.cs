// File: Services/QueryExecutionService.cs
using SqlIdeProject.Commands;
using System;
using System.Data;

namespace SqlIdeProject.Services
{
    public class QueryExecutionService
    {
        // 1. Оголошуємо подію, на яку зможуть підписатися інші об'єкти
        public event EventHandler<QueryCompletedEventArgs>? QueryCompleted;

        public void Submit(ICommand command)
        {
            try
            {
                command.Execute();

                // Отримуємо результат з команди (поки що тільки для ExecuteQueryCommand)
                if (command is ExecuteQueryCommand queryCommand)
                {
                    // 2. Генеруємо подію про успішне завершення, передаючи результат
                    OnQueryCompleted(new QueryCompletedEventArgs(queryCommand.Result!));
                }
            }
            catch (Exception ex)
            {
                // 3. Генеруємо подію про помилку
                OnQueryCompleted(new QueryCompletedEventArgs(ex));
            }
        }

        // Метод для безпечного виклику події
        private void OnQueryCompleted(QueryCompletedEventArgs args)
        {
            QueryCompleted?.Invoke(this, args);
        }
    }
}