using System;

namespace ExpenseTracker.Web.Models;

public class Expense
{
    public DateTime Date { get; set; }
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
}
 