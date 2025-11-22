using SqlIdeProject.Commands;
using SqlIdeProject.Patterns.Visitor;
using SqlIdeProject.Services;
using SqlIdeProject.Utils;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; 
using System.Windows.Media;
using SqlIdeProject.DataAccess.Repositories;
using SqlIdeProject.DataAccess.Repositories.Sqlite;
using SqlIdeProject.Models.Internal;


namespace SqlIdeProject
{
    public partial class MainWindow : Window
    {
        
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

				try
            {
                SqlHighlightingHelper.Register();
                var highlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("CustomSQL");
                
                if (highlighting != null)
                {
                    QueryTextEditor.SyntaxHighlighting = highlighting;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження підсвітки синтаксису: {ex.Message}", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
				
            _connectionService = ConnectionService.Instance;
            _queryExecutionService = new QueryExecutionService();
            _profileRepository = new SqliteConnectionProfileRepository();
            _historyRepository = new SqliteQueryHistoryRepository();
            _settingsRepository = new SqliteUserSettingsRepository();
            
            _queryExecutionService.QueryCompleted += OnQueryExecutionCompleted;
            
            LoadProfiles();
            LoadSettings(); 
        }

        
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
                ThemeHelper.ApplyTheme(this, themeName);
                
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
        
        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_settingsRepository == null) return; 

            if (sender is RadioButton rb && rb.IsChecked == true)
            {
                string themeName = rb.Tag.ToString() ?? "Light";
                ThemeHelper.ApplyTheme(this, themeName);
                
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
                ThemeHelper.ApplyTheme(this, DarkThemeRadioButton.IsChecked == true ? "Dark" : "Light");
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
            
            ThemeHelper.ApplyTheme(this, DarkThemeRadioButton.IsChecked == true ? "Dark" : "Light");
            
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
                // Отримуємо поточне з'єднання
                var connectionManager = _connectionService.GetOrCreateConnection(ActiveConnectionId, "", "");
                
                // Отримуємо схему
                var schema = connectionManager.GetSchema();
                
                // ВІДКРИВАЄМО НОВЕ ВІКНО З ДЕРЕВОМ
                var viewer = new SchemaViewerWindow(schema);
                viewer.Owner = this;
                viewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка отримання схеми: {ex.Message}. Переконайтесь, що ви підключені.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompareSchemasButton_Click(object sender, RoutedEventArgs e)
        {
            // Відкриваємо вікно, де користувач сам вибере, що порівнювати
            var compareWindow = new SchemaCompareWindow();
            compareWindow.Owner = this;
            compareWindow.ShowDialog();
        }

    }
}
