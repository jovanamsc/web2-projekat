using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanner.TravelService.Models;

public class ChecklistItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TravelPlanId { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TravelPlanId))]
    public TravelPlan TravelPlan { get; set; } = null!;
}
