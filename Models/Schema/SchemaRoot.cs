using SqlIdeProject.Patterns.Visitor;
using System.Collections.Generic;

namespace SqlIdeProject.Models.Schema
{
    public class SchemaRoot : ISchemaElement
    {
        public List<TableElement> Tables { get; } = new();

        public void Accept(ISchemaVisitor visitor)
        {
            visitor.Visit(this); // Спочатку відвідуємо сам корінь
            foreach (var table in Tables)
            {
                table.Accept(visitor); // Потім кожну таблицю
            }
        }
    }
}