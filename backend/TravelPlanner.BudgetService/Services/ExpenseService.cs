using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TravelPlanner.BudgetService.Data;
using TravelPlanner.BudgetService.Models;
using TravelPlanner.Common.DTOs;
using TravelPlanner.Common.Enums;
using TravelPlanner.Common.Interfaces;

namespace TravelPlanner.BudgetService.Services;

public class ExpenseService : IBudgetServiceContract
{
    private readonly BudgetDbContext _context;
    private readonly IMapper _mapper;

    public ExpenseService(BudgetDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ExpenseDto>> GetExpensesAsync(int travelPlanId)
    {
        var expenses = await _context.Expenses
            .Where(e => e.TravelPlanId == travelPlanId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        return _mapper.Map<List<ExpenseDto>>(expenses);
    }

    public async Task<ExpenseDto?> GetExpenseByIdAsync(int travelPlanId, int id)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == travelPlanId);
        return expense == null ? null : _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<ExpenseDto> CreateExpenseAsync(int travelPlanId, int userId, CreateExpenseDto dto)
    {
        if (dto.Amount < 0)
            throw new ArgumentException("Amount cannot be negative.");
        if (!ExpenseCategory.IsValid(dto.Category))
            throw new ArgumentException($"Invalid category. Allowed: {string.Join(", ", ExpenseCategory.All)}");

        var expense = _mapper.Map<Expense>(dto);
        expense.TravelPlanId = travelPlanId;
        expense.UserId = userId;

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<ExpenseDto?> UpdateExpenseAsync(int travelPlanId, int id, int userId, UpdateExpenseDto dto)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == travelPlanId);

        if (expense == null) return null;

        if (dto.Title != null) expense.Title = dto.Title;
        if (dto.Category != null)
        {
            if (!ExpenseCategory.IsValid(dto.Category))
                throw new ArgumentException($"Invalid category. Allowed: {string.Join(", ", ExpenseCategory.All)}");
            expense.Category = dto.Category;
        }
        if (dto.Amount.HasValue)
        {
            if (dto.Amount.Value < 0) throw new ArgumentException("Amount cannot be negative.");
            expense.Amount = dto.Amount.Value;
        }
        if (dto.Date.HasValue) expense.Date = dto.Date.Value;
        if (dto.Description != null) expense.Description = dto.Description;

        await _context.SaveChangesAsync();
        return _mapper.Map<ExpenseDto>(expense);
    }

    public async Task<bool> DeleteExpenseAsync(int travelPlanId, int id, int userId)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && e.TravelPlanId == travelPlanId);

        if (expense == null) return false;

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<BudgetSummaryDto> GetBudgetSummaryAsync(int travelPlanId, decimal plannedBudget)
    {
        var expenses = await _context.Expenses
            .Where(e => e.TravelPlanId == travelPlanId)
            .ToListAsync();

        var totalExpenses = expenses.Sum(e => e.Amount);
        var byCategory = expenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        return new BudgetSummaryDto
        {
            TravelPlanId = travelPlanId,
            PlannedBudget = plannedBudget,
            TotalExpenses = totalExpenses,
            RemainingBudget = plannedBudget - totalExpenses,
            ExpensesByCategory = byCategory
        };
    }

    public async Task<decimal> GetTotalExpensesAsync(int travelPlanId)
    {
        return await _context.Expenses
            .Where(e => e.TravelPlanId == travelPlanId)
            .SumAsync(e => e.Amount);
    }

    public async Task<bool> DeleteExpensesByPlanIdAsync(int travelPlanId)
    {
        var expenses = await _context.Expenses
            .Where(e => e.TravelPlanId == travelPlanId)
            .ToListAsync();

        if (!expenses.Any()) return true;

        _context.Expenses.RemoveRange(expenses);
        await _context.SaveChangesAsync();
        return true;
    }
}
