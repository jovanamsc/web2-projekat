using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelPlanner.Gateway.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class GatewayUsersController : ProxyControllerBase
{
    private readonly HttpClient _userClient;

    public GatewayUsersController(IHttpClientFactory factory)
    {
        _userClient = factory.CreateClient("UserService");
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser() =>
        await ForwardGet(_userClient, "api/users/me");

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers() =>
        await ForwardGet(_userClient, "api/users");

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUser(int id) =>
        await ForwardGet(_userClient, $"api/users/{id}");

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] object body) =>
        await ForwardPut(_userClient, $"api/users/{id}", body);

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id) =>
        await ForwardDelete(_userClient, $"api/users/{id}");
}
