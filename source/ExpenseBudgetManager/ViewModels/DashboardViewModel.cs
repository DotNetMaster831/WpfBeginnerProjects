using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public DashboardViewModel()
        {
            _logger!.LogInformation("DashboardViewModel initialized.");
        }
    }
}
