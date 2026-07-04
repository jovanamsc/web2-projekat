namespace TravelPlanner.ShareService;

[Serializable]
public class ShareToken
{
    public string Token { get; set; } = string.Empty;
    public int TravelPlanId { get; set; }
    public string AccessType { get; set; } = string.Empty; // VIEW or EDIT
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
