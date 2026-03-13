using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ExpenseBudgetManager.Models
{
    public class AppSettings
    {
        private string _currencySymbol = "₹";
        public string CurrencySymbol
        {
            get => _currencySymbol;
            set { _currencySymbol = value; OnPropertyChanged(); }
        }

        private string _dateFormat = "dd MMM yyyy";
        public string DateFormat
        {
            get => _dateFormat;
            set { _dateFormat = value; OnPropertyChanged(); }
        }

        private string _theme = "Dark";
        public string Theme
        {
            get => _theme;
            set { _theme = value; OnPropertyChanged(); }
        }

        private bool _autoBackup = true;
        public bool AutoBackup
        {
            get => _autoBackup;
            set { _autoBackup = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));
    }
}
