using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelPlanner.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class GatewayAuthController : ProxyControllerBase
{
    private readonly HttpClient _userClient;

    public GatewayAuthController(IHttpClientFactory factory)
    {
        _userClient = factory.CreateClient("UserService");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] object body) =>
        await ForwardPost(_userClient, "api/auth/register", body);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] object body) =>
        await ForwardPost(_userClient, "api/auth/login", body);
}
