using HutchManager.Models;
using HutchManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HutchManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataSourcesController : ControllerBase
{
  private readonly DataSourceService _dataSources;
  public DataSourcesController(DataSourceService dataSources)
  {
    _dataSources = dataSources;
  }

  [HttpGet]
  public async Task<List<Models.DataSource>> List()
    => await _dataSources.List();

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(string id)
  {
    try
    {
      await _dataSources.Delete(id);
    }
    catch (KeyNotFoundException) { }
    return NoContent();
  }

}
