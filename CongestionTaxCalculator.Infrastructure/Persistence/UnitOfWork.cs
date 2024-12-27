using CongestionTaxCalculator.Domain.Repositories;

namespace CongestionTaxCalculator.Infrastructure.Persistence;

// The idea was to create a single point of access for repositories and transactions
// This is just for "DDD" combination with "Clean Architecture" but we have only TaxRule
// and vehicles as entities. This can help the scalability of the application. 
public class UnitOfWork : IUnitOfWork
{
    private readonly CongestionTaxContext _context;
    public ITaxRuleRepository TaxRules { get; }

    public UnitOfWork(
        CongestionTaxContext context,
        ITaxRuleRepository taxRules)
    {
        _context = context;
        TaxRules = taxRules;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}