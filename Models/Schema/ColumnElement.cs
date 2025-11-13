using SqlIdeProject.Patterns.Visitor;

namespace SqlIdeProject.Models.Schema
{
    public class ColumnElement : ISchemaElement
    {
        public string Name { get; }
        public string DataType { get; }

        public ColumnElement(string name, string dataType) { Name = name; DataType = dataType; }

        public void Accept(ISchemaVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}