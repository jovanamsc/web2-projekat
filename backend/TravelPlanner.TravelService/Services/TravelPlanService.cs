using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using TravelPlanner.Common.DTOs;
using TravelPlanner.Common.Enums;
using TravelPlanner.Common.Interfaces;
using TravelPlanner.TravelService.Data;
using TravelPlanner.TravelService.Models;

namespace TravelPlanner.TravelService.Services;

public class TravelPlanService : ITravelServiceContract
{
    private readonly TravelDbContext _context;
    private readonly IMapper _mapper;
    private readonly HttpClient _shareServiceClient;

    public TravelPlanService(TravelDbContext context, IMapper mapper, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _mapper = mapper;
        _shareServiceClient = httpClientFactory.CreateClient("ShareService");
    }

    // Planovi putovanja
    public async Task<List<TravelPlanDto>> GetAllPlansAsync(int userId)
    {
        var plans = await _context.TravelPlans
            .Include(p => p.Destinations)
            .Include(p => p.Activities)
            .Include(p => p.ChecklistItems)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        return _mapper.Map<List<TravelPlanDto>>(plans);
    }

    public async Task<List<TravelPlanDto>> GetAllPlansAdminAsync()
    {
        var plans = await _context.TravelPlans
            .Include(p => p.Destinations)
            .Include(p => p.Activities)
            .Include(p => p.ChecklistItems)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        return _mapper.Map<List<TravelPlanDto>>(plans);
    }

    public async Task<TravelPlanDto?> GetPlanByIdAsync(int id, int userId, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans
            .Include(p => p.Destinations)
            .Include(p => p.Activities)
            .Include(p => p.ChecklistItems)
            .FirstOrDefaultAsync(p => p.Id == id && (isAdmin || p.UserId == userId));

        return plan == null ? null : _mapper.Map<TravelPlanDto>(plan);
    }

    public async Task<TravelPlanDto> CreatePlanAsync(int userId, CreateTravelPlanDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            throw new ArgumentException("End date cannot be before start date.");
        if (dto.Budget < 0)
            throw new ArgumentException("Budget cannot be negative.");

        var plan = _mapper.Map<TravelPlan>(dto);
        plan.UserId = userId;

        _context.TravelPlans.Add(plan);
        await _context.SaveChangesAsync();

        return _mapper.Map<TravelPlanDto>(plan);
    }

    public async Task<TravelPlanDto?> UpdatePlanAsync(int id, int userId, UpdateTravelPlanDto dto, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans
            .Include(p => p.Destinations)
            .Include(p => p.Activities)
            .Include(p => p.ChecklistItems)
            .FirstOrDefaultAsync(p => p.Id == id && (isAdmin || p.UserId == userId));

        if (plan == null) return null;

        if (dto.Title != null) plan.Title = dto.Title;
        if (dto.Description != null) plan.Description = dto.Description;
        if (dto.StartDate.HasValue) plan.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) plan.EndDate = dto.EndDate.Value;
        if (dto.Budget.HasValue)
        {
            if (dto.Budget.Value < 0) throw new ArgumentException("Budget cannot be negative.");
            plan.Budget = dto.Budget.Value;
        }
        if (dto.Notes != null) plan.Notes = dto.Notes;

        if (plan.EndDate < plan.StartDate)
            throw new ArgumentException("End date cannot be before start date.");

        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return _mapper.Map<TravelPlanDto>(plan);
    }

    public async Task<bool> DeletePlanAsync(int id, int userId, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == id && (isAdmin || p.UserId == userId));
        if (plan == null) return false;

        var activities = await _context.Activities.Where(a => a.TravelPlanId == id).ToListAsync();
        _context.Activities.RemoveRange(activities);

        _context.TravelPlans.Remove(plan);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserPlansAsync(int userId)
    {
        var plans = await _context.TravelPlans.Where(p => p.UserId == userId).ToListAsync();
        if (!plans.Any()) return true;

        var planIds = plans.Select(p => p.Id).ToList();
        var activities = await _context.Activities.Where(a => planIds.Contains(a.TravelPlanId)).ToListAsync();
        _context.Activities.RemoveRange(activities);
        _context.TravelPlans.RemoveRange(plans);
        await _context.SaveChangesAsync();
        return true;
    }

    // Destinacije
    public async Task<List<DestinationDto>> GetDestinationsAsync(int planId)
    {
        var destinations = await _context.Destinations
            .Where(d => d.TravelPlanId == planId)
            .OrderBy(d => d.ArrivalDate)
            .ToListAsync();

        return _mapper.Map<List<DestinationDto>>(destinations);
    }

    public async Task<DestinationDto?> GetDestinationByIdAsync(int planId, int id)
    {
        var dest = await _context.Destinations
            .FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == planId);
        return dest == null ? null : _mapper.Map<DestinationDto>(dest);
    }

    public async Task<DestinationDto> CreateDestinationAsync(int planId, int userId, CreateDestinationDto dto, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && (isAdmin || p.UserId == userId));
        if (plan == null) throw new KeyNotFoundException("Travel plan not found.");

        if (dto.DepartureDate < dto.ArrivalDate)
            throw new ArgumentException("Departure date cannot be before arrival date.");

        var destination = _mapper.Map<Destination>(dto);
        destination.TravelPlanId = planId;

        _context.Destinations.Add(destination);
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return _mapper.Map<DestinationDto>(destination);
    }

    public async Task<DestinationDto?> UpdateDestinationAsync(int planId, int id, int userId, UpdateDestinationDto dto, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && (isAdmin || p.UserId == userId));
        if (plan == null) return null;

        var dest = await _context.Destinations.FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == planId);
        if (dest == null) return null;

        if (dto.Name != null) dest.Name = dto.Name;
        if (dto.Location != null) dest.Location = dto.Location;
        if (dto.ArrivalDate.HasValue) dest.ArrivalDate = dto.ArrivalDate.Value;
        if (dto.DepartureDate.HasValue) dest.DepartureDate = dto.DepartureDate.Value;
        if (dto.Description != null) dest.Description = dto.Description;
        if (dto.Notes != null) dest.Notes = dto.Notes;

        if (dest.DepartureDate < dest.ArrivalDate)
            throw new ArgumentException("Departure date cannot be before arrival date.");

        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return _mapper.Map<DestinationDto>(dest);
    }

    public async Task<bool> DeleteDestinationAsync(int planId, int id, int userId, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && (isAdmin || p.UserId == userId));
        if (plan == null) return false;

        var dest = await _context.Destinations.FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == planId);
        if (dest == null) return false;

        _context.Destinations.Remove(dest);
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Aktivnosti
    public async Task<List<ActivityDto>> GetActivitiesAsync(int planId)
    {
        var activities = await _context.Activities
            .Where(a => a.TravelPlanId == planId)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .ToListAsync();

        return _mapper.Map<List<ActivityDto>>(activities);
    }

    public async Task<ActivityDto?> GetActivityByIdAsync(int planId, int id)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == planId);
        return activity == null ? null : _mapper.Map<ActivityDto>(activity);
    }

    public async Task<ActivityDto> CreateActivityAsync(int planId, int userId, CreateActivityDto dto, bool isAdmin = false)
    {
        if (!ActivityStatus.IsValid(dto.Status))
            throw new ArgumentException($"Invalid status. Allowed: {string.Join(", ", ActivityStatus.All)}");

        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && (isAdmin || p.UserId == userId));
        if (plan == null) throw new KeyNotFoundException("Travel plan not found.");

        var activity = _mapper.Map<Activity>(dto);
        activity.TravelPlanId = planId;

        _context.Activities.Add(activity);
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return _mapper.Map<ActivityDto>(activity);
    }

    public async Task<ActivityDto?> UpdateActivityAsync(int planId, int id, int userId, UpdateActivityDto dto, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && (isAdmin || p.UserId == userId));
        if (plan == null) return null;

        var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == planId);
        if (activity == null) return null;

        if (dto.Title != null) activity.Title = dto.Title;
        if (dto.Date.HasValue) activity.Date = dto.Date.Value;
        if (dto.Time.HasValue) activity.Time = dto.Time.Value;
        if (dto.Location != null) activity.Location = dto.Location;
        if (dto.Description != null) activity.Description = dto.Description;
        if (dto.EstimatedCost.HasValue) activity.EstimatedCost = dto.EstimatedCost.Value;
        if (dto.Status != null)
        {
            if (!ActivityStatus.IsValid(dto.Status))
                throw new ArgumentException($"Invalid status. Allowed: {string.Join(", ", ActivityStatus.All)}");
            activity.Status = dto.Status;
        }
        if (dto.DestinationId.HasValue) activity.DestinationId = dto.DestinationId;

        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return _mapper.Map<ActivityDto>(activity);
    }

    public async Task<bool> DeleteActivityAsync(int planId, int id, int userId, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && (isAdmin || p.UserId == userId));
        if (plan == null) return false;

        var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == planId);
        if (activity == null) return false;

        _context.Activities.Remove(activity);
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Ceklista
    public async Task<List<ChecklistItemDto>> GetChecklistItemsAsync(int planId)
    {
        var items = await _context.ChecklistItems
            .Where(c => c.TravelPlanId == planId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ChecklistItemDto>>(items);
    }

    public async Task<ChecklistItemDto> CreateChecklistItemAsync(int planId, int userId, CreateChecklistItemDto dto, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && (isAdmin || p.UserId == userId));
        if (plan == null) throw new KeyNotFoundException("Travel plan not found.");

        var item = _mapper.Map<ChecklistItem>(dto);
        item.TravelPlanId = planId;

        _context.ChecklistItems.Add(item);
        await _context.SaveChangesAsync();

        return _mapper.Map<ChecklistItemDto>(item);
    }

    public async Task<ChecklistItemDto?> UpdateChecklistItemAsync(int planId, int id, int userId, UpdateChecklistItemDto dto, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && (isAdmin || p.UserId == userId));
        if (plan == null) return null;

        var item = await _context.ChecklistItems.FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == planId);
        if (item == null) return null;

        if (dto.Title != null) item.Title = dto.Title;
        if (dto.IsCompleted.HasValue) item.IsCompleted = dto.IsCompleted.Value;

        await _context.SaveChangesAsync();
        return _mapper.Map<ChecklistItemDto>(item);
    }

    public async Task<bool> DeleteChecklistItemAsync(int planId, int id, int userId, bool isAdmin = false)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && (isAdmin || p.UserId == userId));
        if (plan == null) return false;

        var item = await _context.ChecklistItems.FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == planId);
        if (item == null) return false;

        _context.ChecklistItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    // Dijeljenje
    public async Task<ShareLinkDto> CreateShareLinkAsync(int planId, int userId, CreateShareLinkDto dto)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);
        if (plan == null) throw new KeyNotFoundException("Travel plan not found.");

        var response = await _shareServiceClient.PostAsJsonAsync("api/share-tokens", new
        {
            TravelPlanId = planId,
            AccessType = dto.AccessLevel,
            ExpiryDays = dto.ExpiryDays
        });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ShareTokenResponse>();
        return new ShareLinkDto
        {
            TravelPlanId = planId,
            Token = result!.Token,
            AccessLevel = dto.AccessLevel,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(dto.ExpiryDays)
        };
    }

    public async Task<TravelPlanDto?> GetPlanByShareTokenAsync(string token)
    {
        var response = await _shareServiceClient.GetAsync($"api/share-tokens/{token}");
        if (!response.IsSuccessStatusCode) return null;

        var shareToken = await response.Content.ReadFromJsonAsync<ShareTokenInfo>();
        if (shareToken == null) return null;

        var plan = await _context.TravelPlans
            .Include(p => p.Destinations)
            .Include(p => p.Activities)
            .Include(p => p.ChecklistItems)
            .FirstOrDefaultAsync(p => p.Id == shareToken.TravelPlanId);

        return plan == null ? null : _mapper.Map<TravelPlanDto>(plan);
    }

    public async Task<ShareLinkDto?> GetShareLinkInfoAsync(string token)
    {
        var response = await _shareServiceClient.GetAsync($"api/share-tokens/{token}");
        if (!response.IsSuccessStatusCode) return null;

        var shareToken = await response.Content.ReadFromJsonAsync<ShareTokenInfo>();
        if (shareToken == null) return null;

        return new ShareLinkDto
        {
            TravelPlanId = shareToken.TravelPlanId,
            Token = shareToken.Token,
            AccessLevel = shareToken.AccessType,
            CreatedAt = shareToken.CreatedAt,
            ExpiresAt = shareToken.ExpiresAt
        };
    }

    public async Task<bool> DeleteShareLinkAsync(int planId, string token, int userId)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);
        if (plan == null) return false;

        var response = await _shareServiceClient.DeleteAsync($"api/share-tokens/{token}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ShareLinkDto>> GetShareLinksAsync(int planId, int userId)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);
        if (plan == null) return new List<ShareLinkDto>();

        var response = await _shareServiceClient.GetAsync($"api/share-tokens?planId={planId}");
        if (!response.IsSuccessStatusCode) return new List<ShareLinkDto>();

        var tokens = await response.Content.ReadFromJsonAsync<List<ShareTokenInfo>>();
        return tokens?.Select(t => new ShareLinkDto
        {
            TravelPlanId = t.TravelPlanId,
            Token = t.Token,
            AccessLevel = t.AccessType,
            CreatedAt = t.CreatedAt,
            ExpiresAt = t.ExpiresAt
        }).ToList() ?? new List<ShareLinkDto>();
    }

    public async Task<ShareLinkDto?> ValidateShareTokenAccessAsync(string token, string requiredAccess)
    {
        var response = await _shareServiceClient.GetAsync($"api/share-tokens/{token}");
        if (!response.IsSuccessStatusCode) return null;

        var shareToken = await response.Content.ReadFromJsonAsync<ShareTokenInfo>();
        if (shareToken == null) return null;

        if (requiredAccess == "EDIT" && shareToken.AccessType != "EDIT") return null;

        return new ShareLinkDto
        {
            TravelPlanId = shareToken.TravelPlanId,
            Token = shareToken.Token,
            AccessLevel = shareToken.AccessType,
            CreatedAt = shareToken.CreatedAt,
            ExpiresAt = shareToken.ExpiresAt
        };
    }

    public async Task<TravelPlanDto?> UpdatePlanByTokenAsync(int planId, UpdateTravelPlanDto dto)
    {
        var plan = await _context.TravelPlans
            .Include(p => p.Destinations)
            .Include(p => p.Activities)
            .Include(p => p.ChecklistItems)
            .FirstOrDefaultAsync(p => p.Id == planId);
        if (plan == null) return null;

        if (dto.Title != null) plan.Title = dto.Title;
        if (dto.Description != null) plan.Description = dto.Description;
        if (dto.StartDate.HasValue) plan.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) plan.EndDate = dto.EndDate.Value;
        if (dto.Budget.HasValue)
        {
            if (dto.Budget.Value < 0) throw new ArgumentException("Budget cannot be negative.");
            plan.Budget = dto.Budget.Value;
        }
        if (dto.Notes != null) plan.Notes = dto.Notes;

        if (plan.EndDate < plan.StartDate)
            throw new ArgumentException("End date cannot be before start date.");

        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return _mapper.Map<TravelPlanDto>(plan);
    }

    public async Task<DestinationDto> CreateDestinationByTokenAsync(int planId, CreateDestinationDto dto)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId);
        if (plan == null) throw new KeyNotFoundException("Travel plan not found.");

        if (dto.DepartureDate < dto.ArrivalDate)
            throw new ArgumentException("Departure date cannot be before arrival date.");

        var destination = _mapper.Map<Destination>(dto);
        destination.TravelPlanId = planId;
        _context.Destinations.Add(destination);
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return _mapper.Map<DestinationDto>(destination);
    }

    public async Task<DestinationDto?> UpdateDestinationByTokenAsync(int planId, int id, UpdateDestinationDto dto)
    {
        var dest = await _context.Destinations.FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == planId);
        if (dest == null) return null;

        if (dto.Name != null) dest.Name = dto.Name;
        if (dto.Location != null) dest.Location = dto.Location;
        if (dto.ArrivalDate.HasValue) dest.ArrivalDate = dto.ArrivalDate.Value;
        if (dto.DepartureDate.HasValue) dest.DepartureDate = dto.DepartureDate.Value;
        if (dto.Description != null) dest.Description = dto.Description;
        if (dto.Notes != null) dest.Notes = dto.Notes;

        if (dest.DepartureDate < dest.ArrivalDate)
            throw new ArgumentException("Departure date cannot be before arrival date.");

        await _context.SaveChangesAsync();
        return _mapper.Map<DestinationDto>(dest);
    }

    public async Task<bool> DeleteDestinationByTokenAsync(int planId, int id)
    {
        var dest = await _context.Destinations.FirstOrDefaultAsync(d => d.Id == id && d.TravelPlanId == planId);
        if (dest == null) return false;
        _context.Destinations.Remove(dest);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ActivityDto> CreateActivityByTokenAsync(int planId, CreateActivityDto dto)
    {
        if (!ActivityStatus.IsValid(dto.Status))
            throw new ArgumentException($"Invalid status. Allowed: {string.Join(", ", ActivityStatus.All)}");

        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId);
        if (plan == null) throw new KeyNotFoundException("Travel plan not found.");

        var activity = _mapper.Map<Activity>(dto);
        activity.TravelPlanId = planId;
        _context.Activities.Add(activity);
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return _mapper.Map<ActivityDto>(activity);
    }

    public async Task<ActivityDto?> UpdateActivityByTokenAsync(int planId, int id, UpdateActivityDto dto)
    {
        var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == planId);
        if (activity == null) return null;

        if (dto.Title != null) activity.Title = dto.Title;
        if (dto.Date.HasValue) activity.Date = dto.Date.Value;
        if (dto.Time.HasValue) activity.Time = dto.Time.Value;
        if (dto.Location != null) activity.Location = dto.Location;
        if (dto.Description != null) activity.Description = dto.Description;
        if (dto.EstimatedCost.HasValue) activity.EstimatedCost = dto.EstimatedCost.Value;
        if (dto.Status != null)
        {
            if (!ActivityStatus.IsValid(dto.Status))
                throw new ArgumentException($"Invalid status. Allowed: {string.Join(", ", ActivityStatus.All)}");
            activity.Status = dto.Status;
        }
        if (dto.DestinationId.HasValue) activity.DestinationId = dto.DestinationId;

        await _context.SaveChangesAsync();
        return _mapper.Map<ActivityDto>(activity);
    }

    public async Task<bool> DeleteActivityByTokenAsync(int planId, int id)
    {
        var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id && a.TravelPlanId == planId);
        if (activity == null) return false;
        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ChecklistItemDto> CreateChecklistItemByTokenAsync(int planId, CreateChecklistItemDto dto)
    {
        var plan = await _context.TravelPlans.FirstOrDefaultAsync(p => p.Id == planId);
        if (plan == null) throw new KeyNotFoundException("Travel plan not found.");

        var item = _mapper.Map<ChecklistItem>(dto);
        item.TravelPlanId = planId;
        _context.ChecklistItems.Add(item);
        await _context.SaveChangesAsync();
        return _mapper.Map<ChecklistItemDto>(item);
    }

    public async Task<ChecklistItemDto?> UpdateChecklistItemByTokenAsync(int planId, int id, UpdateChecklistItemDto dto)
    {
        var item = await _context.ChecklistItems.FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == planId);
        if (item == null) return null;

        if (dto.Title != null) item.Title = dto.Title;
        if (dto.IsCompleted.HasValue) item.IsCompleted = dto.IsCompleted.Value;

        await _context.SaveChangesAsync();
        return _mapper.Map<ChecklistItemDto>(item);
    }

    public async Task<bool> DeleteChecklistItemByTokenAsync(int planId, int id)
    {
        var item = await _context.ChecklistItems.FirstOrDefaultAsync(c => c.Id == id && c.TravelPlanId == planId);
        if (item == null) return false;
        _context.ChecklistItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    private record ShareTokenResponse(string Token);
    private record ShareTokenInfo(string Token, int TravelPlanId, string AccessType, DateTime CreatedAt, DateTime ExpiresAt);
}
