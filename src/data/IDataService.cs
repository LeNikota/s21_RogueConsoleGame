using domain;
namespace data;

interface IDataService
{
    Task<(GameStatistics, Level)?> LoadAsync();
    Task SaveAsync(GameSession gameSession);
}