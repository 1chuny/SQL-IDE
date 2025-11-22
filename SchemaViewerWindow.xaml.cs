using System.Windows;
using SqlIdeProject.Models.Schema;
using SqlIdeProject.Utils;

namespace SqlIdeProject
{
    public partial class SchemaViewerWindow : Window
    {
        public SchemaViewerWindow(SchemaRoot schema)
        {
            InitializeComponent();
            // Використовуємо нашого помічника, щоб намалювати дерево
            SchemaTreeBuilder.BuildTree(MainTree, schema);
        }
    }
}