using SqlIdeProject.Patterns.Visitor;

namespace SqlIdeProject.Models.Schema
{
    public interface ISchemaElement
    {
        void Accept(ISchemaVisitor visitor);
    }
}