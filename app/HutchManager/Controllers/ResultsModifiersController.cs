using HutchManager.Models;
using HutchManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HutchManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResultsModifiersController : ControllerBase
{
  private readonly ResultsModifierService _resultsModifier;
  public ResultsModifiersController(ResultsModifierService resultsModifier)
  {
    _resultsModifier = resultsModifier;
  }
  
  /// <summary>
  /// Create a new ResultsModifier
  /// </summary>
  /// <param name="resultsModifier"></param>
  /// <returns></returns>
  [HttpPost]
  public async Task<ResultsModifierModel> Create(CreateResultsModifier resultsModifier)
    => await _resultsModifier.Create(resultsModifier);
  
  /// <summary>
  /// Get a list of all Types
  /// </summary>
  /// <returns></returns>
  [HttpGet("types")]
  public async Task<List<ModifierTypeModel>> GetTypes()
    => await _resultsModifier.GetTypes();
  
  /// <summary>
  /// Delete a ResultsModiefier by ID
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    try
    {
      await _resultsModifier.Delete(id);
    }
    catch (KeyNotFoundException)
    {

    }
    return NoContent();
  }
  
  /// <summary>
  /// Modify a ResultsModifier by ID
  /// </summary>
  /// <param name="id"></param>
  /// <param name="resultsModifier"></param>
  /// <returns></returns>
  [HttpPut("{id}")]
  public async Task<IActionResult> Set(int id, [FromBody] CreateResultsModifier resultsModifier)
  {
    try
    {
      return Ok(await _resultsModifier.Set(id, resultsModifier));
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }
}
