using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.Models
{
    public static class CategoryList
    {
        public static readonly string[] ExpenseCategories =
        {
            "Housing",
            "Food & Dining",
            "Transport",
            "Healthcare",
            "Entertainment",
            "Shopping",
            "Education",
            "Utilities",
            "Insurance",
            "Personal Care",
            "Travel",
            "Gifts & Donations",
            "Other"
        };

        public static readonly string[] IncomeCategories =
        {
            "Salary",
            "Freelance",
            "Business",
            "Investment",
            "Rental Income",
            "Gift",
            "Bonus",
            "Other"
        };

        // Combined — used when filter is "All"
        public static string[] All =>
            IncomeCategories.Concat(ExpenseCategories)
                            .Distinct()
                            .OrderBy(c => c)
                            .ToArray();
    }
}
