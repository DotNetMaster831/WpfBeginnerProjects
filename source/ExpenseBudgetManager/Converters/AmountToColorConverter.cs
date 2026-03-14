using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace ExpenseBudgetManager.Converters
{
    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                return amount >= 0
                    ? new SolidColorBrush(
                        Color.FromRgb(0x6A, 0x99, 0x4E)) // SageGreen
                    : new SolidColorBrush(
                        Color.FromRgb(0xBC, 0x47, 0x49)); // BlushedBrick
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
