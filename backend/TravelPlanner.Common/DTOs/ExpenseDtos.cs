namespace TravelPlanner.Common.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseDto
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}

public class UpdateExpenseDto
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
}

public class BudgetSummaryDto
{
    public int TravelPlanId { get; set; }
    public decimal PlannedBudget { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal RemainingBudget { get; set; }
    public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new(); // grupisano po kategorijama za prikaz na frontu
}
