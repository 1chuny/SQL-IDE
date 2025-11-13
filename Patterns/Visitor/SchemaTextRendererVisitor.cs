using SqlIdeProject.Models.Schema;
using System.Text;

namespace SqlIdeProject.Patterns.Visitor
{
    public class SchemaTextRendererVisitor : ISchemaVisitor
    {
        private readonly StringBuilder _stringBuilder = new();
        private int _indent = 0;

        public string GetResult() => _stringBuilder.ToString();

        public void Visit(SchemaRoot schema)
        {
            _stringBuilder.AppendLine("Схема БД:");
        }

        public void Visit(TableElement table)
        {
            _indent++;
            _stringBuilder.AppendLine($"{new string(' ', _indent * 2)}- Таблиця: {table.Name}");
        }

        public void Visit(ColumnElement column)
        {
            _indent++;
            _stringBuilder.AppendLine($"{new string(' ', _indent * 2)}- Стовпець: {column.Name} ({column.DataType})");
            _indent--; // Повертаємо відступ назад після стовпця
        }
    }
}