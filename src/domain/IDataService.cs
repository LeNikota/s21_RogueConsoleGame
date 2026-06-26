namespace domain;


public interface IDataService
{
    Task<SaveData?> LoadAsync();
    Task SaveAsync(SaveData data);
}