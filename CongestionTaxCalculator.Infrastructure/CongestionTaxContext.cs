using CongestionTaxCalculator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CongestionTaxCalculator.Infrastructure;

public class CongestionTaxContext : DbContext
{
    public CongestionTaxContext(DbContextOptions<CongestionTaxContext> options)
        : base(options)
    {
    }
    public DbSet<TaxRule> TaxRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed tax rules
        modelBuilder.Entity<TaxRule>().HasData(
            new TaxRule { Id = 1, StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(6, 29, 59), Amount = 8 , City = "Gothenburg"},
            new TaxRule { Id = 2, StartTime = new TimeSpan(6, 30, 0), EndTime = new TimeSpan(6, 59, 59), Amount = 13, City = "Gothenburg" },
            new TaxRule { Id = 3, StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(7, 59, 59), Amount = 18, City = "Gothenburg" },
            new TaxRule { Id = 4, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 29, 59), Amount = 13, City = "Gothenburg" },
            new TaxRule { Id = 5, StartTime = new TimeSpan(8, 30, 0), EndTime = new TimeSpan(14, 59, 59), Amount = 8, City = "Gothenburg" },
            new TaxRule { Id = 6, StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(15, 29, 59), Amount = 13, City = "Gothenburg" },
            new TaxRule { Id = 7, StartTime = new TimeSpan(15, 30, 0), EndTime = new TimeSpan(16, 59, 59), Amount = 18, City = "Gothenburg" },
            new TaxRule { Id = 8, StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(17, 59, 59), Amount = 13, City = "Gothenburg" },
            new TaxRule { Id = 9, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(18, 29, 59), Amount = 8, City = "Gothenburg" }
        );
    }
}