using System.Fabric;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Common.Interfaces;
using TravelPlanner.TravelService.Data;
using TravelPlanner.TravelService.Mapping;
using TravelPlanner.TravelService.Services;

internal sealed class TravelService : StatelessService
{
    public TravelService(StatelessServiceContext context) : base(context) { }

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

                    builder.Services.AddDbContext<TravelDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("TravelDb")));

                    builder.Services.AddHttpClient("ShareService", client =>
                        client.BaseAddress = new Uri(builder.Configuration["Services:ShareService"]!));

                    builder.Services.AddScoped<ITravelServiceContract, TravelPlanService>();
                    builder.Services.AddAutoMapper(typeof(TravelMappingProfile));

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

                    using (var scope = app.Services.CreateScope())
                        scope.ServiceProvider.GetRequiredService<TravelDbContext>().Database.Migrate();

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
            ServiceRuntime.RegisterServiceAsync("TravelServiceType",
                context => new TravelService(context)).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }
}
