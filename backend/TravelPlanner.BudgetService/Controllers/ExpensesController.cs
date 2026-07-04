using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Common.DTOs;
using TravelPlanner.Common.Interfaces;

namespace TravelPlanner.BudgetService.Controllers;

[ApiController]
[Route("api/travel-plans/{planId}/expenses")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IBudgetServiceContract _budgetService;

    public ExpensesController(IBudgetServiceContract budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExpenseDto>>> GetAll(int planId)
    {
        var expenses = await _budgetService.GetExpensesAsync(planId);
        return Ok(expenses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseDto>> GetById(int planId, int id)
    {
        var expense = await _budgetService.GetExpenseByIdAsync(planId, id);
        if (expense == null) return NotFound();
        return Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(int planId, [FromBody] CreateExpenseDto dto)
    {
        try
        {
            var userId = GetUserId();
            var expense = await _budgetService.CreateExpenseAsync(planId, userId, dto);
            return CreatedAtAction(nameof(GetById), new { planId, id = expense.Id }, expense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseDto>> Update(int planId, int id, [FromBody] UpdateExpenseDto dto)
    {
        try
        {
            var userId = GetUserId();
            var expense = await _budgetService.UpdateExpenseAsync(planId, id, userId, dto);
            if (expense == null) return NotFound();
            return Ok(expense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int planId, int id)
    {
        var userId = GetUserId();
        var result = await _budgetService.DeleteExpenseAsync(planId, id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<BudgetSummaryDto>> GetSummary(int planId, [FromQuery] decimal plannedBudget = 0)
    {
        var summary = await _budgetService.GetBudgetSummaryAsync(planId, plannedBudget);
        return Ok(summary);
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
