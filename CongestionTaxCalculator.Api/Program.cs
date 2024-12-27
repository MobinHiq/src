using CongestionTaxCalculator.Api.Middleware;
using CongestionTaxCalculator.Application.Interfaces;
using CongestionTaxCalculator.Application.Services;
using CongestionTaxCalculator.Domain.Entities;
using CongestionTaxCalculator.Domain.Repositories;
using CongestionTaxCalculator.Infrastructure;
using CongestionTaxCalculator.Infrastructure.Persistence;
using CongestionTaxCalculator.Infrastructure.Repositories;
using CongestionTaxCalculator.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CongestionTaxCalculator.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure Serilog for logging
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/tax-calculator-.txt", 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7));
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<CongestionTaxContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<ICacheService, CacheService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<ITaxRuleRepository, TaxRuleRepository>();
        builder.Services.AddScoped<ITaxCalculatorService, TaxCalculatorService>();

        var app = builder.Build();
        
        // Configure middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapControllers();
        
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CongestionTaxContext>();
            await context.Database.MigrateAsync(); 
            await SeedData(context); 
            Log.Information("Database migration completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the database");
            throw; 
        }
        
        await app.RunAsync(); 
        
        async Task SeedData(CongestionTaxContext context)
        {
            if (!await context.TaxRules.AnyAsync())
            {
                var taxRules = new List<TaxRule>
                {
                    new TaxRule { Id = 1, StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(6, 29, 59), Amount = 8, City = "Gothenburg" },
                    new TaxRule { Id = 2, StartTime = new TimeSpan(6, 30, 0), EndTime = new TimeSpan(6, 59, 59), Amount = 13, City = "Gothenburg" },
                    new TaxRule { Id = 3, StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(7, 59, 59), Amount = 18, City = "Gothenburg" },
                    new TaxRule { Id = 4, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 29, 59), Amount = 13, City = "Gothenburg" },
                    new TaxRule { Id = 5, StartTime = new TimeSpan(8, 30, 0), EndTime = new TimeSpan(14, 59, 59), Amount = 8, City = "Gothenburg" },
                    new TaxRule { Id = 6, StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(15, 29, 59), Amount = 13, City = "Gothenburg" },
                    new TaxRule { Id = 7, StartTime = new TimeSpan(15, 30, 0), EndTime = new TimeSpan(16, 59, 59), Amount = 18, City = "Gothenburg" },
                    new TaxRule { Id = 8, StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(17, 59, 59), Amount = 13, City = "Gothenburg" },
                    new TaxRule { Id = 9, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(18, 29, 59), Amount = 8, City = "Gothenburg" }
                };

                await context.TaxRules.AddRangeAsync(taxRules);
                await context.SaveChangesAsync(); 
                Log.Information("Seeded tax rules");
            }
        }
    }
}
