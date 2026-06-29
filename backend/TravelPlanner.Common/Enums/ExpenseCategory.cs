namespace TravelPlanner.Common.Enums;

public static class ExpenseCategory
{
    public const string Transport = "Transport";
    public const string Accommodation = "Accommodation";
    public const string Food = "Food";
    public const string Tickets = "Tickets";
    public const string Shopping = "Shopping";
    public const string Other = "Other";

    public static readonly string[] All = [Transport, Accommodation, Food, Tickets, Shopping, Other];

    public static bool IsValid(string category) => All.Contains(category);
}
