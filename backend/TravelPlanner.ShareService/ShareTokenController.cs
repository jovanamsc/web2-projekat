using Microsoft.AspNetCore.Mvc;

namespace TravelPlanner.ShareService;

[ApiController]
[Route("api/share-tokens")]
public class ShareTokenController : ControllerBase
{
    private readonly ShareService _shareService;

    public ShareTokenController(ShareService shareService)
    {
        _shareService = shareService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShareTokenRequest request)
    {
        var token = await _shareService.CreateShareTokenAsync(
            request.TravelPlanId,
            request.AccessType,
            TimeSpan.FromDays(request.ExpiryDays));

        return Ok(new { token });
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> Validate(string token)
    {
        var result = await _shareService.ValidateTokenAsync(token);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetByPlan([FromQuery] int planId)
    {
        var tokens = await _shareService.GetTokensByPlanAsync(planId);
        return Ok(tokens);
    }

    [HttpDelete("{token}")]
    public async Task<IActionResult> Revoke(string token)
    {
        await _shareService.RevokeTokenAsync(token);
        return NoContent();
    }
}

public record CreateShareTokenRequest(int TravelPlanId, string AccessType, int ExpiryDays = 7);
