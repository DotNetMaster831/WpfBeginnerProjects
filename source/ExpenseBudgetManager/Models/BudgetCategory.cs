using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ExpenseBudgetManager.Models
{
    public class BudgetCategory : INotifyPropertyChanged, IDataErrorInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private decimal _budgetedAmount;
        public decimal BudgetedAmount
        {
            get => _budgetedAmount;
            set { _budgetedAmount = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemainingAmount)); OnPropertyChanged(nameof(ProgressPercent)); }
        }

        private decimal _spentAmount;
        public decimal SpentAmount
        {
            get => _spentAmount;
            set { _spentAmount = value; OnPropertyChanged(); OnPropertyChanged(nameof(RemainingAmount)); OnPropertyChanged(nameof(ProgressPercent)); OnPropertyChanged(nameof(IsOverBudget)); }
        }

        private string _icon = "💰";
        public string Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(); }
        }

        private int _month = DateTime.Today.Month;
        public int Month
        {
            get => _month;
            set { _month = value; OnPropertyChanged(); }
        }

        private int _year = DateTime.Today.Year;
        public int Year
        {
            get => _year;
            set { _year = value; OnPropertyChanged(); }
        }

        // ─────────────────────────────────────
        // Computed
        // ─────────────────────────────────────
        public decimal RemainingAmount => BudgetedAmount - SpentAmount;

        public double ProgressPercent => BudgetedAmount <= 0
            ? 0
            : Math.Min((double)(SpentAmount / BudgetedAmount) * 100, 100);

        public bool IsOverBudget => SpentAmount > BudgetedAmount;

        public string StatusText => IsOverBudget
            ? $"Over by {RemainingAmount * -1:N2}"
            : $"Remaining: {RemainingAmount:N2}";

        // ─────────────────────────────────────
        // IDataErrorInfo
        // ─────────────────────────────────────
        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                return columnName switch
                {
                    nameof(Name) => ValidateName(),
                    nameof(BudgetedAmount) => ValidateBudgetedAmount(),
                    _ => string.Empty
                };
            }
        }

        private string ValidateName()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "Category name is required.";
            if (Name.Trim().Length < 2)
                return "Name must be at least 2 characters.";
            return string.Empty;
        }

        private string ValidateBudgetedAmount()
        {
            if (BudgetedAmount <= 0)
                return "Budget amount must be greater than zero.";
            return string.Empty;
        }

        public bool IsValid =>
            string.IsNullOrEmpty(ValidateName()) &&
            string.IsNullOrEmpty(ValidateBudgetedAmount());

        // ─────────────────────────────────────
        // INotifyPropertyChanged
        // ─────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));
    }
}
