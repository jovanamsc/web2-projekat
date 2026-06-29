namespace TravelPlanner.Common.DTOs;

public class TravelPlanDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Budget { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<DestinationDto> Destinations { get; set; } = new();
    public List<ActivityDto> Activities { get; set; } = new();
    public List<ChecklistItemDto> ChecklistItems { get; set; } = new();
    public decimal TotalExpenses { get; set; }
    public decimal RemainingBudget { get; set; }
}

public class CreateTravelPlanDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Budget { get; set; }
    public string? Notes { get; set; }
}

public class UpdateTravelPlanDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public string? Notes { get; set; }
}
