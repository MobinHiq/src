using CongestionTaxCalculator.Domain.Entities;

namespace CongestionTaxCalculator.Domain.Repositories;

public interface ITaxRuleRepository
{
    Task<List<TaxRule>> GetAllTaxRulesByCityAsync(
        string cityCode, CancellationToken cancellationToken = default);
}