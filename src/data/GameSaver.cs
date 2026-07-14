using domain;

namespace data;

class GameSaver : IDisposable
{
    private readonly IDataService _dataService;
    private readonly GameSession _session;
    private readonly Scoreboard _scoreboard;

    public GameSaver(GameSession session, Scoreboard scoreboard, IDataService dataService)
    {
        _session = session;
        _scoreboard = scoreboard;
        _dataService = dataService;

        _session.OnSaveRequested += HandleSaveRequest;
        // _session.OnGameEvent += HandleGameEvent;
    }

    // private void HandleGameEvent(object? sender, GameEventArgs e)
    // {
    //      Tracks specific milestones.
    // }

    private async void HandleSaveRequest(object? sender, GameSaveEventArgs e)
    {
        try
        {
            await _dataService.SaveGameAsync(e.Stats, e.Level);
            if (_scoreboard.TryAddScore(e.Stats))
                await _dataService.SaveScoreboardAsync(_scoreboard);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to auto-save: {ex.Message}");
        }
    }

    public void Dispose()
    {
        // To prevent memory leaks and multiple saves
        _session.OnSaveRequested -= HandleSaveRequest;
        // _session.OnGameEvent -= HandleGameEvent;
    }
}