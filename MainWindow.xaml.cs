using SqlIdeProject.Commands;
using SqlIdeProject.Services;
using SqlIdeProject.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SqlIdeProject.DataAccess.Repositories;
using SqlIdeProject.DataAccess.Repositories.Sqlite;
using SqlIdeProject.Models.Internal;

namespace SqlIdeProject
{
    /// <summary>
    /// Головне вікно додатку. Відповідає за взаємодію з користувачем (UI Layer).
    /// Реалізує роль Клієнта для патернів Singleton, Command та Observer.
    /// </summary>
    public partial class MainWindow : Window
    {
        // --- СЕРВІСИ ТА МЕНЕДЖЕРИ ---
        private readonly ConnectionService _connectionService; // Singleton для керування з'єднаннями
        private readonly QueryExecutionService _queryExecutionService; // Сервіс виконання команд (Observer Publisher)
        private const string ActiveConnectionId = "main_active_connection"; // ID для поточного активного з'єднання
        
        // --- РЕПОЗИТОРІЇ (Патерн Repository) ---
        private readonly IConnectionProfileRepository _profileRepository; // Доступ до збережених профілів
        private readonly IQueryHistoryRepository _historyRepository;    // Доступ до історії запитів
        private readonly IUserSettingsRepository _settingsRepository;   // Доступ до налаштувань (тема, шрифт)

        // --- СТАН UI ---
        private ConnectionProfile? _activeProfile; // Зберігає профіль, який зараз використовується
        
        // --- КЛЮЧІ НАЛАШТУВАНЬ ---
        private const string ThemeSettingKey = "AppTheme";
        private const string FontSizeSettingKey = "EditorFontSize";


        public MainWindow()
        {
            // 1. Ініціалізація внутрішньої бази даних (SQLite) для налаштувань
            InternalDatabaseService.InitializeDatabase();
            
            InitializeComponent();

            // 2. Налаштування підсвітки синтаксису SQL для редактора AvalonEdit
            SqlHighlightingHelper.Register();
            QueryTextEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("CustomSQL");

            // 3. Ініціалізація сервісів (Singleton) та бізнес-логіки
            _connectionService = ConnectionService.Instance;
            _queryExecutionService = new QueryExecutionService();
            
            // 4. Ініціалізація репозиторіїв
            _profileRepository = new SqliteConnectionProfileRepository();
            _historyRepository = new SqliteQueryHistoryRepository();
            _settingsRepository = new SqliteUserSettingsRepository();
            
            // 5. Підписка на подію завершення запиту (Патерн Observer)
            _queryExecutionService.QueryCompleted += OnQueryExecutionCompleted;
            
            // 6. Завантаження початкових даних в UI
            LoadProfiles();
            LoadSettings(); 
        }

        // ============================================================
        //      РОЗДІЛ: НАЛАШТУВАННЯ (ТЕМА ТА ШРИФТ)
        // ============================================================
        
        /// <summary>
        /// Завантажує збережені налаштування з БД та застосовує їх до інтерфейсу.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // Завантаження розміру шрифту
                string? fontSize = _settingsRepository.GetSetting(FontSizeSettingKey);
                if (!string.IsNullOrEmpty(fontSize) && double.TryParse(fontSize, out double size))
                {
                    QueryTextEditor.FontSize = size;
                    EditorFontSizeTextBox.Text = fontSize;
                }
                
                // Завантаження теми
                string? theme = _settingsRepository.GetSetting(ThemeSettingKey);
                string themeName = theme ?? "Light"; // За замовчуванням світла
                
                // Використання хелпера для застосування стилів
                ThemeHelper.ApplyTheme(this, themeName); 
                
                // Оновлення стану перемикачів
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
        
        /// <summary>
        /// Обробник кнопки "Зберегти налаштування".
        /// </summary>
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fontSizeString = EditorFontSizeTextBox.Text;
                if (double.TryParse(fontSizeString, out double size) && size > 5 && size < 40)
                {
                    // Зберігаємо в БД через репозиторій
                    _settingsRepository.SaveSetting(FontSizeSettingKey, fontSizeString);
                    // Застосовуємо зміни одразу
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
        
        /// <summary>
        /// Обробник зміни перемикача теми. Миттєво застосовує та зберігає нову тему.
        /// </summary>
        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_settingsRepository == null) return; // Захист від виклику під час ініціалізації

            if (sender is RadioButton rb && rb.IsChecked == true)
            {
                string themeName = rb.Tag.ToString() ?? "Light";
                
                // Застосовуємо тему через допоміжний клас
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

        // ============================================================
        //      РОЗДІЛ: ПРОФІЛІ ПІДКЛЮЧЕННЯ
        // ============================================================
        
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
                
                // Валідація
                if (string.IsNullOrWhiteSpace(profileName) || profileName == "Назва профілю...")
                {
                    MessageBox.Show("Будь ласка, введіть коректну назву профілю.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Перевірка на унікальність
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
                LoadProfiles(); // Оновлюємо список у ComboBox
                ProfileNameTextBox.Text = "Назва профілю...";
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Помилка збереження профілю: {ex.Message}");
            }
        }

        /// <summary>
        /// Автоматичне заповнення полів при виборі профілю зі списку.
        /// </summary>
        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileComboBox.SelectedItem is ConnectionProfile selectedProfile)
            {
                ConnectionStringTextBox.Text = selectedProfile.ConnectionString;
                ProfileNameTextBox.Text = selectedProfile.ProfileName;
                
                // Встановлення правильного типу БД у ComboBox
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
        
        // ============================================================
        //      РОЗДІЛ: ОСНОВНІ ФУНКЦІЇ IDE
        // ============================================================
        
        /// <summary>
        /// Встановлює з'єднання з базою даних через сервіс.
        /// </summary>
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

            // Якщо параметри змінені вручну, скидаємо активний профіль
            if (ProfileComboBox.SelectedItem == null || ((ConnectionProfile)ProfileComboBox.SelectedItem).ConnectionString != connectionString)
            {
                _activeProfile = null; 
            }

            try
            {
                // 1. Відключаємо старе з'єднання
                _connectionService.Disconnect(ActiveConnectionId);

                // 2. Створюємо нове з'єднання через Singleton
                _connectionService.GetOrCreateConnection(ActiveConnectionId, dbType, connectionString);
                
                IdeGrid.IsEnabled = true; // Розблоковуємо редактор
                StatusTextBlock.Text = $"Підключено до {dbType}. Готовий.";
                
                // Оновлюємо тему (щоб статус-бар мав правильний колір)
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

        /// <summary>
        /// Створює команду та передає її на виконання. (Патерн Command)
        /// </summary>
        private void ExecuteQueryButton_Click(object sender, RoutedEventArgs e)
        {
            string query = QueryTextEditor.Text;
            if (string.IsNullOrWhiteSpace(query)) return;
            try
            {
                var connectionManager = _connectionService.GetOrCreateConnection(ActiveConnectionId, "", "");
                
                // Створюємо об'єкт команди
                var command = new ExecuteQueryCommand(connectionManager, query);
                
                StatusTextBlock.Text = "Виконується запит...";
                
                // Передаємо виконавцю
                _queryExecutionService.Submit(command);
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Помилка: {ex.Message}. Можливо, ви не підключені до бази даних.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Обробник події завершення запиту (Патерн Observer).
        /// Оновлює UI та зберігає історію.
        /// </summary>
        private void OnQueryExecutionCompleted(object? sender, QueryCompletedEventArgs e)
        {
            // 1. Оновлення UI
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
            
            // 2. Збереження історії
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
            // Відкриваємо вікно історії
            QueryHistoryWindow historyWindow = new QueryHistoryWindow(_historyRepository);
            historyWindow.Owner = this; 
            historyWindow.ShowDialog();
        }

        /// <summary>
        /// Відкриває вікно для візуалізації схеми (Патерн Visitor / TreeView).
        /// </summary>
        private void ShowSchemaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionManager = _connectionService.GetOrCreateConnection(ActiveConnectionId, "", "");
                var schema = connectionManager.GetSchema();
                
                var viewer = new SchemaViewerWindow(schema);
                viewer.Owner = this;
                viewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка отримання схеми: {ex.Message}. Переконайтесь, що ви підключені.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Відкриває вікно для порівняння двох схем.
        /// </summary>
        private void CompareSchemasButton_Click(object sender, RoutedEventArgs e)
        {
            var compareWindow = new SchemaCompareWindow();
            compareWindow.Owner = this;
            compareWindow.ShowDialog();
        }
    }
}