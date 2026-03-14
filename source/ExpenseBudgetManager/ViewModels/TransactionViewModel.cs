// ViewModels/TransactionViewModel.cs
using ExpenseBudgetManager.Commands;
using ExpenseBudgetManager.Infrastructure;
using ExpenseBudgetManager.Models;
using ExpenseBudgetManager.Services;
using Serilog.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace ExpenseBudgetManager.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        private readonly ITransactionStore _store;

        // ─────────────────────────────────────
        // Master list + filtered view
        // ─────────────────────────────────────
        private readonly ObservableCollection<Transaction> _allTransactions = new();
        private ICollectionView _transactionsView = null!;
        public ICollectionView TransactionsView
        {
            get => _transactionsView;
            private set => SetProperty(ref _transactionsView, value);
        }

        // ─────────────────────────────────────
        // Filter state
        // ─────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    App.Current.Dispatcher.Invoke(() => TransactionsView?.Refresh());
            }
        }

        public string[] TypeFilters { get; } = { "All", "Income", "Expense" };

        private string _selectedTypeFilter = "All";
        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set
            {
                if (SetProperty(ref _selectedTypeFilter, value))
                    App.Current.Dispatcher.Invoke(() => TransactionsView?.Refresh());
            }
        }

        public ObservableCollection<string> CategoryFilters { get; } = new();

        private string _selectedCategoryFilter = "All";
        public string SelectedCategoryFilter
        {
            get => _selectedCategoryFilter;
            set
            {
                if (SetProperty(ref _selectedCategoryFilter, value))
                    App.Current.Dispatcher.Invoke(() => TransactionsView?.Refresh());
            }
        }

        public ObservableCollection<string> MonthFilters { get; } = new();

        private string _selectedMonthFilter = "All";
        public string SelectedMonthFilter
        {
            get => _selectedMonthFilter;
            set
            {
                if (SetProperty(ref _selectedMonthFilter, value))
                    App.Current.Dispatcher.Invoke(() => TransactionsView?.Refresh());
            }
        }

        // ─────────────────────────────────────
        // Form state
        // ─────────────────────────────────────
        private bool _isFormVisible;
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string FormTitle => IsEditMode ? "Edit Transaction" : "Add Transaction";

        // ─────────────────────────────────────
        // Form fields
        // ─────────────────────────────────────
        private Guid _editingId = Guid.Empty;

        private string _formTitleField = string.Empty;
        public string FormTitleField
        {
            get => _formTitleField;
            set => SetProperty(ref _formTitleField, value);
        }

        private string _formAmount = string.Empty;
        public string FormAmount
        {
            get => _formAmount;
            set => SetProperty(ref _formAmount, value);
        }

        private TranscationType _formType = TranscationType.Expense;
        public TranscationType FormType
        {
            get => _formType;
            set
            {
                if (SetProperty(ref _formType, value))
                    RefreshCategoryOptions();
            }
        }

        public TranscationType[] TypeOptions { get; } =
            { TranscationType.Income, TranscationType.Expense };

        private string _formCategory = string.Empty;
        public string FormCategory
        {
            get => _formCategory;
            set => SetProperty(ref _formCategory, value);
        }

        public ObservableCollection<string> CategoryOptions { get; } = new();

        private DateTime _formDate = DateTime.Today;
        public DateTime FormDate
        {
            get => _formDate;
            set => SetProperty(ref _formDate, value);
        }

        private string _formNote = string.Empty;
        public string FormNote
        {
            get => _formNote;
            set => SetProperty(ref _formNote, value);
        }

        private string _formError = string.Empty;
        public string FormError
        {
            get => _formError;
            set => SetProperty(ref _formError, value);
        }

        // ─────────────────────────────────────
        // Summary
        // ─────────────────────────────────────
        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        private decimal _filteredIncome;
        public decimal FilteredIncome
        {
            get => _filteredIncome;
            set => SetProperty(ref _filteredIncome, value);
        }

        private decimal _filteredExpense;
        public decimal FilteredExpense
        {
            get => _filteredExpense;
            set => SetProperty(ref _filteredExpense, value);
        }

        // ─────────────────────────────────────
        // Commands
        // ─────────────────────────────────────
        public ICommand ShowAddFormCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        // ─────────────────────────────────────
        // Event
        // ─────────────────────────────────────
        public event Action? TransactionsChanged;

        // ─────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────
        public TransactionViewModel()
        {
            _store = ServiceLocator.TransactionStore;

            // MUST build CollectionView on UI thread
            App.Current.Dispatcher.Invoke(() =>
            {
                _transactionsView = CollectionViewSource
                    .GetDefaultView(_allTransactions);
                _transactionsView.Filter = ApplyFilters;
                _transactionsView.SortDescriptions.Add(
                    new SortDescription(
                        nameof(Transaction.Date),
                        ListSortDirection.Descending));
                _transactionsView.CollectionChanged +=
                    (_, _) => UpdateSummary();
            });

            ShowAddFormCommand = new RelayCommad(_ => OpenAddForm());
            SaveCommand = new RelayCommad(_ => SaveTransactionAsync());
            CancelCommand = new RelayCommad(_ => CloseForm());
            EditCommand = new RelayCommad(t => OpenEditForm(t as Transaction));
            DeleteCommand = new RelayCommad(t => DeleteTransactionAsync(t as Transaction));
            ClearFiltersCommand = new RelayCommad(_ => ClearFilters());

            _logger!.LogInformation("TransactionViewModel initialized.");
            LoadAsync();
        }

        // ─────────────────────────────────────
        // Load
        // ─────────────────────────────────────
        private async void LoadAsync()
        {
            try
            {
                var all = await _store.GetAllAsync();

                App.Current.Dispatcher.Invoke(() =>
                {
                    _allTransactions.Clear();
                    foreach (var t in all)
                        _allTransactions.Add(t);

                    BuildFilterLists();
                    _transactionsView.Refresh();
                    UpdateSummary();
                    OnPropertyChanged(nameof(TransactionsView));
                });

                _logger!.LogInformation($"Loaded {all.Count} transactions.");
            }
            catch (Exception ex)
            {
                _logger!.LogError("Failed to load transactions", ex);
            }
        }

        // ─────────────────────────────────────
        // Filter logic
        // ─────────────────────────────────────
        private bool ApplyFilters(object obj)
        {
            if (obj is not Transaction t) return false;

            if (_selectedTypeFilter != "All" &&
                t.Type.ToString() != _selectedTypeFilter)
                return false;

            if (_selectedCategoryFilter != "All" &&
                t.Category != _selectedCategoryFilter)
                return false;

            if (_selectedMonthFilter != "All" &&
                t.Date.ToString("MMM yyyy") != _selectedMonthFilter)
                return false;

            if (!string.IsNullOrWhiteSpace(_searchText) &&
                !t.Title.Contains(_searchText,
                    StringComparison.OrdinalIgnoreCase) &&
                !t.Category.Contains(_searchText,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        private void BuildFilterLists()
        {
            CategoryFilters.Clear();
            CategoryFilters.Add("All");
            foreach (var c in _allTransactions
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(c => c))
                CategoryFilters.Add(c);

            MonthFilters.Clear();
            MonthFilters.Add("All");
            foreach (var m in _allTransactions
                .Select(t => t.Date.ToString("MMM yyyy"))
                .Distinct()
                .OrderByDescending(m => m))
                MonthFilters.Add(m);
        }

        private void UpdateSummary()
        {
            var visible = _transactionsView
                .Cast<Transaction>().ToList();
            TotalCount = visible.Count;
            FilteredIncome = visible
                .Where(t => t.Type == TranscationType.Income)
                .Sum(t => t.Amount);
            FilteredExpense = visible
                .Where(t => t.Type == TranscationType.Expense)
                .Sum(t => t.Amount);
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedTypeFilter = "All";
            SelectedCategoryFilter = "All";
            SelectedMonthFilter = "All";
        }

        // ─────────────────────────────────────
        // Form open / close
        // ─────────────────────────────────────
        private void OpenAddForm()
        {
            IsEditMode = false;
            _editingId = Guid.Empty;
            FormTitleField = string.Empty;
            FormAmount = string.Empty;
            FormType = TranscationType.Expense;
            FormDate = DateTime.Today;
            FormNote = string.Empty;
            FormError = string.Empty;
            RefreshCategoryOptions();
            IsFormVisible = true;
            OnPropertyChanged(nameof(FormTitle));
        }

        private void OpenEditForm(Transaction? t)
        {
            if (t == null) return;
            IsEditMode = true;
            _editingId = t.Id;
            FormTitleField = t.Title;
            FormAmount = t.Amount.ToString("F2");
            FormType = t.Type;
            FormDate = t.Date;
            FormNote = t.Note ?? string.Empty;
            FormError = string.Empty;
            RefreshCategoryOptions();
            FormCategory = t.Category;
            IsFormVisible = true;
            OnPropertyChanged(nameof(FormTitle));
        }

        private void CloseForm()
        {
            IsFormVisible = false;
            FormError = string.Empty;
        }

        private void RefreshCategoryOptions()
        {
            CategoryOptions.Clear();
            var cats = FormType == TranscationType.Income
                ? CategoryList.IncomeCategories
                : CategoryList.ExpenseCategories;
            foreach (var c in cats)
                CategoryOptions.Add(c);
            FormCategory = CategoryOptions.FirstOrDefault() ?? string.Empty;
        }

        // ─────────────────────────────────────
        // Save
        // ─────────────────────────────────────
        private async void SaveTransactionAsync()
        {
            if (string.IsNullOrWhiteSpace(FormTitleField))
            { FormError = "Title is required."; return; }

            if (!decimal.TryParse(FormAmount, out var amount) || amount <= 0)
            { FormError = "Enter a valid amount greater than 0."; return; }

            if (string.IsNullOrWhiteSpace(FormCategory))
            { FormError = "Please select a category."; return; }

            FormError = string.Empty;

            try
            {
                if (IsEditMode)
                {
                    var existing = _allTransactions
                        .First(t => t.Id == _editingId);
                    existing.Title = FormTitleField.Trim();
                    existing.Amount = amount;
                    existing.Type = FormType;
                    existing.Category = FormCategory;
                    existing.Date = FormDate;
                    existing.Note = FormNote.Trim();

                    await _store.UpdateAsync(existing);
                    _logger!.LogInformation($"Updated: {existing.Title}");

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _transactionsView.Refresh();
                        UpdateSummary();
                    });
                }
                else
                {
                    var newTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        Title = FormTitleField.Trim(),
                        Amount = amount,
                        Type = FormType,
                        Category = FormCategory,
                        Date = FormDate,
                        Note = FormNote.Trim()
                    };

                    await _store.AddAsync(newTransaction);

                    // ← Key fix: add to collection AND refresh on UI thread
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _allTransactions.Add(newTransaction);
                        BuildFilterLists();
                        _transactionsView.Refresh();
                        UpdateSummary();
                        OnPropertyChanged(nameof(TransactionsView));
                    });

                    _logger!.LogInformation($"Added: {newTransaction.Title}");
                }

                TransactionsChanged?.Invoke();
                CloseForm();
            }
            catch (Exception ex)
            {
                FormError = "Failed to save. Please try again.";
                _logger!.LogError("Save transaction failed", ex);
            }
        }

        // ─────────────────────────────────────
        // Delete
        // ─────────────────────────────────────
        private async void DeleteTransactionAsync(Transaction? t)
        {
            if (t == null) return;
            try
            {
                await _store.DeleteAsync(t.Id);

                App.Current.Dispatcher.Invoke(() =>
                {
                    _allTransactions.Remove(t);
                    BuildFilterLists();
                    _transactionsView.Refresh();
                    UpdateSummary();
                    OnPropertyChanged(nameof(TransactionsView));
                });

                TransactionsChanged?.Invoke();
                _logger!.LogInformation($"Deleted: {t.Title}");
            }
            catch (Exception ex)
            {
                _logger!.LogError("Delete transaction failed", ex);
            }
        }
    }
}