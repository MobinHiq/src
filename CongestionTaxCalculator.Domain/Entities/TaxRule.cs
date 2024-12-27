namespace CongestionTaxCalculator.Domain.Entities;

public class TaxRule
{
    public int Id { get; set; }
    public string City { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal Amount { get; set; }
    
    // For testing
    public bool IsApplicable(TimeSpan time)
    {
        return time >= StartTime && time <= EndTime;
    }
}