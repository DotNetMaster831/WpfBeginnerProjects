using ExpenseBudgetManager.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace ExpenseBudgetManager.Converters
{
    public class TransactionTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is TranscationType type)
            {
                return type == TranscationType.Income
                    ? new SolidColorBrush(Color.FromRgb(0x6A, 0x99, 0x4E))
                    : new SolidColorBrush(Color.FromRgb(0xBC, 0x47, 0x49));
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
