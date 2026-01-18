using Microsoft.AspNetCore.Mvc;

namespace BrainShelf.Api.Controllers;

/// <summary>
/// Health check controller for monitoring API status
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns the health status of the API
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check endpoint called");
        
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "BrainShelf API",
            Version = "1.0.0"
        });
    }
}
