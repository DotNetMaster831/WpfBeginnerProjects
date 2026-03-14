using ExpenseBudgetManager.Models;
using MaterialDesignThemes.Wpf;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ExpenseBudgetManager.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ─────────────────────────────────────
        // Module ViewModels — created once
        // ─────────────────────────────────────
        private readonly DashboardViewModel _dashboardVM = new();
        private readonly TransactionViewModel _transactionVM = new();
        private readonly BudgetViewModel _budgetVM = new();
        private readonly SettingsViewModel _settingsVM = new();

        // ─────────────────────────────────────
        // Navigation
        // ─────────────────────────────────────
        public ObservableCollection<NavigationItem> NavItems { get; }

        private NavigationItem? _selectedNavItem;
        public NavigationItem? SelectedNavItem
        {
            get => _selectedNavItem;
            set
            {
                if (SetProperty(ref _selectedNavItem, value))
                {
                    NavigateTo(value);
                    _logger!.LogInformation($"Navigated to: {value?.Label}");
                }
            }
        }

        // ─────────────────────────────────────
        // Current view
        // ─────────────────────────────────────
        private BaseViewModel _currentView;
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // ─────────────────────────────────────
        // Status bar
        // ─────────────────────────────────────
        private string _balanceText = "₹0.00";
        public string BalanceText
        {
            get => _balanceText;
            set => SetProperty(ref _balanceText, value);
        }

        private string _monthText = DateTime.Now.ToString("MMMM yyyy");
        public string MonthText
        {
            get => _monthText;
            set => SetProperty(ref _monthText, value);
        }

        // ─────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────
        public MainViewModel()
        {
            _logger!.LogInformation("MainViewModel initializing...");

            NavItems = new ObservableCollection<NavigationItem>
            {
                new() { Label = "Dashboard",    IconKind = PackIconKind.ViewDashboard   },
                new() { Label = "Transactions", IconKind = PackIconKind.SwapHorizontal  },
                new() { Label = "Budget",       IconKind = PackIconKind.ChartDonut      },
                new() { Label = "Settings",     IconKind = PackIconKind.Cog             }
            };

            _transactionVM.TransactionsChanged += () =>
            {
                _dashboardVM.Refresh();
                UpdateBalance();
            };

            // Default view
            _currentView = _dashboardVM;
            _selectedNavItem = NavItems[0];

            _logger!.LogInformation("MainViewModel initialized.");
        }
        // Add this method:
        private void UpdateBalance()
        {
            // Will calculate properly once store is accessible
            // For now trigger dashboard which recalculates
            _logger!.LogInformation("Balance update triggered.");
        }
        // ─────────────────────────────────────
        // Navigation logic
        // ─────────────────────────────────────
        private void NavigateTo(NavigationItem? item)
        {
            if (item == null) return;

            CurrentView = item.Label switch
            {
                "Dashboard" => _dashboardVM,
                "Transactions" => _transactionVM,
                "Budget" => _budgetVM,
                "Settings" => _settingsVM,
                _ => _dashboardVM
            };
        }

        // ─────────────────────────────────────
        // Called by child ViewModels to update
        // the status bar balance
        // ─────────────────────────────────────
        public void UpdateBalance(decimal balance)
        {
            BalanceText = balance >= 0
                ? $"₹{balance:N2}"
                : $"-₹{Math.Abs(balance):N2}";
        }
    }
}
