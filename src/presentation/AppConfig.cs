using domain;
using data;
using System.Diagnostics;

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
        _windowManager.PushWindow(new Window("App has not been initialize via AppConfig.InitializeAsync()."));
    }

    public async Task InitializeAsync()
    {
        _windowManager.Clear();

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
        var gameWindow3D = new Game3DWindow();
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
        bool is3DMode = false;
        pauseMenu.AddItem("Continue", () => _windowManager.PopWindow());
        pauseMenu.AddItem("Toggle 2D/3D Mode", () =>
        {
            if (is3DMode)
            {
                gameWindow.GameSession = _currentGameSession;
                _windowManager.SwitchTo(gameWindow);
            }
            else
            {
                gameWindow3D.GameSession = _currentGameSession;
                _windowManager.SwitchTo(gameWindow3D);
            }

            is3DMode = !is3DMode;
            _currentGameSession.Is3DMode = is3DMode;
        });
        pauseMenu.AddItem("Restart", () => StartNewGame(gameWindow));
        pauseMenu.AddItem("View Statistics", () => _windowManager.PushWindow(statsWindow));
        pauseMenu.AddItem("Main Menu", () => _windowManager.SwitchTo(mainMenu));
        pauseMenu.AddItem("Exit", () => _windowManager.ShouldClose = true);

        // Game Window Logic
        gameWindow.AddInput(InputAction.ESCAPE, () =>
        {
            _currentGameSession?.RequestSave();

            if (_currentGameSession?.state != GameSession.State.PLAYING)
            {
                _windowManager.SwitchTo(mainMenu);
                return;
            }

            _windowManager.PushWindow(pauseMenu);
        });
        gameWindow3D.AddInput(InputAction.ESCAPE, () =>
        {
            _currentGameSession?.RequestSave();

            if (_currentGameSession?.state != GameSession.State.PLAYING)
            {
                _windowManager.SwitchTo(mainMenu);
                return;
            }

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

    // private void Toggle2D3DMode(bool is3DMode)
    // {
    //     _windowManager.SwitchTo()
    // }
}