using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TravelPlanner.TravelService.Data;

public class TravelDbContextFactory : IDesignTimeDbContextFactory<TravelDbContext>
{
    public TravelDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Local.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<TravelDbContext>();
        optionsBuilder.UseSqlServer(config.GetConnectionString("TravelDb"));

        return new TravelDbContext(optionsBuilder.Options);
    }
}
