using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.Models
{
    public class NavigationItem
    {
        public string Label { get; set; } = string.Empty;
        public PackIconKind IconKind { get; set; }
        public string? BadgeCount { get; set; }
    }
}
