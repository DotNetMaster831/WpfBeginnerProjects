using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        public TransactionViewModel()
        {
            _logger!.LogInformation("TransactionViewModel initialized.");
        }
    }
}
