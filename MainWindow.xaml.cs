using SqlIdeProject.Commands;
using SqlIdeProject.Patterns.Visitor;
using SqlIdeProject.Services;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; // Потрібно для DataGridColumnHeader
using System.Windows.Media;
using SqlIdeProject.DataAccess.Repositories;
using SqlIdeProject.DataAccess.Repositories.Sqlite;
using SqlIdeProject.Models.Internal;

namespace SqlIdeProject
{
    public partial class MainWindow : Window
    {
        // --- (всі ваші поля: _connectionService, _profileRepository тощо) ---
        private readonly ConnectionService _connectionService;
        private readonly QueryExecutionService _queryExecutionService;
        private const string ActiveConnectionId = "main_active_connection";
        private readonly IConnectionProfileRepository _profileRepository;
        private readonly IQueryHistoryRepository _historyRepository;
        private readonly IUserSettingsRepository _settingsRepository; 
        private ConnectionProfile? _activeProfile;
        private const string ThemeSettingKey = "AppTheme";
        private const string FontSizeSettingKey = "EditorFontSize";


        public MainWindow()
        {
            InternalDatabaseService.InitializeDatabase();
            InitializeComponent();
            
            _connectionService = ConnectionService.Instance;
            _queryExecutionService = new QueryExecutionService();
            _profileRepository = new SqliteConnectionProfileRepository();
            _historyRepository = new SqliteQueryHistoryRepository();
            _settingsRepository = new SqliteUserSettingsRepository();
            
            _queryExecutionService.QueryCompleted += OnQueryExecutionCompleted;
            
            LoadProfiles();
            LoadSettings(); 
        }

        // --- МЕТОДИ ДЛЯ НАЛАШТУВАНЬ (ШРИФТ + ТЕМА) ---
        
        private void LoadSettings()
        {
            try
            {
                string? fontSize = _settingsRepository.GetSetting(FontSizeSettingKey);
                if (!string.IsNullOrEmpty(fontSize) && double.TryParse(fontSize, out double size))
                {
                    QueryTextEditor.FontSize = size;
                    EditorFontSizeTextBox.Text = fontSize;
                }
                
                string? theme = _settingsRepository.GetSetting(ThemeSettingKey);
                string themeName = theme ?? "Light";
                ApplyTheme(themeName); 
                
                if (themeName == "Dark")
                    DarkThemeRadioButton.IsChecked = true;
                else
                    LightThemeRadioButton.IsChecked = true;
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Помилка завантаження налаштувань: {ex.Message}");
            }
        }
        
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fontSizeString = EditorFontSizeTextBox.Text;
                if (double.TryParse(fontSizeString, out double size) && size > 5 && size < 40)
                {
                    _settingsRepository.SaveSetting(FontSizeSettingKey, fontSizeString);
                    QueryTextEditor.FontSize = size;
                    MessageBox.Show("Налаштування збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Будь ласка, введіть коректний розмір шрифту (наприклад, 14).", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Помилка збереження налаштувань: {ex.Message}");
            }
        }
        
        // --- ОСЬ ВЕЛИКЕ ОНОВЛЕННЯ ---
        private void ApplyTheme(string themeName)
        {
            // Визначаємо кольори
            var darkBg = new SolidColorBrush(Color.FromRgb(45, 45, 48));
            var darkInputBg = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            var lightFg = Brushes.Gainsboro;
            var lightFgTitle = Brushes.White;

            if (themeName == "Dark")
            {
                // Вікно та Статус
                AppWindow.Background = darkBg;
                StatusTextBlock.Foreground = lightFg;
                
                // Рамка
                ConnectionBorder.BorderBrush = Brushes.DarkGray;

                // Усі Текстові Метки (TextBlock)
                ConnectionTitle.Foreground = lightFgTitle;
                ProfileLoadLabel.Foreground = lightFg;
                DbTypeLabel.Foreground = lightFg;
                ConnectionStringLabel.Foreground = lightFg;
                FontSizeLabel.Foreground = lightFg;
                ThemeLabel.Foreground = lightFg;
                QueryEditorLabel.Foreground = lightFgTitle;
                
                // Перемикачі (RadioButton)
                LightThemeRadioButton.Foreground = lightFg;
                DarkThemeRadioButton.Foreground = lightFg;

                // Поля вводу (TextBox)
                ConnectionStringTextBox.Background = darkInputBg;
                ConnectionStringTextBox.Foreground = lightFg;
                ProfileNameTextBox.Background = darkInputBg;
                ProfileNameTextBox.Foreground = lightFg;
                EditorFontSizeTextBox.Background = darkInputBg;
                EditorFontSizeTextBox.Foreground = lightFg;

                // Списки (ComboBox)
                ProfileComboBox.Background = darkInputBg;
                DbTypeComboBox.Background = darkInputBg;

                // Редактор коду AvalonEdit
                QueryTextEditor.Background = darkInputBg;
                QueryTextEditor.Foreground = lightFg;

                // Таблиця результатів DataGrid
                ResultsDataGrid.Background = darkInputBg;
                ResultsDataGrid.Foreground = lightFg;
                ResultsDataGrid.RowBackground = darkInputBg;
                
                // Стиль для заголовків DataGrid
                var headerStyle = new Style(typeof(DataGridColumnHeader));
                headerStyle.Setters.Add(new Setter(BackgroundProperty, darkBg));
                headerStyle.Setters.Add(new Setter(ForegroundProperty, lightFgTitle));
                ResultsDataGrid.ColumnHeaderStyle = headerStyle;
                
                // Стиль для рядків DataGrid
                var rowStyle = new Style(typeof(DataGridRow));
                rowStyle.Setters.Add(new Setter(BackgroundProperty, darkInputBg));
                rowStyle.Setters.Add(new Setter(ForegroundProperty, lightFg));
                ResultsDataGrid.RowStyle = rowStyle;
            }
            else // "Light"
            {
                // Повертаємо все до стандартних кольорів
                AppWindow.Background = Brushes.White;
                StatusTextBlock.Foreground = Brushes.Black;
                ConnectionBorder.BorderBrush = Brushes.LightGray;

                // Метки
                ConnectionTitle.Foreground = Brushes.Black;
                ProfileLoadLabel.Foreground = Brushes.Black;
                DbTypeLabel.Foreground = Brushes.Black;
                ConnectionStringLabel.Foreground = Brushes.Black;
                FontSizeLabel.Foreground = Brushes.Black;
                ThemeLabel.Foreground = Brushes.Black;
                QueryEditorLabel.Foreground = Brushes.Black;
                
                // Перемикачі
                LightThemeRadioButton.Foreground = Brushes.Black;
                DarkThemeRadioButton.Foreground = Brushes.Black;

                // Поля вводу
                ConnectionStringTextBox.Background = Brushes.White;
                ConnectionStringTextBox.Foreground = Brushes.Black;
                ProfileNameTextBox.Background = Brushes.White;
                ProfileNameTextBox.Foreground = Brushes.Black;
                EditorFontSizeTextBox.Background = Brushes.White;
                EditorFontSizeTextBox.Foreground = Brushes.Black;

                // Списки
                ProfileComboBox.Background = Brushes.White;
                DbTypeComboBox.Background = Brushes.White;

                // Редактор
                QueryTextEditor.Background = Brushes.White;
                QueryTextEditor.Foreground = Brushes.Black;
                
                // Таблиця
                ResultsDataGrid.Background = Brushes.White;
                ResultsDataGrid.Foreground = Brushes.Black;
                ResultsDataGrid.RowBackground = Brushes.White;
                ResultsDataGrid.ColumnHeaderStyle = null; // Скинути до дефолту
                ResultsDataGrid.RowStyle = null; // Скинути до дефолту
            }
        }
        
        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_settingsRepository == null) return; 

            if (sender is RadioButton rb && rb.IsChecked == true)
            {
                string themeName = rb.Tag.ToString() ?? "Light";
                ApplyTheme(themeName);
                
                try
                {
                    _settingsRepository.SaveSetting(ThemeSettingKey, themeName);
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"Помилка збереження теми: {ex.Message}";
                }
            }
        }

        // --- (Решта коду (LoadProfiles, SaveProfileButton_Click, ... аж до кінця) залишається без змін) ---
        
        private void LoadProfiles()
        {
            try
            {
                var profiles = _profileRepository.GetAll();
                ProfileComboBox.ItemsSource = profiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження профілів: {ex.Message}");
            }
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string profileName = ProfileNameTextBox.Text;
                
                if (string.IsNullOrWhiteSpace(profileName) || profileName == "Назва профілю...")
                {
                    MessageBox.Show("Будь ласка, введіть коректну назву профілю.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var existingProfile = _profileRepository.GetByName(profileName);
                if (existingProfile != null)
                {
                    MessageBox.Show("Профіль з такою назвою вже існує. Будь ласка, оберіть іншу.", "Дублікат", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; 
                }
                
                var newProfile = new ConnectionProfile
                {
                    ProfileName = profileName,
                    DatabaseType = ((ComboBoxItem)DbTypeComboBox.SelectedItem).Content.ToString()!, 
                    ConnectionString = ConnectionStringTextBox.Text
                };

                _profileRepository.Add(newProfile);
                
                MessageBox.Show("Профіль збережено!");
                LoadProfiles(); 
                ProfileNameTextBox.Text = "Назва профілю...";
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Помилка збереження профілю: {ex.Message}");
            }
        }

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileComboBox.SelectedItem is ConnectionProfile selectedProfile)
            {
                ConnectionStringTextBox.Text = selectedProfile.ConnectionString;
                ProfileNameTextBox.Text = selectedProfile.ProfileName;
                
                foreach (ComboBoxItem item in DbTypeComboBox.Items)
                {
                    if (item.Content?.ToString() == selectedProfile.DatabaseType)
                    {
                        DbTypeComboBox.SelectedItem = item;
                        break;
                    }
                }
                _activeProfile = selectedProfile;
            }
        }
        
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDbItem = DbTypeComboBox.SelectedItem as ComboBoxItem;
            if (selectedDbItem == null) return;
            string dbType = selectedDbItem.Content?.ToString() ?? "SQLite";
            string connectionString = ConnectionStringTextBox.Text;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Рядок підключення не може бути порожнім.", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProfileComboBox.SelectedItem == null || ((ConnectionProfile)ProfileComboBox.SelectedItem).ConnectionString != connectionString)
            {
                _activeProfile = null; 
            }

            try
            {
                _connectionService.GetOrCreateConnection(ActiveConnectionId, dbType, connectionString);
                IdeGrid.IsEnabled = true; 
                StatusTextBlock.Text = $"Підключено до {dbType}. Готовий.";
                // Застосовуємо колір до статус-бару при підключенні
                ApplyTheme(DarkThemeRadioButton.IsChecked == true ? "Dark" : "Light");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                IdeGrid.IsEnabled = false;
                StatusTextBlock.Text = "Помилка підключення.";
                StatusTextBlock.Foreground = Brushes.Red;
            }
        }

        private void ExecuteQueryButton_Click(object sender, RoutedEventArgs e)
        {
            string query = QueryTextEditor.Text;
            if (string.IsNullOrWhiteSpace(query)) return;
            try
            {
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
        
        private void OnQueryExecutionCompleted(object? sender, QueryCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                ResultsDataGrid.ItemsSource = e.Result?.DefaultView;
                StatusTextBlock.Text = $"Успішно. Отримано рядків: {e.Result?.Rows.Count ?? 0}";
            }
            else
            {
                MessageBox.Show($"Помилка виконання запиту: {e.Error?.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Помилка виконання";
            }
            
            // Встановлюємо колір статус-бару
            ApplyTheme(DarkThemeRadioButton.IsChecked == true ? "Dark" : "Light");
            
            try
            {
                var historyEntry = new QueryHistory
                {
                    QueryText = QueryTextEditor.Text,
                    ExecutionTime = DateTime.Now,
                    WasSuccessful = e.IsSuccess,
                    ConnectionProfileId = _activeProfile?.Id 
                };
                _historyRepository.Add(historyEntry);
            }
            catch(Exception ex)
            {
                StatusTextBlock.Text += $" | Помилка збереження історії: {ex.Message}";
            }
        }
        
        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            QueryHistoryWindow historyWindow = new QueryHistoryWindow(_historyRepository);
            historyWindow.Owner = this; 
            historyWindow.ShowDialog();
        }

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

        private (Models.Schema.SchemaRoot, Models.Schema.SchemaRoot) CreateTestSchemasForComparison()
        {
            var conn1 = _connectionService.GetOrCreateConnection("db1_compare", "SQLite", "Data Source=db1.sqlite");
            conn1.RunCommand("DROP TABLE IF EXISTS Products;");
            conn1.RunCommand("DROP TABLE IF EXISTS Customers;");
            conn1.RunCommand("CREATE TABLE Products (Id INT, Name TEXT, Price REAL);");
            conn1.RunCommand("CREATE TABLE Customers (Id INT, FullName TEXT);");
            var schema1 = conn1.GetSchema();
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