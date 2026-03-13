using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            _logger!.LogInformation("SettingsViewModel initialized.");
        }
    }
}
