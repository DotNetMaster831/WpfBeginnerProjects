using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace ExpenseBudgetManager.Models
{
    public class SummaryCardModel
    {
        public string Label { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string SubText { get; set; } = string.Empty;
        public string IconKind { get; set; } = string.Empty;
        public Brush AmountColor { get; set; } = Brushes.White;
        public Brush IconBg { get; set; } = Brushes.Transparent;
        public Brush IconColor { get; set; } = Brushes.White;
    }
}
