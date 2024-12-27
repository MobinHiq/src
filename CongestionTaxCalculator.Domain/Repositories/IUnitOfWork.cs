namespace CongestionTaxCalculator.Domain.Repositories;

public interface IUnitOfWork
{
    ITaxRuleRepository TaxRules { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}