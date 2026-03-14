using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace ExpenseBudgetManager.Converters
{
    public class CurrencyFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                string symbol = parameter?.ToString() ?? "₹";
                return amount >= 0
                    ? $"{symbol}{amount:N2}"
                    : $"-{symbol}{Math.Abs(amount):N2}";
            }
            return "₹0.00";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
