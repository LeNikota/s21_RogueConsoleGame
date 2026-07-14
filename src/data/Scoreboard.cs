using domain;

namespace data;

class Scoreboard(List<GameStatistics> scores, int maxEntries = 10)
{
    private List<GameStatistics> _scores = scores ?? [];
    public IReadOnlyList<GameStatistics> Scores => _scores.AsReadOnly();
    public int MaxId => _scores.Count != 0 ? _scores.Max(e => e.Id) : 0;
    public int NextId => MaxId + 1;
    public int Count => _scores.Count();
    public int MaxEntries { get; } = maxEntries;

    public bool TryAddScore(GameStatistics newStats)
    {
        if (newStats.TreasuresCollected <= 0 && newStats.HighestLevelReached <= 1)
        {
            return false;
        }

        var existingEntry = _scores.FirstOrDefault(s => s.Id == newStats.Id);

        if (existingEntry != null)
            _scores.Remove(existingEntry);

        _scores.Add(newStats);
        _scores = _scores.OrderByDescending(s => s.TreasuresCollected)
                         .ThenByDescending(s => s.HighestLevelReached)
                         .Take(MaxEntries)
                         .ToList();

        return true;
    }
}