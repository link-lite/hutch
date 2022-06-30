using HutchManager.Data;
using HutchManager.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HutchManager.Services;

public class DataSourceService
{
  private readonly ApplicationDbContext _db;
  public DataSourceService(ApplicationDbContext db)
  {
    _db = db;
  }

  public async Task<List<Models.DataSource>> List()
  {
    var list = await _db.DataSources
      .AsNoTracking()
      .ToListAsync();
    return list.ConvertAll<Models.DataSource>(x => new(x));
  }

  public async Task<Models.DataSource> CreateorUpdate(Models.DataSource dataSource)
  {
    var entity = await _db.DataSources
      .AsNoTracking()
      .FirstOrDefaultAsync(x => x.Id == dataSource.Id);
    if (entity is null)
    {
      // create new Datasource with LastCheckin set as now
      var datasourceEntity = new DataSource{
        Id = dataSource.Id,
        LastCheckin = dataSource.LastCheckin,
      };
      await _db.DataSources.AddAsync(datasourceEntity);
      await _db.SaveChangesAsync();
      return new(datasourceEntity);
    }
    else
    {
      // update Datasource LastCheckin to now
      entity.LastCheckin = dataSource.LastCheckin;
      await _db.SaveChangesAsync();
      return new(entity);
    }

  }

  public async Task Delete(string dataSourceId)
  {
    var entity = await _db.DataSources
      .AsNoTracking()
      .FirstOrDefaultAsync(x => x.Id == dataSourceId);
    if (entity is null)
      throw new KeyNotFoundException(
        $"No DataSource with ID: {dataSourceId}");
    _db.DataSources.Remove(entity);
    await _db.SaveChangesAsync();

  }

}
