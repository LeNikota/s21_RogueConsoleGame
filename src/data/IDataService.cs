using domain;
namespace data;

interface IDataService
{
    Task<(GameStatistics, Level)?> LoadGameAsync();

    Task SaveGameAsync(GameStatistics stats, Level level);

    Task<List<GameStatistics>?> LoadScoreboardAsync();

    Task SaveScoreboardAsync(List<GameStatistics> statsList);

}