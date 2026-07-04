using AutoMapper;
using TravelPlanner.BudgetService.Models;
using TravelPlanner.Common.DTOs;

namespace TravelPlanner.BudgetService.Mapping;

public class BudgetMappingProfile : Profile
{
    public BudgetMappingProfile()
    {
        CreateMap<Expense, ExpenseDto>();
        CreateMap<CreateExpenseDto, Expense>();
    }
}
