using CongestionTaxCalculator.Domain.Entities;
using CongestionTaxCalculator.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CongestionTaxCalculator.Infrastructure.Repositories;

public class TaxRuleRepository : ITaxRuleRepository
{
    private readonly CongestionTaxContext _context;
    private readonly ILogger<TaxRuleRepository> _logger;

    public TaxRuleRepository(CongestionTaxContext context, ILogger<TaxRuleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TaxRule>> GetAllTaxRulesByCityAsync(
        string city, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.TaxRules
                .AsNoTracking()
                .Where(r => r.City == city)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tax rules for city: {CityCode}", city);
            throw;
        }
    }
}