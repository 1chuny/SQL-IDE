using SqlIdeProject.Commands;
using SqlIdeProject.Patterns.Visitor;
using SqlIdeProject.Services;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SqlIdeProject
{
    public partial class MainWindow : Window
    {
        private readonly ConnectionService _connectionService;
        private readonly QueryExecutionService _queryExecutionService;
        
        // Унікальний ідентифікатор для нашого основного активного підключення
        private const string ActiveConnectionId = "main_active_connection";

        public MainWindow()
        {
            InitializeComponent();
            _connectionService = ConnectionService.Instance;
            _queryExecutionService = new QueryExecutionService();

            // Підписуємо UI на події від сервісу виконання запитів
            _queryExecutionService.QueryCompleted += OnQueryExecutionCompleted;
        }

        // Обробник для кнопки "Підключитись"
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDbItem = DbTypeComboBox.SelectedItem as ComboBoxItem;
            if (selectedDbItem == null) return;

            string dbType = selectedDbItem.Content.ToString()!;
            string connectionString = ConnectionStringTextBox.Text;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Рядок підключення не може бути порожнім.", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Використовуємо сервіс для створення (або пере-створення) підключення
                _connectionService.GetOrCreateConnection(ActiveConnectionId, dbType, connectionString);
                
                // Робимо основну частину IDE активною
                IdeGrid.IsEnabled = true; 
                StatusTextBlock.Text = $"Підключено до {dbType}. Готовий.";
                StatusTextBlock.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                IdeGrid.IsEnabled = false;
                StatusTextBlock.Text = "Помилка підключення.";
                StatusTextBlock.Foreground = Brushes.Red;
            }
        }

        // Обробник для кнопки "Виконати запит (SELECT)"
        private void ExecuteQueryButton_Click(object sender, RoutedEventArgs e)
        {
            string query = QueryTextEditor.Text;
            if (string.IsNullOrWhiteSpace(query)) return;

            try
            {
                // Отримуємо вже існуюче активне підключення
                var connectionManager = _connectionService.GetOrCreateConnection(ActiveConnectionId, "", "");
                var command = new ExecuteQueryCommand(connectionManager, query);
                
                StatusTextBlock.Text = "Виконується запит...";
                _queryExecutionService.Submit(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}. Можливо, ви не підключені до бази даних.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод, що реагує на завершення виконання запиту (патерн Observer)
        private void OnQueryExecutionCompleted(object? sender, QueryCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                ResultsDataGrid.ItemsSource = e.Result?.DefaultView;
                StatusTextBlock.Text = $"Успішно. Отримано рядків: {e.Result?.Rows.Count ?? 0}";
                StatusTextBlock.Foreground = Brushes.Green;
            }
            else
            {
                MessageBox.Show($"Помилка виконання запиту: {e.Error?.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Помилка виконання";
                StatusTextBlock.Foreground = Brushes.Red;
            }
        }

        // Обробник для кнопки "Показати схему"
        private void ShowSchemaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionManager = _connectionService.GetOrCreateConnection(ActiveConnectionId, "", "");
                var schema = connectionManager.GetSchema();
                
                var visitor = new SchemaTextRendererVisitor();
                schema.Accept(visitor);
                
                MessageBox.Show(visitor.GetResult(), "Схема Бази Даних");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка отримання схеми: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробник для кнопки "Порівняти схеми (Тест)"
        private void CompareSchemasButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var (schema1, schema2) = CreateTestSchemasForComparison();
                var visitor = new SchemaComparisonVisitor(schema2);
                schema1.Accept(visitor);
                
                var resultText = new StringBuilder();
                resultText.AppendLine("Результати порівняння:");
                if (visitor.Differences.Count <= 1)
                {
                    resultText.AppendLine("Відмінностей не знайдено.");
                }
                else
                {
                    visitor.Differences.ForEach(d => resultText.AppendLine(d));
                }
                MessageBox.Show(resultText.ToString(), "Порівняння Схем");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка порівняння схем: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Допоміжний метод для створення тестових БД для порівняння
        private (Models.Schema.SchemaRoot, Models.Schema.SchemaRoot) CreateTestSchemasForComparison()
        {
            // Схема 1 (Еталон) - використовуємо SQLite
            var conn1 = _connectionService.GetOrCreateConnection("db1_compare", "SQLite", "Data Source=db1.sqlite");
            conn1.RunCommand("DROP TABLE IF EXISTS Products;");
            conn1.RunCommand("DROP TABLE IF EXISTS Customers;");
            conn1.RunCommand("CREATE TABLE Products (Id INT, Name TEXT, Price REAL);");
            conn1.RunCommand("CREATE TABLE Customers (Id INT, FullName TEXT);");
            var schema1 = conn1.GetSchema();

            // Схема 2 (З відмінностями) - використовуємо SQLite
            var conn2 = _connectionService.GetOrCreateConnection("db2_compare", "SQLite", "Data Source=db2.sqlite");
            conn2.RunCommand("DROP TABLE IF EXISTS Products;");
            conn2.RunCommand("DROP TABLE IF EXISTS Orders;");
            conn2.RunCommand("CREATE TABLE Products (Id INTEGER, Name VARCHAR, Quantity INT);");
            conn2.RunCommand("CREATE TABLE Orders (OrderId INT);");
            var schema2 = conn2.GetSchema();

            return (schema1, schema2);
        }
    }
}