using System.Fabric;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

internal sealed class GatewayService : StatelessService
{
    public GatewayService(StatelessServiceContext context) : base(context) { }

    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() =>
        new ServiceInstanceListener[]
        {
            new(serviceContext =>
                new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                {
                    var builder = WebApplication.CreateBuilder();
                    builder.Configuration
                        .AddJsonFile("appsettings.json", optional: false)
                        .AddJsonFile("appsettings.Local.json", optional: true);
                    builder.WebHost.UseUrls(url);
                    builder.WebHost.UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None);

                    builder.Services.AddSingleton(serviceContext);
                    builder.Services.AddControllers();

                    builder.Services.AddHttpClient("UserService", client =>
                        client.BaseAddress = new Uri(builder.Configuration["Services:UserService"]!));
                    builder.Services.AddHttpClient("TravelService", client =>
                        client.BaseAddress = new Uri(builder.Configuration["Services:TravelService"]!));
                    builder.Services.AddHttpClient("BudgetService", client =>
                        client.BaseAddress = new Uri(builder.Configuration["Services:BudgetService"]!));

                    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                                ValidateIssuer = true,
                                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                                ValidateAudience = true,
                                ValidAudience = builder.Configuration["Jwt:Audience"],
                                ValidateLifetime = true,
                                ClockSkew = TimeSpan.Zero
                            };
                        });

                    builder.Services.AddAuthorization();

                    builder.Services.AddCors(options =>
                        options.AddPolicy("AllowFrontend", policy =>
                            policy.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(",") ?? ["http://localhost:5173"])
                                .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

                    var app = builder.Build();

                    app.UseCors("AllowFrontend");
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.MapControllers();

                    return app;
                }))
        };
}

internal static class Program
{
    private static void Main()
    {
        try
        {
            ServiceRuntime.RegisterServiceAsync("GatewayServiceType",
                context => new GatewayService(context)).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }
}
