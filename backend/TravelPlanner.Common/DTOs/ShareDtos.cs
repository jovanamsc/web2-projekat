namespace TravelPlanner.Common.DTOs;

public class ShareLinkDto
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = "View";
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CreateShareLinkDto
{
    public string AccessLevel { get; set; } = "View";
    public DateTime? ExpiresAt { get; set; }
}
