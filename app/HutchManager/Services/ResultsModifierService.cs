using HutchManager.Data;
using Microsoft.EntityFrameworkCore;

namespace HutchManager.Services;

public class ResultsModifierService
{
  private readonly ApplicationDbContext _db;

  public ResultsModifierService(ApplicationDbContext db)
  {
    _db = db;
  }

  /// <summary>
  /// Get all records in ModifierTypeModel
  /// </summary>
  /// <returns></returns>
  public async Task<List<Models.ModifierTypeModel>> GetTypes()
  {
    var list = await _db.ModifierTypes
      .AsNoTracking()
      .ToListAsync();
    return list.ConvertAll<Models.ModifierTypeModel>(x => new(x));
  }

  /// <summary>
  /// Create a new ResultsModifier record
  /// </summary>
  /// <param name="resultsModifier"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  /// <exception cref="BadHttpRequestException"></exception>
  public async Task<Models.ResultsModifierModel> Create(Models.CreateResultsModifier resultsModifier)
  {
    var type = (await _db.ModifierTypes.ToListAsync()).FirstOrDefault(x => x.Id == resultsModifier.Type) ??
               throw new InvalidOperationException($"Type {resultsModifier.Type} is not a valid ModifierType");
    var activitySource =
      (await _db.ActivitySources.ToListAsync()).FirstOrDefault(x => x.Id == resultsModifier.ActivitySourceId) ??
      throw new InvalidOperationException(
        $"Activity Source {resultsModifier.ActivitySourceId} is not a valid Activity Source");

    var limitCheck = (await _db.ResultsModifier.ToListAsync())
      .Where(x => x.ActivitySource.Id == activitySource.Id && x.Type.Id == type.Id);

    if (limitCheck.Count() >= type.Limit)
    {
      throw new BadHttpRequestException(
        $"Cannot create any more modifiers of type {type.Id} for the activity source {activitySource.DisplayName}");
    }

    var entity = resultsModifier.ToEntity(type, activitySource);
    await _db.ResultsModifier.AddAsync(entity);
    await _db.SaveChangesAsync();
    return new(entity);
  }

  /// <summary>
  /// Modify an existing ResultsModifier record
  /// </summary>
  /// <param name="id"></param>
  /// <param name="resultsModifier"></param>
  /// <returns></returns>
  /// <exception cref="KeyNotFoundException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
  public async Task<Models.ResultsModifierModel> Set(int id, Models.CreateResultsModifier resultsModifier)
  {
    var entity = await _db.ResultsModifier
      .Include(x => x.Type)
      .FirstOrDefaultAsync(x => x.Id == id);

    if (entity is null)
      throw new KeyNotFoundException(
        $"No ResultsModifier with ID: {id}");

    var type = (await _db.ModifierTypes.ToListAsync()).FirstOrDefault(x => x.Id == resultsModifier.Type) ??
               throw new InvalidOperationException($"Type {resultsModifier.Type} is not a valid ModifierType");
    var activitySource =
      (await _db.ActivitySources.ToListAsync()).FirstOrDefault(x => x.Id == resultsModifier.ActivitySourceId) ??
      throw new InvalidOperationException(
        $"Activity Source {resultsModifier.ActivitySourceId} is not a valid Activity Source");
    entity.Order = resultsModifier.Order;
    entity.Type = type;
    entity.Parameters = resultsModifier.Parameters;
    entity.ActivitySource = activitySource;
    await _db.SaveChangesAsync();
    return new(entity);
  }
  
  /// <summary>
  /// Delete a ResultsModifier by ID
  /// </summary>
  /// <param name="resultsModifierId"></param>
  /// <exception cref="KeyNotFoundException"></exception>
  public async Task Delete(int resultsModifierId)
  {
    var entity = await _db.ResultsModifier
      .AsNoTracking()
      .Include(x => x.Type)
      .FirstOrDefaultAsync(x => x.Id == resultsModifierId);

    if (entity is null)
      throw new KeyNotFoundException(
        $"No Results Modifier with ID: {resultsModifierId}");
    _db.ResultsModifier.Remove(entity);
    await _db.SaveChangesAsync();
  }
}
