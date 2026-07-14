namespace domain;

class GameSaveEventArgs(GameStatistics stats, Level level) : EventArgs
{
    public GameStatistics Stats { get; } = stats;
    public Level Level { get; } = level;
}