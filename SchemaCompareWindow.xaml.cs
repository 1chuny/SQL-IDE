using System;
using System.Windows;
using SqlIdeProject.DataAccess.Repositories;
using SqlIdeProject.Models.Internal;
using SqlIdeProject.Services;
using SqlIdeProject.Utils;

namespace SqlIdeProject
{
    public partial class SchemaCompareWindow : Window
    {
        private readonly IConnectionProfileRepository _profileRepository;
        private readonly ConnectionService _connectionService;

        public SchemaCompareWindow()
        {
            InitializeComponent();
            
            // Отримуємо доступ до сервісів
            _profileRepository = new DataAccess.Repositories.Sqlite.SqliteConnectionProfileRepository();
            _connectionService = ConnectionService.Instance;

            LoadProfiles();
        }

        private void LoadProfiles()
        {
            try
            {
                var profiles = _profileRepository.GetAll();
                ProfileCombo1.ItemsSource = profiles;
                ProfileCombo2.ItemsSource = profiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження профілів: " + ex.Message);
            }
        }

        private void CompareBtn_Click(object sender, RoutedEventArgs e)
        {
            var p1 = ProfileCombo1.SelectedItem as ConnectionProfile;
            var p2 = ProfileCombo2.SelectedItem as ConnectionProfile;

            if (p1 == null || p2 == null)
            {
                MessageBox.Show("Будь ласка, оберіть обох користувачів для порівняння.");
                return;
            }

            try
            {
                // 1. Підключаємося до першого і беремо схему
                var manager1 = _connectionService.GetOrCreateConnection("compare_temp_1", p1.DatabaseType, p1.ConnectionString);
                var schema1 = manager1.GetSchema();
                SchemaTreeBuilder.BuildTree(Tree1, schema1);

                // 2. Підключаємося до другого і беремо схему
                var manager2 = _connectionService.GetOrCreateConnection("compare_temp_2", p2.DatabaseType, p2.ConnectionString);
                var schema2 = manager2.GetSchema();
                SchemaTreeBuilder.BuildTree(Tree2, schema2);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час отримання схем: {ex.Message}\nПеревірте, чи доступні обидві бази даних.");
            }
        }
    }
}