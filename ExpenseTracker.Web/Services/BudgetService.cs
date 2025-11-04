using System.Collections.Generic;

namespace ExpenseTracker.Web.Services;

public class BudgetService
{
    // key example: "2025-11"
    private readonly Dictionary<string, decimal> _budgets = new();

    private static string Key(int year, int month) => $"{year:D4}-{month:D2}";

    public void SetBudget(int year, int month, decimal amount)
    {
        _budgets[Key(year, month)] = amount;
    }

    public decimal? GetBudget(int year, int month)
    {
        return _budgets.TryGetValue(Key(year, month), out var value)
            ? value
            : (decimal?)null;
    }
}
