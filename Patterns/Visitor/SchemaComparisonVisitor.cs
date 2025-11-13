// File: Patterns/Visitor/SchemaComparisonVisitor.cs
using SqlIdeProject.Models.Schema;
using System.Collections.Generic;
using System.Linq;

namespace SqlIdeProject.Patterns.Visitor
{
    public class SchemaComparisonVisitor : ISchemaVisitor
    {
        private readonly SchemaRoot _schemaToCompare;
        private TableElement? _currentTableInSchema1;

        public List<string> Differences { get; } = new();

        public SchemaComparisonVisitor(SchemaRoot schemaToCompare)
        {
            _schemaToCompare = schemaToCompare;
        }

        public void Visit(SchemaRoot schema)
        {
            Differences.Add("Початок порівняння схем...");
        }

        public void Visit(TableElement table)
        {
            _currentTableInSchema1 = table; // Запам'ятовуємо поточну таблицю
            var tableInSchema2 = _schemaToCompare.Tables.FirstOrDefault(t => t.Name == table.Name);

            if (tableInSchema2 == null)
            {
                Differences.Add($"- Таблиця '{table.Name}' існує в першій схемі, але відсутня в другій.");
            }
        }

        public void Visit(ColumnElement column)
        {
            if (_currentTableInSchema1 == null) return;

            // Знаходимо відповідну таблицю в другій схемі
            var tableInSchema2 = _schemaToCompare.Tables.FirstOrDefault(t => t.Name == _currentTableInSchema1.Name);
            if (tableInSchema2 == null) return; // Відмінність таблиці вже зафіксована

            // Шукаємо відповідний стовпець
            var columnInSchema2 = tableInSchema2.Columns.FirstOrDefault(c => c.Name == column.Name);

            if (columnInSchema2 == null)
            {
                Differences.Add($"- У таблиці '{_currentTableInSchema1.Name}': стовпець '{column.Name}' відсутній у другій схемі.");
            }
            else if (columnInSchema2.DataType != column.DataType)
            {
                Differences.Add($"- У таблиці '{_currentTableInSchema1.Name}': стовпець '{column.Name}' має різний тип даних ('{column.DataType}' vs '{columnInSchema2.DataType}').");
            }
        }
    }
}