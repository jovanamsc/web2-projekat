namespace TravelPlanner.Common.Enums;

public static class ActivityStatus
{
    public const string Planned = "Planned";
    public const string Reserved = "Reserved";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Planned, Reserved, Completed, Cancelled];

    public static bool IsValid(string status) => All.Contains(status);
}
