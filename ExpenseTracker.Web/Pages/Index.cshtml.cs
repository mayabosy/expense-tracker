using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseTracker.Web.Models;
using ExpenseTracker.Web.Services;

namespace ExpenseTracker.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ExpenseService _expenseService;

    public IndexModel(ExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [BindProperty]
    public DateTime Date { get; set; } = DateTime.Today;

    [BindProperty]
    public string Category { get; set; } = string.Empty;

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public decimal Amount { get; set; }

    public IReadOnlyList<Expense> Expenses { get; private set; } = Array.Empty<Expense>();
    public decimal Total { get; private set; }

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

        // Reset fields (except date)
        Category = string.Empty;
        Description = string.Empty;
        Amount = 0;

        LoadData();
        return Page();
    }

    private void LoadData()
    {
        Expenses = _expenseService.GetAll();
        Total = _expenseService.GetTotal();
    }
}
