using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        public BudgetViewModel()
        {
            _logger!.LogInformation("BudgetViewModel initialized.");
        }
    }
}
