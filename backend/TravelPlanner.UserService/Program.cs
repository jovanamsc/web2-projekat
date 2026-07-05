using System.Fabric;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using TravelPlanner.Common.Interfaces;
using TravelPlanner.UserService.Data;
using TravelPlanner.UserService.Mapping;
using TravelPlanner.UserService.Services;

internal sealed class UserService : StatelessService
{
    public UserService(StatelessServiceContext context) : base(context) { }

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

                    builder.Services.AddDbContext<UserDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("UserDb")));

                    builder.Services.AddScoped<IUserServiceContract, AuthService>();
                    builder.Services.AddAutoMapper(typeof(UserMappingProfile));

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
                    {
                        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                        db.Database.Migrate();

                        if (!db.Users.Any(u => u.Role == "Admin"))
                        {
                            var adminEmail = app.Configuration["AdminSeed:Email"]!;
                            var adminPassword = app.Configuration["AdminSeed:Password"]!;
                            db.Users.Add(new TravelPlanner.UserService.Models.User
                            {
                                FirstName = "Admin",
                                LastName = "TravelPlanner",
                                Email = adminEmail,
                                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                                Role = "Admin"
                            });
                            db.SaveChanges();
                        }
                    }

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
            ServiceRuntime.RegisterServiceAsync("UserServiceType",
                context => new UserService(context)).GetAwaiter().GetResult();
            Thread.Sleep(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }
}
