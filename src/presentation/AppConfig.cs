using domain;
using data;
using System.Runtime.InteropServices;
using Mindmagma.Curses;

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

        AskForResize();
        ResizeTerminal();
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
        mainMenu.AddItem("Continue Game", () => ContinueGame(gameWindow), () => _currentGameSession != null);
        mainMenu.AddItem("View Statistics", () => _windowManager.PushWindow(statsWindow));
        mainMenu.AddItem("Exit", () => _windowManager.ShouldClose = true);

        // Pause Menu Logic
        bool is3DMode = false;
        pauseMenu.AddItem("Continue", () => _windowManager.PopWindow());
        pauseMenu.AddItem("Toggle 2D/3D Mode", () =>
        {
            if (is3DMode)
            {
                ResizeTerminal();
                gameWindow.GameSession = _currentGameSession;
                _windowManager.SwitchTo(gameWindow);
            }
            else
            {
                int width = Math.Max(Console.WindowWidth, Level.WIDTH + 4);
                int height = Math.Max(Console.WindowHeight, Level.HEIGHT + 4);

                ResizeTerminal(width, height);
                gameWindow3D.GameSession = _currentGameSession;
                _windowManager.SwitchTo(gameWindow3D);
            }

            is3DMode = !is3DMode;
            _currentGameSession?.Is3DMode = is3DMode;
        });
        pauseMenu.AddItem("Restart", () => StartNewGame(gameWindow));
        pauseMenu.AddItem("View Statistics", () => _windowManager.PushWindow(statsWindow));
        pauseMenu.AddItem("Main Menu", () => _windowManager.SwitchTo(mainMenu));
        pauseMenu.AddItem("Exit", () => _windowManager.ShouldClose = true);

        // Game Window Logic
        gameWindow.AddInput(InputAction.ESCAPE, () =>
        {
            if (_currentGameSession?.state != GameSession.State.PLAYING)
            {
                mainMenu.Reset();
                _windowManager.SwitchTo(mainMenu);
                _currentGameSession = null;
                return;
            }

            _currentGameSession?.RequestSave();
            _windowManager.PushWindow(pauseMenu);
        });
        gameWindow3D.AddInput(InputAction.ESCAPE, () =>
        {
            if (_currentGameSession?.state != GameSession.State.PLAYING)
            {
                mainMenu.Reset();
                _windowManager.SwitchTo(mainMenu);
                _currentGameSession = null;
                return;
            }

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

    static private void ResizeTerminal(int width = Level.WIDTH + 4, int height = Level.HEIGHT + 4)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                // 1. If we are shrinking, we must shrink the WINDOW first, then the BUFFER.
                // 2. If we are expanding, we must expand the BUFFER first, then the WINDOW.
                bool isShrinking = width < Console.BufferWidth || height < Console.BufferHeight;

                if (isShrinking)
                {
                    Console.SetWindowSize(width, height);
                    Console.BufferWidth = width;
                    Console.BufferHeight = height;
                }
                else
                {
                    Console.BufferWidth = width;
                    Console.BufferHeight = height;
                    Console.SetWindowSize(width, height);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Fallback: If resizing fails (e.g., font size issues), just try to match buffer to window
                Console.BufferWidth = Math.Max(Console.WindowWidth, width);
                Console.BufferHeight = Math.Max(Console.WindowHeight, height);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // On Linux/Mac, this escape sequence asks the terminal emulator to resize
            Console.Write($"\x1b[8;{height};{width}t");
            Console.Out.Flush();
            // Gives terminal time to react
            Thread.Sleep(100);
        }

        // Tells NCurses to re-read terminal size
        NCurses.Refresh();
        NCurses.Clear();
    }

    static private void AskForResize(int expectedWidth = Level.WIDTH + 4, int expectedHeight = Level.HEIGHT + 4)
    {
        while (Console.WindowWidth < expectedWidth || Console.WindowHeight < expectedHeight)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.CursorTop = Console.WindowHeight / 2 - 1;

            string message = $"Please resize terminal to {expectedWidth}x{expectedHeight} (currently {Console.WindowWidth}x{Console.WindowHeight})";
            int centeredMessageX = Console.WindowWidth / 2 - message.Length / 2;
            if (centeredMessageX < 0)
                centeredMessageX = 0;

            Console.CursorLeft = centeredMessageX;
            Console.WriteLine(message);

            message = "Resize and press any key.";
            centeredMessageX = Console.WindowWidth / 2 - message.Length / 2;
            if (centeredMessageX < 0)
                centeredMessageX = 0;

            Console.CursorLeft = centeredMessageX;
            Console.CursorTop++;
            Console.WriteLine(message);
            Console.ReadKey(true);
        }
    }
}