using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TravelPlanner.BudgetService.Data;

public class BudgetDbContextFactory : IDesignTimeDbContextFactory<BudgetDbContext>
{
    public BudgetDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Local.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BudgetDbContext>();
        optionsBuilder.UseSqlServer(config.GetConnectionString("BudgetDb"));

        return new BudgetDbContext(optionsBuilder.Options);
    }
}
