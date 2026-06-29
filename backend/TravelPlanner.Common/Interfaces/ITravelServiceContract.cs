using TravelPlanner.Common.DTOs;

namespace TravelPlanner.Common.Interfaces;

public interface ITravelServiceContract
{
    // Travel Plans
    Task<List<TravelPlanDto>> GetAllPlansAsync(int userId);
    Task<List<TravelPlanDto>> GetAllPlansAdminAsync();
    Task<TravelPlanDto?> GetPlanByIdAsync(int id, int userId);
    Task<TravelPlanDto> CreatePlanAsync(int userId, CreateTravelPlanDto dto);
    Task<TravelPlanDto?> UpdatePlanAsync(int id, int userId, UpdateTravelPlanDto dto);
    Task<bool> DeletePlanAsync(int id, int userId);

    // Destinations
    Task<List<DestinationDto>> GetDestinationsAsync(int planId);
    Task<DestinationDto?> GetDestinationByIdAsync(int planId, int id);
    Task<DestinationDto> CreateDestinationAsync(int planId, int userId, CreateDestinationDto dto);
    Task<DestinationDto?> UpdateDestinationAsync(int planId, int id, int userId, UpdateDestinationDto dto);
    Task<bool> DeleteDestinationAsync(int planId, int id, int userId);

    // Activities
    Task<List<ActivityDto>> GetActivitiesAsync(int planId);
    Task<ActivityDto?> GetActivityByIdAsync(int planId, int id);
    Task<ActivityDto> CreateActivityAsync(int planId, int userId, CreateActivityDto dto);
    Task<ActivityDto?> UpdateActivityAsync(int planId, int id, int userId, UpdateActivityDto dto);
    Task<bool> DeleteActivityAsync(int planId, int id, int userId);

    // Checklist
    Task<List<ChecklistItemDto>> GetChecklistItemsAsync(int planId);
    Task<ChecklistItemDto> CreateChecklistItemAsync(int planId, int userId, CreateChecklistItemDto dto);
    Task<ChecklistItemDto?> UpdateChecklistItemAsync(int planId, int id, int userId, UpdateChecklistItemDto dto);
    Task<bool> DeleteChecklistItemAsync(int planId, int id, int userId);

    // Sharing
    Task<ShareLinkDto> CreateShareLinkAsync(int planId, int userId, CreateShareLinkDto dto);
    Task<TravelPlanDto?> GetPlanByShareTokenAsync(string token);
    Task<ShareLinkDto?> GetShareLinkInfoAsync(string token);
    Task<bool> DeleteShareLinkAsync(int planId, int id, int userId);
    Task<List<ShareLinkDto>> GetShareLinksAsync(int planId, int userId);
}
