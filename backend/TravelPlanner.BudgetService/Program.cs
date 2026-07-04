using System.Fabric;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.BudgetService.Data;
using TravelPlanner.BudgetService.Mapping;
using TravelPlanner.BudgetService.Services;
using TravelPlanner.Common.Interfaces;

internal sealed class BudgetService : StatelessService
{
    public BudgetService(StatelessServiceContext context) : base(context) { }

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

                    builder.Services.AddDbContext<BudgetDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("BudgetDb")));

                    builder.Services.AddScoped<IBudgetServiceContract, ExpenseService>();
                    builder.Services.AddAutoMapper(typeof(BudgetMappingProfile));

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
                        scope.ServiceProvider.GetRequiredService<BudgetDbContext>().Database.Migrate();

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
            ServiceRuntime.RegisterServiceAsync("BudgetServiceType",
                context => new BudgetService(context)).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }
}
