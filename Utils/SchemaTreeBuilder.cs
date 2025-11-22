using System.Windows.Controls;
using System.Windows.Media;
using SqlIdeProject.Models.Schema;

namespace SqlIdeProject.Utils
{
    public static class SchemaTreeBuilder
    {
        public static void BuildTree(TreeView treeView, SchemaRoot schema)
        {
            treeView.Items.Clear();

            foreach (var table in schema.Tables)
            {
                // Створюємо вузол Таблиці
                var tableNode = new TreeViewItem();
                // Використовуємо іконку таблиці (▦)
                tableNode.Header = $"▦ {table.Name}"; 
                tableNode.Foreground = Brushes.White; // Або Black, залежно від теми, але нехай буде світлим для темної
                tableNode.FontSize = 14;
                tableNode.IsExpanded = false; // Спочатку згорнуто

                foreach (var column in table.Columns)
                {
                    // Створюємо вузол Стовпця
                    var colNode = new TreeViewItem();
                    // Використовуємо іконку стовпця і додаємо тип даних
                    colNode.Header = $"   ◫  {column.Name}  :  {column.DataType}";
                    colNode.Foreground = Brushes.LightGray;
                    colNode.FontSize = 12;
                    
                    tableNode.Items.Add(colNode);
                }

                treeView.Items.Add(tableNode);
            }
        }
    }
}