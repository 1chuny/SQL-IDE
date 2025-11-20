// File: QueryHistoryWindow.xaml.cs
using SqlIdeProject.DataAccess.Repositories;
using System.Windows;

namespace SqlIdeProject
{
    public partial class QueryHistoryWindow : Window
    {
    
        private readonly IQueryHistoryRepository _historyRepository;

        public QueryHistoryWindow(IQueryHistoryRepository historyRepository)
        {
            InitializeComponent();
            _historyRepository = historyRepository;
        }

        private void LoadHistory()
        {
            try
            {
                // 1. Отримуємо ВСІ записи з репозиторію
                var historyEntries = _historyRepository.GetAll();
                // 2. Встановлюємо їх як джерело даних для нашого списку
                HistoryListView.ItemsSource = historyEntries;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Не вдалося завантажити історію: {ex.Message}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHistory();
        }
    }
}
