using HutchManager.Models;
using HutchManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HutchManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivitySourceController : ControllerBase
{
  private readonly ActivitySourceService _activitySources;
  public ActivitySourceController(ActivitySourceService activitySources)
  {
    _activitySources = activitySources;
  }

  [HttpGet]
  public async Task<List<Models.ActivitySource>> List()
    => await _activitySources.List();
  
  [HttpPost]
  public async Task<ActivitySource> Create(CreateActivitySource activitySource)
    => await _activitySources.Create(activitySource);
  
  [HttpGet("{id}")]
  public async Task<ActionResult<ActivitySource>> Get(int id)
    =>await _activitySources.Get(id);
  
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    try
    {
      await _activitySources.Delete(id);
    }
    catch (KeyNotFoundException) {
      
    }
    return NoContent();
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> Set(int id, [FromBody] CreateActivitySource activitySource)
  {
    try
    {
      return Ok(await _activitySources.Set(id, activitySource));
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }
}
