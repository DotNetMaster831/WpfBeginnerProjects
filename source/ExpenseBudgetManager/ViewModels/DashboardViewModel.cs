using ExpenseBudgetManager.Commands;
using ExpenseBudgetManager.Infrastructure;
using ExpenseBudgetManager.Models;
using ExpenseBudgetManager.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace ExpenseBudgetManager.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ITransactionStore _store;

        // ─────────────────────────────────────
        // Summary totals
        // ─────────────────────────────────────
        private decimal _totalIncome;
        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        private decimal _totalExpense;
        public decimal TotalExpense
        {
            get => _totalExpense;
            set => SetProperty(ref _totalExpense, value);
        }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        private decimal _savingsRate;
        public decimal SavingsRate
        {
            get => _savingsRate;
            set => SetProperty(ref _savingsRate, value);
        }

        // ─────────────────────────────────────
        // Recent transactions
        // ─────────────────────────────────────
        public ObservableCollection<Transaction> RecentTransactions { get; }
            = new();

        private bool _hasTransactions;
        public bool HasTransactions
        {
            get => _hasTransactions;
            set => SetProperty(ref _hasTransactions, value);
        }

        private string _monthDisplay = DateTime.Now.ToString("MMMM yyyy");
        public string MonthDisplay
        {
            get => _monthDisplay;
            set => SetProperty(ref _monthDisplay, value);
        }

        // ─────────────────────────────────────
        // Chart
        // ─────────────────────────────────────
        private PlotModel _incomeExpenseChart = new();
        public PlotModel IncomeExpenseChart
        {
            get => _incomeExpenseChart;
            set => SetProperty(ref _incomeExpenseChart, value);
        }

        // ─────────────────────────────────────
        // Commands
        // ─────────────────────────────────────
        public ICommand RefreshCommand { get; }

        public DashboardViewModel()
        {
            _store = ServiceLocator.TransactionStore;
            RefreshCommand = new RelayCommad(_ => LoadDataAsync());

            _logger!.LogInformation("DashboardViewModel initialized.");
            LoadDataAsync();
        }

        // ─────────────────────────────────────
        // Data loading
        // ─────────────────────────────────────
        private async void LoadDataAsync()
        {
            try
            {
                _logger!.LogInformation("Dashboard loading data...");

                var all = await _store.GetAllAsync();

                // Current month totals
                var thisMonth = all.Where(t =>
                    t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year).ToList();

                TotalIncome = thisMonth
                    .Where(t => t.Type == TranscationType.Income)
                    .Sum(t => t.Amount);

                TotalExpense = thisMonth
                    .Where(t => t.Type == TranscationType.Expense)
                    .Sum(t => t.Amount);

                Balance = TotalIncome - TotalExpense;
                SavingsRate = TotalIncome > 0
                    ? Math.Round((Balance / TotalIncome) * 100, 1)
                    : 0;

                // Recent 5
                App.Current.Dispatcher.Invoke(() =>
                {
                    RecentTransactions.Clear();
                    foreach (var t in all
                        .OrderByDescending(t => t.Date)
                        .Take(5))
                        RecentTransactions.Add(t);

                    HasTransactions = RecentTransactions.Count > 0;
                });

                // Build chart
                BuildIncomeExpenseChart(all);

                _logger!.LogInformation(
                    $"Dashboard loaded — Income:{TotalIncome} " +
                    $"Expense:{TotalExpense} Balance:{Balance}");
            }
            catch (Exception ex)
            {
                _logger!.LogError("Failed to load dashboard data", ex);
            }
        }

        // ─────────────────────────────────────
        // Chart builder — last 6 months
        // ─────────────────────────────────────
        private void BuildIncomeExpenseChart(List<Transaction> all)
        {
            var model = new PlotModel
            {
                Background = OxyColor.FromArgb(0, 0, 0, 0), // transparent
                PlotAreaBackground = OxyColor.FromArgb(0, 0, 0, 0),
                TextColor = OxyColor.Parse("#B8C9A3"),
                PlotAreaBorderColor = OxyColor.Parse("#2E5230"),
                PlotAreaBorderThickness = new OxyThickness(0, 0, 0, 1),
                Padding = new OxyThickness(0, 10, 0, 0),
                TitleColor = OxyColor.Parse("#F2E8CF"),
            };

            // CHANGE THIS — CategoryAxis must be on Y axis for BarSeries
            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,   // ← was Bottom, must be Left
                IsZoomEnabled = false,
                IsPanEnabled = false,
                TicklineColor = OxyColor.Parse("#2E5230"),
                AxislineColor = OxyColor.Parse("#2E5230"),
                TextColor = OxyColor.Parse("#B8C9A3"),
                FontSize = 11,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                GapWidth = 0.4,
            };

            // CHANGE THIS — ValueAxis must be on X axis (Bottom)
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,  // ← was Left, must be Bottom
                IsZoomEnabled = false,
                IsPanEnabled = false,
                TextColor = OxyColor.Parse("#B8C9A3"),
                TicklineColor = OxyColor.Parse("#2E5230"),
                AxislineColor = OxyColor.Parse("#2E5230"),
                FontSize = 11,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.Parse("#2E5230"),
                MinorGridlineStyle = LineStyle.None,
                MinimumPadding = 0,
                AbsoluteMinimum = 0,
                StringFormat = "₹#,##0",
            };

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            // ── Series ────────────────────────────
            var incomeSeries = new BarSeries
            {
                Title = "Income",
                FillColor = OxyColor.Parse("#6A994E"),
                StrokeThickness = 0,
                BarWidth = 0.35,
            };

            var expenseSeries = new BarSeries
            {
                Title = "Expense",
                FillColor = OxyColor.Parse("#BC4749"),
                StrokeThickness = 0,
                BarWidth = 0.35,
            };

            // ── Last 6 months data ─────────────────
            var now = DateTime.Now;
            var months = Enumerable.Range(0, 6)
                .Select(i => now.AddMonths(-5 + i))
                .ToList();

            foreach (var month in months)
            {
                // Label on X axis
                categoryAxis.Labels.Add(month.ToString("MMM"));

                var monthData = all.Where(t =>
                    t.Date.Month == month.Month &&
                    t.Date.Year == month.Year).ToList();

                var income = (double)monthData
                    .Where(t => t.Type == TranscationType.Income)
                    .Sum(t => t.Amount);

                var expense = (double)monthData
                    .Where(t => t.Type == TranscationType.Expense)
                    .Sum(t => t.Amount);

                incomeSeries.Items.Add(new BarItem(income));
                expenseSeries.Items.Add(new BarItem(expense));
            }

            // ── Legend ────────────────────────────
            model.IsLegendVisible = true;
            model.Legends.Add(new OxyPlot.Legends.Legend
            {
                LegendPosition = OxyPlot.Legends.LegendPosition.TopRight,
                LegendBackground = OxyColor.FromArgb(0, 0, 0, 0),
                LegendBorder = OxyColor.FromArgb(0, 0, 0, 0),
                LegendTextColor = OxyColor.Parse("#B8C9A3"),
                LegendFontSize = 11,
            });

            model.Series.Add(incomeSeries);
            model.Series.Add(expenseSeries);

            App.Current.Dispatcher.Invoke(() =>
            {
                IncomeExpenseChart = model;
            });

            _logger!.LogInformation("Income vs Expense chart built.");
        }

        public void Refresh() => LoadDataAsync();

    }
}
