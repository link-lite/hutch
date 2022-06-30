namespace HutchManager.Models;

public class DataSource
{
  public string Id { get; set; } = string.Empty;
  public DateTimeOffset LastCheckin { get; set; }

  public DataSource(Data.Entities.DataSource entity)
  {
    Id = entity.Id;
    LastCheckin = entity.LastCheckin;
   
  }

}
