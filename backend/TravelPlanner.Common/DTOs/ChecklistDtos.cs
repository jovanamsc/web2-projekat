namespace TravelPlanner.Common.DTOs;

public class ChecklistItemDto
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateChecklistItemDto
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateChecklistItemDto
{
    public string? Title { get; set; }
    public bool? IsCompleted { get; set; }
}
