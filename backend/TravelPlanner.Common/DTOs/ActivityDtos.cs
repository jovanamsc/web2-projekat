namespace TravelPlanner.Common.DTOs;

public class ActivityDto
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public int? DestinationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? Time { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Status { get; set; } = "Planned";
    public DateTime CreatedAt { get; set; }
}

public class CreateActivityDto
{
    public int? DestinationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? Time { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Status { get; set; } = "Planned";
}

public class UpdateActivityDto
{
    public int? DestinationId { get; set; }
    public string? Title { get; set; }
    public DateTime? Date { get; set; }
    public TimeSpan? Time { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Status { get; set; }
}
