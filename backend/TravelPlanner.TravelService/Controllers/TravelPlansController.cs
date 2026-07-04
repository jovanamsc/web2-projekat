using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelPlanner.Common.DTOs;
using TravelPlanner.Common.Interfaces;

namespace TravelPlanner.TravelService.Controllers;

[ApiController]
[Route("api/travel-plans")]
[Authorize]
public class TravelPlansController : ControllerBase
{
    private readonly ITravelServiceContract _travelService;

    public TravelPlansController(ITravelServiceContract travelService)
    {
        _travelService = travelService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TravelPlanDto>>> GetAll()
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var plans = role == "Admin"
            ? await _travelService.GetAllPlansAdminAsync()
            : await _travelService.GetAllPlansAsync(userId);

        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TravelPlanDto>> GetById(int id)
    {
        var userId = GetUserId();
        var plan = await _travelService.GetPlanByIdAsync(id, userId);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<TravelPlanDto>> Create([FromBody] CreateTravelPlanDto dto)
    {
        try
        {
            var userId = GetUserId();
            var plan = await _travelService.CreatePlanAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TravelPlanDto>> Update(int id, [FromBody] UpdateTravelPlanDto dto)
    {
        try
        {
            var userId = GetUserId();
            var plan = await _travelService.UpdatePlanAsync(id, userId, dto);
            if (plan == null) return NotFound();
            return Ok(plan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var result = await _travelService.DeletePlanAsync(id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    // Destinations
    [HttpGet("{planId}/destinations")]
    public async Task<ActionResult<List<DestinationDto>>> GetDestinations(int planId)
    {
        var destinations = await _travelService.GetDestinationsAsync(planId);
        return Ok(destinations);
    }

    [HttpGet("{planId}/destinations/{id}")]
    public async Task<ActionResult<DestinationDto>> GetDestination(int planId, int id)
    {
        var dest = await _travelService.GetDestinationByIdAsync(planId, id);
        if (dest == null) return NotFound();
        return Ok(dest);
    }

    [HttpPost("{planId}/destinations")]
    public async Task<ActionResult<DestinationDto>> CreateDestination(int planId, [FromBody] CreateDestinationDto dto)
    {
        try
        {
            var userId = GetUserId();
            var dest = await _travelService.CreateDestinationAsync(planId, userId, dto);
            return CreatedAtAction(nameof(GetDestination), new { planId, id = dest.Id }, dest);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Travel plan not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{planId}/destinations/{id}")]
    public async Task<ActionResult<DestinationDto>> UpdateDestination(int planId, int id, [FromBody] UpdateDestinationDto dto)
    {
        try
        {
            var userId = GetUserId();
            var dest = await _travelService.UpdateDestinationAsync(planId, id, userId, dto);
            if (dest == null) return NotFound();
            return Ok(dest);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{planId}/destinations/{id}")]
    public async Task<IActionResult> DeleteDestination(int planId, int id)
    {
        var userId = GetUserId();
        var result = await _travelService.DeleteDestinationAsync(planId, id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    // Activities
    [HttpGet("{planId}/activities")]
    public async Task<ActionResult<List<ActivityDto>>> GetActivities(int planId)
    {
        var activities = await _travelService.GetActivitiesAsync(planId);
        return Ok(activities);
    }

    [HttpGet("{planId}/activities/{id}")]
    public async Task<ActionResult<ActivityDto>> GetActivity(int planId, int id)
    {
        var activity = await _travelService.GetActivityByIdAsync(planId, id);
        if (activity == null) return NotFound();
        return Ok(activity);
    }

    [HttpPost("{planId}/activities")]
    public async Task<ActionResult<ActivityDto>> CreateActivity(int planId, [FromBody] CreateActivityDto dto)
    {
        try
        {
            var userId = GetUserId();
            var activity = await _travelService.CreateActivityAsync(planId, userId, dto);
            return CreatedAtAction(nameof(GetActivity), new { planId, id = activity.Id }, activity);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Travel plan not found." });
        }
    }

    [HttpPut("{planId}/activities/{id}")]
    public async Task<ActionResult<ActivityDto>> UpdateActivity(int planId, int id, [FromBody] UpdateActivityDto dto)
    {
        var userId = GetUserId();
        var activity = await _travelService.UpdateActivityAsync(planId, id, userId, dto);
        if (activity == null) return NotFound();
        return Ok(activity);
    }

    [HttpDelete("{planId}/activities/{id}")]
    public async Task<IActionResult> DeleteActivity(int planId, int id)
    {
        var userId = GetUserId();
        var result = await _travelService.DeleteActivityAsync(planId, id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    // Checklist
    [HttpGet("{planId}/checklist")]
    public async Task<ActionResult<List<ChecklistItemDto>>> GetChecklist(int planId)
    {
        var items = await _travelService.GetChecklistItemsAsync(planId);
        return Ok(items);
    }

    [HttpPost("{planId}/checklist")]
    public async Task<ActionResult<ChecklistItemDto>> CreateChecklistItem(int planId, [FromBody] CreateChecklistItemDto dto)
    {
        try
        {
            var userId = GetUserId();
            var item = await _travelService.CreateChecklistItemAsync(planId, userId, dto);
            return Created($"api/travel-plans/{planId}/checklist/{item.Id}", item);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Travel plan not found." });
        }
    }

    [HttpPut("{planId}/checklist/{id}")]
    public async Task<ActionResult<ChecklistItemDto>> UpdateChecklistItem(int planId, int id, [FromBody] UpdateChecklistItemDto dto)
    {
        var userId = GetUserId();
        var item = await _travelService.UpdateChecklistItemAsync(planId, id, userId, dto);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpDelete("{planId}/checklist/{id}")]
    public async Task<IActionResult> DeleteChecklistItem(int planId, int id)
    {
        var userId = GetUserId();
        var result = await _travelService.DeleteChecklistItemAsync(planId, id, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    // Sharing
    [HttpPost("{planId}/share")]
    public async Task<ActionResult<ShareLinkDto>> CreateShareLink(int planId, [FromBody] CreateShareLinkDto dto)
    {
        try
        {
            var userId = GetUserId();
            var link = await _travelService.CreateShareLinkAsync(planId, userId, dto);
            return Created($"api/travel-plans/shared/{link.Token}", link);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Travel plan not found." });
        }
    }

    [HttpGet("shared/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<TravelPlanDto>> GetSharedPlan(string token)
    {
        var plan = await _travelService.GetPlanByShareTokenAsync(token);
        if (plan == null) return NotFound(new { message = "Share link is invalid or expired." });
        return Ok(plan);
    }

    [HttpGet("shared/{token}/info")]
    [AllowAnonymous]
    public async Task<ActionResult<ShareLinkDto>> GetShareLinkInfo(string token)
    {
        var info = await _travelService.GetShareLinkInfoAsync(token);
        if (info == null) return NotFound(new { message = "Share link is invalid or expired." });
        return Ok(info);
    }

    [HttpGet("{planId}/share")]
    public async Task<ActionResult<List<ShareLinkDto>>> GetShareLinks(int planId)
    {
        var userId = GetUserId();
        var links = await _travelService.GetShareLinksAsync(planId, userId);
        return Ok(links);
    }

    [HttpDelete("{planId}/share/{token}")]
    public async Task<IActionResult> DeleteShareLink(int planId, string token)
    {
        var userId = GetUserId();
        var result = await _travelService.DeleteShareLinkAsync(planId, token, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private string GetUserRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
}
