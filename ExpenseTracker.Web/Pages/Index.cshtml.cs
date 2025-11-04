using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseTracker.Web.Models;
using ExpenseTracker.Web.Services;

namespace ExpenseTracker.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ExpenseService _expenseService;
    private readonly BudgetService _budgetService;

    public IndexModel(ExpenseService expenseService, BudgetService budgetService)
    {
        _expenseService = expenseService;
        _budgetService = budgetService;
    }

    // Form fields for adding expenses
    [BindProperty]
    public DateTime Date { get; set; } = DateTime.Today;

    [BindProperty]
    public string Category { get; set; } = string.Empty;

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public decimal Amount { get; set; }

    // Predefined category list
    public List<string> CategoryOptions { get; } = new()
    {
        "Groceries",
        "Personal Care",
        "Rent",
        "Bills",
        "Entertainment",
        "Health",
        "Restaurants"
    };

    // Budget form field
    [BindProperty]
    public decimal BudgetAmount { get; set; }

    // All expenses
    public IReadOnlyList<Expense> Expenses { get; private set; } = Array.Empty<Expense>();
    public decimal Total { get; private set; }

    // Grouped: Month -> Categories -> Expenses
    public List<MonthlyGroup> MonthlyGroups { get; private set; } = new();

    public void OnGet()
    {
        LoadData();
    }

    public IActionResult OnPostAdd()
    {
        if (Amount <= 0)
        {
            ModelState.AddModelError(nameof(Amount), "Amount must be positive.");
        }

        if (string.IsNullOrWhiteSpace(Category))
        {
            ModelState.AddModelError(nameof(Category), "Category is required.");
        }

        if (!ModelState.IsValid)
        {
            LoadData();
            return Page();
        }

        var expense = new Expense
        {
            Date = Date,
            Category = Category.Trim(),
            Description = Description?.Trim() ?? string.Empty,
            Amount = Amount
        };

        _expenseService.Add(expense);

        // Clear inputs (keep date)
        Category = string.Empty;
        Description = string.Empty;
        Amount = 0;

        LoadData();
        return Page();
    }

    // This handles the "Set Budget" form. It gets the year & month explicitly from hidden inputs.
    public IActionResult OnPostSetBudget(int year, int month)
    {
        // Ignore validation for the expense form fields in this handler
        ModelState.Remove(nameof(Date));
        ModelState.Remove(nameof(Category));
        ModelState.Remove(nameof(Description));
        ModelState.Remove(nameof(Amount));

        if (BudgetAmount <= 0)
        {
            ModelState.AddModelError(nameof(BudgetAmount),
                "Budget must be positive. Enter just a number like 500 or 500.00");
        }

        if (!ModelState.IsValid)
        {
            LoadData();
            return Page();
        }

        // Store the budget for the given month/year (current month from the form)
        _budgetService.SetBudget(year, month, BudgetAmount);

        LoadData();
        return Page();
    }


    private void LoadData()
    {
        Expenses = _expenseService.GetAll();
        Total = _expenseService.GetTotal();

        // Build month -> category -> expenses, and inject budgets
        var groups = Expenses
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(monthGroup =>
            {
                var mg = new MonthlyGroup
                {
                    Year = monthGroup.Key.Year,
                    Month = monthGroup.Key.Month,
                    Total = monthGroup.Sum(e => e.Amount),
                    Categories = monthGroup
                        .GroupBy(e => e.Category)
                        .Select(catGroup => new CategoryGroup
                        {
                            Category = catGroup.Key,
                            Total = catGroup.Sum(e => e.Amount),
                            Expenses = catGroup
                                .OrderBy(e => e.Date)
                                .ThenBy(e => e.Description)
                                .ToList()
                        })
                        .OrderBy(c => c.Category)
                        .ToList()
                };

                mg.Budget = _budgetService.GetBudget(mg.Year, mg.Month);

                return mg;
            })
            .ToList();

        MonthlyGroups = groups
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ToList();
    }
}

// A month with its categories & budget
public class MonthlyGroup
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Total { get; set; }
    public List<CategoryGroup> Categories { get; set; } = new();

    public decimal? Budget { get; set; }

    public decimal? Remaining => Budget.HasValue ? Budget.Value - Total : null;
    public bool HasBudget => Budget.HasValue;
    public bool IsOverBudget => Remaining.HasValue && Remaining.Value < 0;

    public string MonthLabel => new DateTime(Year, Month, 1).ToString("yyyy MMM");
}

// A category inside a month with its expenses
public class CategoryGroup
{
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<Expense> Expenses { get; set; } = new();
}
