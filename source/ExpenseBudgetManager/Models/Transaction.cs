using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ExpenseBudgetManager.Models
{
    public enum TranscationType
    {
        Income,
        Expense
    }

    public class Transaction : INotifyPropertyChanged, IDataErrorInfo
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        // ─────────────────────────────────────
        // Properties
        // ─────────────────────────────────────
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set 
            { 
                _title = value;
                OnPropertyChanged();
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        private TranscationType _type = TranscationType.Expense;
        public TranscationType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        private string _category = string.Empty;
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        private DateTime _date = DateTime.Today;
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        private string _note = string.Empty;
        public string Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(); }
        }

        // ─────────────────────────────────────
        // Computed helpers
        // ─────────────────────────────────────

        public decimal SignedAmount => Type == TranscationType.Income ? Amount : -Amount;

        public string AmountDisplay => Type == TranscationType.Income ? $"+{Amount:N2}" : $"-{Amount:N2}";

        // ─────────────────────────────────────
        // IDataErrorInfo — Validation
        // ─────────────────────────────────────
        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                return columnName switch
                {
                    nameof(Title) => ValidateTitle(),
                    nameof(Amount) => ValidateAmount(),
                    nameof(Category) => ValidateCategory(),
                    nameof(Date) => ValidateDate(),
                    _ => string.Empty
                };
            }
        }

        private string ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return "Title is required.";
            if(Title.Trim().Length < 2)
                return "Title must be at least 2 characters.";
            if(Title.Trim().Length > 100)
                return "Title cannot exceed 100 characters.";
            return string.Empty;
        }

        private string ValidateAmount()
        {
            if (Amount <= 0)
                return "Amount must be greater than zero";
            if (Amount > 10_000_0000)
                return "Amount seems too large. Please verify.";
            return string.Empty;
        }

        private string ValidateCategory()
        {
            if (string.IsNullOrWhiteSpace(Category))
                return "Please select a category.";
            return string.Empty;
        }

        private string ValidateDate()
        {
            if (Date == default)
                return "Please select a valid date.";
            if (Date > DateTime.Today.AddDays(1))
                return "Date cannot be in the future.";
            if (Date < DateTime.Today.AddYears(-10))
                return "Date is too far in the past.";
            return string.Empty;
        }

        // Checks ALL fields at once — used by CanExecute
        public bool IsValid =>
            string.IsNullOrEmpty(ValidateTitle()) &&
            string.IsNullOrEmpty(ValidateAmount()) &&
            string.IsNullOrEmpty(ValidateCategory()) &&
            string.IsNullOrEmpty(ValidateDate());


        // ─────────────────────────────────────
        // INotifyPropertyChanged
        // ─────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(
            [System.Runtime.CompilerServices.CallerMemberName]
            string? name = null)
            => PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));
    }
}
