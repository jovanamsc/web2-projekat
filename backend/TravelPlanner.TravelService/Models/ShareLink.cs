using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanner.TravelService.Models;

public class ShareLink
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TravelPlanId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string AccessLevel { get; set; } = "View";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    [ForeignKey(nameof(TravelPlanId))]
    public TravelPlan TravelPlan { get; set; } = null!;
}
