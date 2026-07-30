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
        _session.OnDeleteRequested += HandleDeleteRequest;
        // _session.OnGameEvent += HandleGameEvent;
    }

    // private void HandleGameEvent(object? sender, GameEventArgs e)
    // {
    //      Tracks specific milestones.
    // }

    private async void HandleSaveRequest(object? sender, GameSaveEventArgs args)
    {
        try
        {
            await _dataService.SaveGameAsync(args.Stats, args.Level);
            if (_scoreboard.TryAddScore(args.Stats))
                await _dataService.SaveScoreboardAsync(_scoreboard);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to auto-save: {ex.Message}");
        }
    }

    private async void HandleDeleteRequest(object? sender, EventArgs args)
    {
        try
        {
            await _dataService.DeleteSaveAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete: {ex.Message}");
        }
    }

    public void Dispose()
    {
        // To prevent memory leaks and multiple saves deletions
        _session.OnSaveRequested -= HandleSaveRequest;
        _session.OnDeleteRequested -= HandleDeleteRequest;
        // _session.OnGameEvent -= HandleGameEvent;
    }
}