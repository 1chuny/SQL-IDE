// File: Patterns/Visitor/SchemaComparisonVisitor.cs
using SqlIdeProject.Models.Schema;
using System.Collections.Generic;
using System.Linq;

namespace SqlIdeProject.Patterns.Visitor
{
    // Цей клас є "Конкретним Відвідувачем" (Concrete Visitor).
    // Він реалізує алгоритм порівняння двох схем, не змінюючи класи самої схеми.
    public class SchemaComparisonVisitor : ISchemaVisitor
    {
        private readonly SchemaRoot _schemaToCompare;
        
        // Зберігає посилання на таблицю, яку ми зараз обходимо.
        // Це потрібно, щоб при відвідуванні стовпця знати, до якої таблиці він належить.
        private TableElement? _currentTableInSchema1;

        public List<string> Differences { get; } = new();

        public SchemaComparisonVisitor(SchemaRoot schemaToCompare)
        {
            _schemaToCompare = schemaToCompare;
        }

        // Точка входу в алгоритм.
        public void Visit(SchemaRoot schema)
        {
            Differences.Add("Початок порівняння схем...");
        }

        // Відвідування таблиці. Тут ми перевіряємо наявність таблиці в другій базі.
        public void Visit(TableElement table)
        {
            _currentTableInSchema1 = table; // Зберігаємо контекст для подальшого обходу стовпців
            
            // Шукаємо таку ж таблицю в другій схемі
            var tableInSchema2 = _schemaToCompare.Tables.FirstOrDefault(t => t.Name == table.Name);

            if (tableInSchema2 == null)
            {
                Differences.Add($"- Таблиця '{table.Name}' існує в першій схемі, але відсутня в другій.");
            }
        }

        // Відвідування стовпця. 
        public void Visit(ColumnElement column)
        {
            // Якщо ми не знаємо, в якій ми таблиці, виходимо (захист).
            if (_currentTableInSchema1 == null) return;

            // 1. Знову знаходимо відповідну таблицю в другій схемі
            var tableInSchema2 = _schemaToCompare.Tables.FirstOrDefault(t => t.Name == _currentTableInSchema1.Name);
            if (tableInSchema2 == null) return; 

            // 2. Шукаємо відповідний стовпець у цій таблиці
            var columnInSchema2 = tableInSchema2.Columns.FirstOrDefault(c => c.Name == column.Name);

            // 3. Перевіряємо наявність та відповідність типів
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