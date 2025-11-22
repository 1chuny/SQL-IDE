using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SqlIdeProject.Utils
{
    public static class ThemeHelper
    {
        public static void ApplyTheme(MainWindow window, string themeName)
        {
            // Визначаємо кольори
            var darkBg = new SolidColorBrush(Color.FromRgb(45, 45, 48));
            var darkInputBg = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            var lightFg = Brushes.Gainsboro;
            var lightFgTitle = Brushes.White;

            if (themeName == "Dark")
            {
                // 1. Головне вікно
                window.AppWindow.Background = darkBg;
                window.StatusTextBlock.Foreground = lightFg;
                window.ConnectionBorder.BorderBrush = Brushes.DarkGray;

                // 2. Текстові написи (Labels)
                window.ConnectionTitle.Foreground = lightFgTitle;
                window.ProfileLoadLabel.Foreground = lightFg;
                window.DbTypeLabel.Foreground = lightFg;
                window.ConnectionStringLabel.Foreground = lightFg;
                window.FontSizeLabel.Foreground = lightFg;
                window.ThemeLabel.Foreground = lightFg;
                window.QueryEditorLabel.Foreground = lightFgTitle;

                // 3. Перемикачі
                window.LightThemeRadioButton.Foreground = lightFg;
                window.DarkThemeRadioButton.Foreground = lightFg;

                // 4. Поля вводу
                window.ConnectionStringTextBox.Background = darkInputBg;
                window.ConnectionStringTextBox.Foreground = lightFg;
                window.ProfileNameTextBox.Background = darkInputBg;
                window.ProfileNameTextBox.Foreground = lightFg;
                window.EditorFontSizeTextBox.Background = darkInputBg;
                window.EditorFontSizeTextBox.Foreground = lightFg;

                // 5. Списки
                window.ProfileComboBox.Background = darkInputBg;
                window.DbTypeComboBox.Background = darkInputBg;

                // 6. Редактор коду
                window.QueryTextEditor.Background = darkInputBg;
                window.QueryTextEditor.Foreground = lightFg;

                // 7. Таблиця результатів
                window.ResultsDataGrid.Background = darkInputBg;
                window.ResultsDataGrid.Foreground = lightFg;
                window.ResultsDataGrid.RowBackground = darkInputBg;

                // Стиль для заголовків DataGrid
                var headerStyle = new Style(typeof(DataGridColumnHeader));
                headerStyle.Setters.Add(new Setter(Control.BackgroundProperty, darkBg));
                headerStyle.Setters.Add(new Setter(Control.ForegroundProperty, lightFgTitle));
                window.ResultsDataGrid.ColumnHeaderStyle = headerStyle;

                // Стиль для рядків DataGrid
                var rowStyle = new Style(typeof(DataGridRow));
                rowStyle.Setters.Add(new Setter(Control.BackgroundProperty, darkInputBg));
                rowStyle.Setters.Add(new Setter(Control.ForegroundProperty, lightFg));
                window.ResultsDataGrid.RowStyle = rowStyle;
            }
            else // "Light"
            {
                // Повертаємо все до стандартних кольорів
                window.AppWindow.Background = Brushes.White;
                window.StatusTextBlock.Foreground = Brushes.Black;
                window.ConnectionBorder.BorderBrush = Brushes.LightGray;

                // Метки
                window.ConnectionTitle.Foreground = Brushes.Black;
                window.ProfileLoadLabel.Foreground = Brushes.Black;
                window.DbTypeLabel.Foreground = Brushes.Black;
                window.ConnectionStringLabel.Foreground = Brushes.Black;
                window.FontSizeLabel.Foreground = Brushes.Black;
                window.ThemeLabel.Foreground = Brushes.Black;
                window.QueryEditorLabel.Foreground = Brushes.Black;

                // Перемикачі
                window.LightThemeRadioButton.Foreground = Brushes.Black;
                window.DarkThemeRadioButton.Foreground = Brushes.Black;

                // Поля вводу
                window.ConnectionStringTextBox.Background = Brushes.White;
                window.ConnectionStringTextBox.Foreground = Brushes.Black;
                window.ProfileNameTextBox.Background = Brushes.White;
                window.ProfileNameTextBox.Foreground = Brushes.Black;
                window.EditorFontSizeTextBox.Background = Brushes.White;
                window.EditorFontSizeTextBox.Foreground = Brushes.Black;

                // Списки
                window.ProfileComboBox.Background = Brushes.White;
                window.DbTypeComboBox.Background = Brushes.White;

                // Редактор
                window.QueryTextEditor.Background = Brushes.White;
                window.QueryTextEditor.Foreground = Brushes.Black;

                // Таблиця
                window.ResultsDataGrid.Background = Brushes.White;
                window.ResultsDataGrid.Foreground = Brushes.Black;
                window.ResultsDataGrid.RowBackground = Brushes.White;
                
                window.ResultsDataGrid.ColumnHeaderStyle = null;
                window.ResultsDataGrid.RowStyle = null;
            }
        }
    }
}