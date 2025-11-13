using SqlIdeProject.Models.Schema;

namespace SqlIdeProject.Patterns.Visitor
{
    public interface ISchemaVisitor
    {
        void Visit(SchemaRoot schema);
        void Visit(TableElement table);
        void Visit(ColumnElement column);
    }
}