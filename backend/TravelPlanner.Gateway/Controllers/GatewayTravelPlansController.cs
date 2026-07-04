using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelPlanner.Gateway.Controllers;

[ApiController]
[Route("api/travel-plans")]
[Authorize]
public class GatewayTravelPlansController : ProxyControllerBase
{
    private readonly HttpClient _travelClient;
    private readonly HttpClient _budgetClient;

    public GatewayTravelPlansController(IHttpClientFactory factory)
    {
        _travelClient = factory.CreateClient("TravelService");
        _budgetClient = factory.CreateClient("BudgetService");
    }

    // Planovi putovanja
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        await ForwardGet(_travelClient, "api/travel-plans");

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) =>
        await ForwardGet(_travelClient, $"api/travel-plans/{id}");

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object body) =>
        await ForwardPost(_travelClient, "api/travel-plans", body);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] object body) =>
        await ForwardPut(_travelClient, $"api/travel-plans/{id}", body);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await ForwardDelete(_travelClient, $"api/travel-plans/{id}");

    // Destinacije
    [HttpGet("{planId}/destinations")]
    public async Task<IActionResult> GetDestinations(int planId) =>
        await ForwardGet(_travelClient, $"api/travel-plans/{planId}/destinations");

    [HttpGet("{planId}/destinations/{id}")]
    public async Task<IActionResult> GetDestination(int planId, int id) =>
        await ForwardGet(_travelClient, $"api/travel-plans/{planId}/destinations/{id}");

    [HttpPost("{planId}/destinations")]
    public async Task<IActionResult> CreateDestination(int planId, [FromBody] object body) =>
        await ForwardPost(_travelClient, $"api/travel-plans/{planId}/destinations", body);

    [HttpPut("{planId}/destinations/{id}")]
    public async Task<IActionResult> UpdateDestination(int planId, int id, [FromBody] object body) =>
        await ForwardPut(_travelClient, $"api/travel-plans/{planId}/destinations/{id}", body);

    [HttpDelete("{planId}/destinations/{id}")]
    public async Task<IActionResult> DeleteDestination(int planId, int id) =>
        await ForwardDelete(_travelClient, $"api/travel-plans/{planId}/destinations/{id}");

    // Aktivnosti
    [HttpGet("{planId}/activities")]
    public async Task<IActionResult> GetActivities(int planId) =>
        await ForwardGet(_travelClient, $"api/travel-plans/{planId}/activities");

    [HttpGet("{planId}/activities/{id}")]
    public async Task<IActionResult> GetActivity(int planId, int id) =>
        await ForwardGet(_travelClient, $"api/travel-plans/{planId}/activities/{id}");

    [HttpPost("{planId}/activities")]
    public async Task<IActionResult> CreateActivity(int planId, [FromBody] object body) =>
        await ForwardPost(_travelClient, $"api/travel-plans/{planId}/activities", body);

    [HttpPut("{planId}/activities/{id}")]
    public async Task<IActionResult> UpdateActivity(int planId, int id, [FromBody] object body) =>
        await ForwardPut(_travelClient, $"api/travel-plans/{planId}/activities/{id}", body);

    [HttpDelete("{planId}/activities/{id}")]
    public async Task<IActionResult> DeleteActivity(int planId, int id) =>
        await ForwardDelete(_travelClient, $"api/travel-plans/{planId}/activities/{id}");

    // Ceklista
    [HttpGet("{planId}/checklist")]
    public async Task<IActionResult> GetChecklist(int planId) =>
        await ForwardGet(_travelClient, $"api/travel-plans/{planId}/checklist");

    [HttpPost("{planId}/checklist")]
    public async Task<IActionResult> CreateChecklistItem(int planId, [FromBody] object body) =>
        await ForwardPost(_travelClient, $"api/travel-plans/{planId}/checklist", body);

    [HttpPut("{planId}/checklist/{id}")]
    public async Task<IActionResult> UpdateChecklistItem(int planId, int id, [FromBody] object body) =>
        await ForwardPut(_travelClient, $"api/travel-plans/{planId}/checklist/{id}", body);

    [HttpDelete("{planId}/checklist/{id}")]
    public async Task<IActionResult> DeleteChecklistItem(int planId, int id) =>
        await ForwardDelete(_travelClient, $"api/travel-plans/{planId}/checklist/{id}");

    // Dijeljenje
    [HttpPost("{planId}/share")]
    public async Task<IActionResult> CreateShareLink(int planId, [FromBody] object body) =>
        await ForwardPost(_travelClient, $"api/travel-plans/{planId}/share", body);

    [HttpGet("shared/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSharedPlan(string token) =>
        await ForwardGet(_travelClient, $"api/travel-plans/shared/{token}");

    [HttpGet("shared/{token}/info")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShareLinkInfo(string token) =>
        await ForwardGet(_travelClient, $"api/travel-plans/shared/{token}/info");

    [HttpGet("{planId}/share")]
    public async Task<IActionResult> GetShareLinks(int planId) =>
        await ForwardGet(_travelClient, $"api/travel-plans/{planId}/share");

    [HttpDelete("{planId}/share/{id}")]
    public async Task<IActionResult> DeleteShareLink(int planId, int id) =>
        await ForwardDelete(_travelClient, $"api/travel-plans/{planId}/share/{id}");

    // Troskovi
    [HttpGet("{planId}/expenses")]
    public async Task<IActionResult> GetExpenses(int planId) =>
        await ForwardGet(_budgetClient, $"api/travel-plans/{planId}/expenses");

    [HttpGet("{planId}/expenses/{id}")]
    public async Task<IActionResult> GetExpense(int planId, int id) =>
        await ForwardGet(_budgetClient, $"api/travel-plans/{planId}/expenses/{id}");

    [HttpPost("{planId}/expenses")]
    public async Task<IActionResult> CreateExpense(int planId, [FromBody] object body) =>
        await ForwardPost(_budgetClient, $"api/travel-plans/{planId}/expenses", body);

    [HttpPut("{planId}/expenses/{id}")]
    public async Task<IActionResult> UpdateExpense(int planId, int id, [FromBody] object body) =>
        await ForwardPut(_budgetClient, $"api/travel-plans/{planId}/expenses/{id}", body);

    [HttpDelete("{planId}/expenses/{id}")]
    public async Task<IActionResult> DeleteExpense(int planId, int id) =>
        await ForwardDelete(_budgetClient, $"api/travel-plans/{planId}/expenses/{id}");

    [HttpGet("{planId}/expenses/summary")]
    public async Task<IActionResult> GetBudgetSummary(int planId, [FromQuery] decimal plannedBudget = 0) =>
        await ForwardGet(_budgetClient, $"api/travel-plans/{planId}/expenses/summary?plannedBudget={plannedBudget}");
}
