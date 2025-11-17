// File: Utils/Converters.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SqlIdeProject.Utils
{
    // Цей клас перетворює true -> "Успіх" і false -> "Помилка"
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Успіх" : "Помилка";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Нам не потрібна зворотна конвертація
            throw new NotImplementedException();
        }
    }

    // Цей клас перетворює true -> Зелений колір і false -> Червоний колір
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Brushes.Green : Brushes.Red;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}