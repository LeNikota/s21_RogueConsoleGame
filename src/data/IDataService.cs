using domain;
namespace data;

interface IDataService
{
    Task<(GameStatistics, Level)?> LoadGameAsync();

    Task SaveGameAsync(GameStatistics stats, Level level);

    Task<Scoreboard> LoadScoreboardAsync();

    Task SaveScoreboardAsync(Scoreboard scoreboard);

}