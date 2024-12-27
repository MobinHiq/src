using CongestionTaxCalculator.Application.DTOs;

namespace CongestionTaxCalculator.Application.Interfaces;

public interface ITaxCalculatorService
{
    Task<TaxCalculationResponse> CalculateTax(
        TaxCalculationRequest request, CancellationToken cancellationToken = default);
}