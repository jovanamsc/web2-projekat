using Microsoft.EntityFrameworkCore;
using TravelPlanner.BudgetService.Models;

namespace TravelPlanner.BudgetService.Data;

public class BudgetDbContext : DbContext
{
    public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options) { }

    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasIndex(e => e.TravelPlanId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Category);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
