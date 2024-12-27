using System.ComponentModel.DataAnnotations;
using CongestionTaxCalculator.Application.DTOs;
using CongestionTaxCalculator.Application.Interfaces;
using CongestionTaxCalculator.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CongestionTaxCalculator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxCalculatorController : ControllerBase
{
    private readonly ITaxCalculatorService _taxCalculatorService;
    private readonly ILogger<TaxCalculatorController> _logger;

    public TaxCalculatorController(ITaxCalculatorService taxCalculatorService, ILogger<TaxCalculatorController> logger)
    {
        _taxCalculatorService = taxCalculatorService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates congestion tax for vehicle passages
    /// </summary>
    [HttpPost("calculate")]
    //[ProducesResponseType(typeof(TaxCalculationResponse), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaxCalculationResponse>> CalculateTax(
        [FromBody] TaxCalculationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            /*if (string.IsNullOrEmpty(request.VehicleType))
            {
                return BadRequest("Vehicle type is required.");
            }
        
            if (string.IsNullOrEmpty(request.City))
            {
                return BadRequest("City is required");
            }*/
            
            var result = await _taxCalculatorService.CalculateTax(request, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning($"Validation error: {ex.Message}");
            
            return BadRequest(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    { "Validation", new[] { ex.Message } }
                }));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error");
            return BadRequest(new ProblemDetails
            {
                Title = "Domain Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calculating tax");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while processing your request",
                    Detail = ex.Message
                });
        }
    }
}