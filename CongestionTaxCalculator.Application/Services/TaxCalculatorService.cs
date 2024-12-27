using System.ComponentModel.DataAnnotations;
using CongestionTaxCalculator.Application.DTOs;
using CongestionTaxCalculator.Application.Interfaces;
using CongestionTaxCalculator.Domain.Entities;
using CongestionTaxCalculator.Domain.Exceptions;
using CongestionTaxCalculator.Domain.Repositories;
using CongestionTaxCalculator.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CongestionTaxCalculator.Application.Services;

public class TaxCalculatorService : ITaxCalculatorService
{
    private const decimal MaxDailyCharge = 60;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITaxRuleRepository _taxRuleRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<TaxCalculatorService> _logger;

    public TaxCalculatorService(
        IUnitOfWork unitOfWork,
        ITaxRuleRepository taxRuleRepository,
        ICacheService cacheService,
        ILogger<TaxCalculatorService> logger)
    {
        _taxRuleRepository = taxRuleRepository;
        _cacheService = cacheService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaxCalculationResponse> CalculateTax(
        TaxCalculationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate vehicle type
        // TODO: Result pattern is better here than exceptions, also you can add IsNullOrWhiteSpace
        if (string.IsNullOrEmpty(request.VehicleType) ||
            !Enum.TryParse(request.VehicleType, true, out VehicleType vehicle))
        {
            throw new ArgumentException($"Invalid vehicle type: {request.VehicleType}");
        }

        // Validate city
        if (string.IsNullOrEmpty(request.City))
        {
            throw new ValidationException("City is required");
        }
        
        try
        {
            var taxRules = await GetTaxRulesAsync(request.City, cancellationToken);

            var taxRulesList = taxRules.ToList();
            if (taxRules == null || taxRulesList.Count == 0)
            {
                throw new ApplicationException("No tax rules found for the specified city.");
            }
            
            TaxCalculationResponse taxCalculationResponse;
            if (Enum.TryParse(request.VehicleType, true, out VehicleType vehicleType))
            {
                // Create the response with valid vehicle type
                taxCalculationResponse = new TaxCalculationResponse
                {
                    Summary = new TaxSummary(),
                    Details = new TaxCalculationDetails
                    {
                        VehicleType = vehicleType, 
                        IsTollFreeVehicle = IsTollFreeVehicle(vehicleType),
                    }
                };
            }
            else
            {
                throw new ArgumentException($"Invalid vehicle type: {request.VehicleType}");
            }

            if (taxCalculationResponse.Details.IsTollFreeVehicle)
            {
                var monthlyCharge = new MonthlyCharge
                {
                    Year = DateTime.Now.Year,
                    Month = DateTime.Now.Month,
                    DailyCharges = new List<DailyCharge>(),
                    TotalTaxMonth = 0 
                };
                
                taxCalculationResponse.Summary.MonthlyCharges.Add(monthlyCharge);
                
                return taxCalculationResponse;
            }

            // Group passages by year, month, and day
            var passagesByMonth = request.Passages
                .OrderBy(p => p)
                .GroupBy(p => new { p.Year, p.Month });

            foreach (var monthGroup in passagesByMonth)
            {
                var monthlyCharge = new MonthlyCharge
                {
                    Year = monthGroup.Key.Year,
                    Month = monthGroup.Key.Month
                };

                var passagesByDay = monthGroup.GroupBy(p => p.Date);

                foreach (var dayGroup in passagesByDay)
                {
                    var dailyCharge = await CalculateDailyCharge(dayGroup.ToList(), taxRulesList);
                    monthlyCharge.DailyCharges.Add(dailyCharge);
                    dailyCharge.IsTollFreeDate = IsTollFreeDate(dayGroup.Key);
                    monthlyCharge.TotalTaxMonth += dailyCharge.DayTotalTax;
                }

                taxCalculationResponse.Summary.MonthlyCharges.Add(monthlyCharge);
                taxCalculationResponse.Summary.TotalTaxYear += monthlyCharge.TotalTaxMonth;
            }

            return taxCalculationResponse;
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Domain error calculating tax");
            throw;
        }
        catch (ApplicationException ex) 
        {
            _logger.LogError(ex, "Application error calculating tax");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calculating tax");
            throw new ApplicationException("Error calculating tax", ex);
        }
    }

        private async Task<DailyCharge> CalculateDailyCharge(List<DateTime> passages, IEnumerable<TaxRule> taxRules)
        {
            var dailyCharge = new DailyCharge
            {
                Date = passages.First().Date,
                Intervals = new List<IntervalDetail>()
            };

            if (IsTollFreeDate(passages.First()))
            {
                return dailyCharge;
            }

            var taxRulesList = taxRules.ToList();
            var currentInterval = new IntervalDetail
            {
                Times = new List<DateTime> { passages[0] },
                Amount = GetTaxAmount(passages[0].TimeOfDay, taxRulesList)
            };

            foreach (var passage in passages.Skip(1))
            {
                var currentAmount = GetTaxAmount(passage.TimeOfDay , taxRulesList);

                if (passage <= currentInterval.Times[0].AddMinutes(60))
                {
                    // Within 60 minutes, add to current interval
                    currentInterval.Times.Add(passage);
                    currentInterval.Amount = Math.Max(currentInterval.Amount, currentAmount);
                }
                else
                {
                    // New interval starts
                    dailyCharge.Intervals.Add(currentInterval);
                    
                    // Creates a new interval and adds the new passage as the first item in the time
                    // list for currentInterval.Times[0]
                    currentInterval = new IntervalDetail
                    {
                        Times = new List<DateTime> { passage },
                        Amount = currentAmount
                    };
                }
            }

            // Add the last interval
            dailyCharge.Intervals.Add(currentInterval);

            // Calculate daily totals
            var originalAmount = dailyCharge.Intervals.Sum(i => i.Amount);
            dailyCharge.DayTotalTax = Math.Min(originalAmount, MaxDailyCharge);
            
            if (originalAmount > MaxDailyCharge)
            {
                dailyCharge.MaxDailyChargeApplied = true;
                dailyCharge.OriginalAmount = originalAmount;
            }

            return dailyCharge;
        }

    private bool IsTollFreeVehicle(VehicleType vehicleType)
    {
        return vehicleType switch
        {
            VehicleType.Emergency => true,
            VehicleType.Bus => true,
            VehicleType.Diplomat => true,
            VehicleType.Motorcycle => true,
            VehicleType.Military => true,
            VehicleType.Foreign => true,
            VehicleType.Car => false,
            _ => throw new ArgumentException($"Unknown vehicle type: {vehicleType}")
        };
    }

    private bool IsTollFreeDate(DateTime date, int year = 2013)
    {
        // Check if the date is on a weekend
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return true;

        // Define toll-free dates for the specified year
        var tollFreeDates = new HashSet<DateTime>
        {
            new DateTime(year, 1, 1),  
            new DateTime(year, 3, 28),
            new DateTime(year, 3, 29),
            new DateTime(year, 4, 1), 
            new DateTime(year, 4, 30),
            new DateTime(year, 5, 1), 
            new DateTime(year, 5, 8), 
            new DateTime(year, 5, 9), 
            new DateTime(year, 6, 5), 
            new DateTime(year, 6, 6), 
            new DateTime(year, 6, 21),
            new DateTime(year, 11, 1), 
            new DateTime(year, 12, 24),
            new DateTime(year, 12, 25),
            new DateTime(year, 12, 26),
            new DateTime(year, 12, 31),
        };

        // Check if the date is in July (always toll-free)
        if (date.Month == 7)
            return true;

        // Check if the day before the public holidays is toll-free
        foreach (var tollFreeDate in tollFreeDates)
        {
            if (date == tollFreeDate.AddDays(-1))
                return true;
        }

        // Return true if the date is a defined toll-free date
        return tollFreeDates.Contains(date);
    }

    private decimal GetTaxAmount(TimeSpan time, IEnumerable<TaxRule> taxRules)
    {
        var rate = taxRules.FirstOrDefault(tr => time >= tr.StartTime && time <= tr.EndTime);
        return rate?.Amount ?? 0; // Return 0 if no rate found
    }

    private async Task<IEnumerable<TaxRule>> GetTaxRulesAsync(string cityCode, CancellationToken cancellationToken)
    {
        var taxRules = await _cacheService.GetOrSetAsync(
            $"tax_rules_{cityCode}",
            () => _unitOfWork.TaxRules.GetAllTaxRulesByCityAsync(cityCode, cancellationToken),
            TimeSpan.FromHours(1),
            cancellationToken);
        
        return taxRules ?? Enumerable.Empty<TaxRule>();
    }
}