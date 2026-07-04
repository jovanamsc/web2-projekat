using TravelPlanner.Common.DTOs;

namespace TravelPlanner.Common.Interfaces;

public interface IBudgetServiceContract
{
    Task<List<ExpenseDto>> GetExpensesAsync(int travelPlanId);
    Task<ExpenseDto?> GetExpenseByIdAsync(int travelPlanId, int id);
    Task<ExpenseDto> CreateExpenseAsync(int travelPlanId, int userId, CreateExpenseDto dto);
    Task<ExpenseDto?> UpdateExpenseAsync(int travelPlanId, int id, int userId, UpdateExpenseDto dto);
    Task<bool> DeleteExpenseAsync(int travelPlanId, int id, int userId);
    Task<BudgetSummaryDto> GetBudgetSummaryAsync(int travelPlanId, decimal plannedBudget);
    Task<decimal> GetTotalExpensesAsync(int travelPlanId);
    Task<bool> DeleteExpensesByPlanIdAsync(int travelPlanId); // poziva se kad se obrise cijeli plan
}
