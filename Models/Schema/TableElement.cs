using SqlIdeProject.Patterns.Visitor;
using System.Collections.Generic;

namespace SqlIdeProject.Models.Schema
{
    public class TableElement : ISchemaElement
    {
        public string Name { get; }
        public List<ColumnElement> Columns { get; } = new();

        public TableElement(string name) { Name = name; }

        public void Accept(ISchemaVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var column in Columns)
            {
                column.Accept(visitor);
            }
        }
    }
}