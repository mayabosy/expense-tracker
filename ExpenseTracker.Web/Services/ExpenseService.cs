using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Web.Models;

namespace ExpenseTracker.Web.Services;

public class ExpenseService
{
    private readonly List<Expense> _expenses = new();

    public IReadOnlyList<Expense> GetAll() => _expenses;

    public void Add(Expense expense)
    {
        _expenses.Add(expense);
    }

    public decimal GetTotal() => _expenses.Sum(e => e.Amount);
}
