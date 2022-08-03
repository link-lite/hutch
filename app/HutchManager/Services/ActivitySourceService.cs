using HutchManager.Data;
using Microsoft.EntityFrameworkCore;

namespace HutchManager.Services;

public class ActivitySourceService
{
  private readonly ApplicationDbContext _db;

  public ActivitySourceService(ApplicationDbContext db)
  {
    _db = db;
  }

  /// <summary>
  /// Get all ActivitySources
  /// </summary>
  /// <returns></returns>
  public async Task<List<Models.ActivitySourceModel>> List()
  {
    var list = await _db.ActivitySources
      .AsNoTracking()
      .Include(x => x.Type)
      .Include(x => x.TargetDataSource)
      .ToListAsync();
    return list.ConvertAll<Models.ActivitySourceModel>(x => new(x));
  }
  
  /// <summary>
  /// Create a new ActivitySource
  /// </summary>
  /// <param name="activitySource"></param>
  /// <returns></returns>
  public async Task<Models.ActivitySourceModel> Create(Models.CreateActivitySource activitySource)
  {
    var types = await _db.SourceTypes.ToListAsync();
    var dataSources = await _db.DataSources.ToListAsync();
    var entity = activitySource.ToEntity(types, dataSources);

    await _db.ActivitySources.AddAsync(entity);
    await _db.SaveChangesAsync();
    return new(entity);
  }
  
  /// <summary>
  /// Modify an ActivitySource by ID
  /// </summary>
  /// <param name="id"></param>
  /// <param name="activitySource"></param>
  /// <returns></returns>
  /// <exception cref="KeyNotFoundException"></exception>
  public async Task<Models.ActivitySourceModel> Set(int id, Models.CreateActivitySource activitySource)
  {
    var entity = await _db.ActivitySources
      .Include(x => x.Type)
      .Include(x => x.TargetDataSource)
      .FirstOrDefaultAsync(x => x.Id == id);

    if (entity is null)
      throw new KeyNotFoundException(
        $"No ActivitySource with ID: {id}");
    entity.Host = activitySource.Host;
    entity.DisplayName = activitySource.DisplayName;
    entity.ResourceId = activitySource.ResourceId;
    await _db.SaveChangesAsync();
    return new(entity);
  }
  
  /// <summary>
  /// Get an ActivitySource by ID
  /// </summary>
  /// <param name="activitySourceId"></param>
  /// <returns></returns>
  /// <exception cref="KeyNotFoundException"></exception>
  public async Task<Models.ActivitySourceModel> Get(int activitySourceId)
  {
    var activitySource = await _db.ActivitySources
                           .AsNoTracking()
                           .Include(x => x.Type)
                           .Include(x => x.TargetDataSource)
                           .SingleOrDefaultAsync(x => x.Id == activitySourceId)
                         ?? throw new KeyNotFoundException();
    return new(activitySource);
  }
  
  /// <summary>
  /// Get a list of ResultsModifiers linked to an ActivitySource
  /// using the ActivitySource ID 
  /// </summary>
  /// <param name="activitySourceId"></param>
  /// <returns></returns>
  public async Task<List<Models.ResultsModifierModel>> GetActivitySourceResultsModifiers(int activitySourceId)
  {
    var resultsModifierList = await _db.ResultsModifier
      .AsNoTracking()
      .Include(x => x.Type)
      .Include(x => x.ActivitySource)
      .Include(x => x.ActivitySource.Type)
      .Include(x => x.ActivitySource.TargetDataSource)
      .Where(x => x.ActivitySource.Id == activitySourceId)
      .ToListAsync();
    return resultsModifierList.ConvertAll<Models.ResultsModifierModel>(x => new(x));
  }

  /// <summary>
  /// Delete an ActivitySource by ID
  /// </summary>
  /// <param name="activitySourceId"></param>
  /// <exception cref="KeyNotFoundException"></exception>
  public async Task Delete(int activitySourceId)
  {
    var entity = await _db.ActivitySources
      .AsNoTracking()
      .Include(x => x.Type)
      .FirstOrDefaultAsync(x => x.Id == activitySourceId);
    if (entity is null)
      throw new KeyNotFoundException(
        $"No ActivitySource with ID: {activitySourceId}");
    _db.ActivitySources.Remove(entity);
    await _db.SaveChangesAsync();
  }
}
