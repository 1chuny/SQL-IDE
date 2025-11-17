// File: QueryHistoryWindow.xaml.cs
using SqlIdeProject.DataAccess.Repositories;
using System.Windows;

namespace SqlIdeProject
{
    public partial class QueryHistoryWindow : Window
    {
        // Вікно має знати, звідки брати дані.
        private readonly IQueryHistoryRepository _historyRepository;

        // "Ін'єкція залежності": Ми вимагаємо, щоб той, хто створює
        // це вікно, передав нам готовий репозиторій.
        public QueryHistoryWindow(IQueryHistoryRepository historyRepository)
        {
            InitializeComponent();
            _historyRepository = historyRepository;
        }

        // Цей метод завантажить дані в ListView
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

        // Цей метод автоматично викличеться, коли вікно завантажиться
        // (ми вказали це у XAML: Loaded="Window_Loaded")
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHistory();
        }
    }
}