using System.ComponentModel.DataAnnotations;

namespace CongestionTaxCalculator.Application.DTOs;

public class TaxCalculationRequest
{
    [Required(ErrorMessage = "City code is required.")]
    public string City { get; set; } 
    
    [Required(ErrorMessage = "Vehicle type is required.")]
    public string VehicleType { get; set; }
    public List<DateTime> Passages { get; set; } = new();
}