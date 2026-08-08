using Microsoft.AspNetCore.Mvc;
namespace GHunterBackend.Controllers;
[ApiController][Route("api/health")]
public sealed class HealthController:ControllerBase{[HttpGet]public IActionResult Get()=>Ok(new{ok=true,service="GHunterBackend",utc=DateTimeOffset.UtcNow});}
