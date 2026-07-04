using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TravelPlanner.Gateway.Controllers;

public abstract class ProxyControllerBase : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    protected async Task<IActionResult> ForwardGet(HttpClient client, string path)
    {
        var request = CreateRequest(HttpMethod.Get, path);
        return await SendAndReturn(client, request);
    }

    protected async Task<IActionResult> ForwardPost(HttpClient client, string path, object? body = null)
    {
        var request = CreateRequest(HttpMethod.Post, path, body);
        return await SendAndReturn(client, request);
    }

    protected async Task<IActionResult> ForwardPut(HttpClient client, string path, object? body = null)
    {
        var request = CreateRequest(HttpMethod.Put, path, body);
        return await SendAndReturn(client, request);
    }

    protected async Task<IActionResult> ForwardDelete(HttpClient client, string path)
    {
        var request = CreateRequest(HttpMethod.Delete, path);
        return await SendAndReturn(client, request);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);

        if (HttpContext.Request.Headers.TryGetValue("Authorization", out var auth))
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(auth.ToString());

        if (body != null)
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");

        return request;
    }

    private async Task<IActionResult> SendAndReturn(HttpClient client, HttpRequestMessage request)
    {
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode,
            string.IsNullOrEmpty(content) ? null : JsonSerializer.Deserialize<JsonElement>(content));
    }
}
