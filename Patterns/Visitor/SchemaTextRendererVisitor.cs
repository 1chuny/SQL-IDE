// File: Patterns/Visitor/SchemaTextRendererVisitor.cs
using SqlIdeProject.Models.Schema;
using System.Text;

namespace SqlIdeProject.Patterns.Visitor
{
    // Реалізація "Відвідувача" (Visitor) для перетворення схеми в текст.
    public class SchemaTextRendererVisitor : ISchemaVisitor
    {
        private readonly StringBuilder _stringBuilder = new();
        private int _indent = 0; 

        public string GetResult() => _stringBuilder.ToString();

        // Метод для відвідування кореня (вхідна точка)
        public void Visit(SchemaRoot schema)
        {
            _stringBuilder.AppendLine("Схема БД:");
        }

        // Метод для відвідування Таблиці
        public void Visit(TableElement table)
        {
            _indent++; // Збільшуємо відступ для вкладеності
            _stringBuilder.AppendLine($"{new string(' ', _indent * 2)}- Таблиця: {table.Name}");
        }

        // Метод для відвідування Стовпця 
        public void Visit(ColumnElement column)
        {
            _indent++;
            _stringBuilder.AppendLine($"{new string(' ', _indent * 2)}- Стовпець: {column.Name} ({column.DataType})");
            _indent--; // Повертаємо відступ назад після завершення
        }
    }
}