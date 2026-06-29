using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanner.TravelService.Models;

public class Activity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TravelPlanId { get; set; }

    public int? DestinationId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    public TimeSpan? Time { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedCost { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Planned";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TravelPlanId))]
    public TravelPlan TravelPlan { get; set; } = null!;

    [ForeignKey(nameof(DestinationId))]
    public Destination? Destination { get; set; }
}
