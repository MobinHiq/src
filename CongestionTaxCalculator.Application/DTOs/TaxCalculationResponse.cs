using CongestionTaxCalculator.Domain.Entities;

namespace CongestionTaxCalculator.Application.DTOs;

public class TaxCalculationResponse
{
    public TaxSummary Summary { get; set; } = new();
    public TaxCalculationDetails Details { get; set; } = new();
}

public class TaxSummary
{
    public decimal TotalTaxYear { get; set; }
    public List<MonthlyCharge> MonthlyCharges { get; set; } = new();
}

public class MonthlyCharge
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalTaxMonth { get; set; }
    public List<DailyCharge> DailyCharges { get; set; } = new();
}

public class DailyCharge
{
    public DateTime Date { get; set; }
    
    public bool IsTollFreeDate { get; set; }
    public decimal DayTotalTax { get; set; }
    public bool MaxDailyChargeApplied { get; set; }
    public decimal? OriginalAmount { get; set; }
    public List<IntervalDetail> Intervals { get; set; } = new();
    
}

public class IntervalDetail
{
    public List<DateTime> Times { get; set; } = new();
    public decimal Amount { get; set; }
}

public class TaxCalculationDetails
{
    public VehicleType VehicleType { get; set; }
    public bool IsTollFreeVehicle { get; set; }
}