namespace TravelPlanner.Common.DTOs;

public class DestinationDto
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime ArrivalDate { get; set; }
    public DateTime DepartureDate { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class CreateDestinationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime ArrivalDate { get; set; }
    public DateTime DepartureDate { get; set; } // mora biti poslije ArrivalDate, validira se u servisu
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class UpdateDestinationDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public DateTime? DepartureDate { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}
