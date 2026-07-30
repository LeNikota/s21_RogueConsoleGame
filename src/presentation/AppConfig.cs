using domain;
using data;

namespace presentation;

class AppConfig
{
    private readonly WindowManager _windowManager = new();
    public bool ShouldClose => _windowManager.ShouldClose;
    public void Render() => _windowManager.Render();
    public void HandleInput(int key) => _windowManager.HandleInput(key);

    private readonly IDataService _dataService;
    private Scoreboard _scoreboard = new([]);

    // Shared State
    private GameSession? _currentGameSession;
    private GameSaver? _currentSaver;


    public AppConfig(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task InitializeAsync()
    {
        await LoadSaveDataAsync();
        await LoadScoreboardAsync();
        SetupWindows();
    }

    private async Task LoadSaveDataAsync()
    {
        var gameSave = await _dataService.LoadGameAsync();
        if (gameSave.HasValue)
        {
            _currentGameSession = new(gameSave.Value.Item1, gameSave.Value.Item2);
        }
    }

    private async Task LoadScoreboardAsync()
    {
        _scoreboard = await _dataService.LoadScoreboardAsync();
    }

    private void SetupWindows()
    {
        var mainMenu = new MenuWindow("==== S21 ROGUE GAME ====");
        var pauseMenu = new MenuWindow("PAUSED");
        var gameWindow = new GameWindow();
        var statsWindow = new StatisticsWindow(
            _currentGameSession?.Stats ?? new GameStatistics(_scoreboard.NextId),
            _scoreboard
        );

        // Main Menu Logic
        mainMenu.AddItem("Start New Game", () => StartNewGame(gameWindow));
        if (_currentGameSession != null)
            mainMenu.AddItem("Continue Game", () => ContinueGame(gameWindow));
        mainMenu.AddItem("View Statistics", () => _windowManager.PushWindow(statsWindow));
        mainMenu.AddItem("Exit", () => _windowManager.ShouldClose = true);

        // Pause Menu Logic
        pauseMenu.AddItem("Continue", () => _windowManager.PopWindow());
        pauseMenu.AddItem("Restart", () => StartNewGame(gameWindow));
        pauseMenu.AddItem("View Statistics", () => _windowManager.PushWindow(statsWindow));
        pauseMenu.AddItem("Main Menu", () => _windowManager.SwitchTo(mainMenu));
        pauseMenu.AddItem("Exit", () => _windowManager.ShouldClose = true);

        // Game Window Logic
        gameWindow.AddInput(InputAction.ESCAPE, () =>
        {
            _currentGameSession?.RequestSave();
            _windowManager.PushWindow(pauseMenu);
        });

        // Statistics Window Logic
        statsWindow.AddInput(InputAction.ESCAPE, () => _windowManager.PopWindow());
        statsWindow.AddInput(InputAction.ENTER, () => _windowManager.PopWindow());

        // Set Start
        _windowManager.SetStartWindow(mainMenu);
    }

    private void StartNewGame(GameWindow gameWindow)
    {
        var stats = new GameStatistics(_scoreboard.NextId);
        var level = new Level(1, new Player());
        _currentGameSession = new(stats, level);

        gameWindow.GameSession = _currentGameSession;
        _currentSaver = new(_currentGameSession, _scoreboard, _dataService);

        _windowManager.SwitchTo(gameWindow);
    }

    private void ContinueGame(GameWindow gameWindow)
    {
        if (_currentGameSession == null) return;

        gameWindow.GameSession = _currentGameSession;
        _currentSaver = new(_currentGameSession, _scoreboard, _dataService);

        _windowManager.SwitchTo(gameWindow);
    }
}