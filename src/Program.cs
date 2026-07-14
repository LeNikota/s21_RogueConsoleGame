using Mindmagma.Curses;
using System.Runtime.InteropServices;
using domain;
using data;
using presentation;

ResizeTerminal();
nint screen = NCurses.InitScreen();
NCurses.SetCursor(0);

if (!NCurses.HasColors())
{
    NCurses.MoveAddString(NCurses.Lines / 2, NCurses.Columns / 2, "Your terminal doesn't support any colours");
    NCurses.GetChar();
    NCurses.EndWin();
    return;
}
NCurses.StartColor();
LevelRenderer.InitColors();

NCurses.Keypad(screen, true);
NCurses.NoEcho();

// Tells ncurses to exit the main loop
bool shouldClose = false;

// --- Services & Data ---
IDataService dataService = new JsonDataService();
var scoreboard = await dataService.LoadScoreboardAsync();
(GameStatistics stats, Level level)? gameSave = await dataService.LoadGameAsync();
GameSession? currentGameSession = null;
if (gameSave.HasValue)
    currentGameSession = new(gameSave.Value.stats, gameSave.Value.level);

// --- Observer: Auto-Saver ---
GameSaver? currentSaver = null;

// --- Windows ---
bool fromPausedMenu = false;
Window currentWindow;
var mainMenuWindow = new MenuWindow("==== S21 ROGUE GAME ====");
var pauseMenuWindow = new MenuWindow();
var gameWindow = new GameWindow();
var statisticWindow = new StatisticsWindow(currentGameSession?.Stats ?? new GameStatistics(scoreboard.NextId), scoreboard);

gameWindow.AddInput(InputAction.ESCAPE, () =>
{
    gameWindow.GameSession.RequestSave();

    fromPausedMenu = true;    
    currentWindow = pauseMenuWindow;
});

// --- Main Menu Setup ---
mainMenuWindow.AddItem("Start New Game", () =>
    {
        var stats = new GameStatistics(scoreboard.NextId);
        var level = new Level(1, new Player());

        currentGameSession = new(stats, level);
        gameWindow.GameSession = currentGameSession;
        currentSaver = new(currentGameSession, scoreboard, dataService);

        currentWindow = gameWindow;
    }
);
if (currentGameSession != null)
{
    mainMenuWindow.AddItem("Continue Game", () =>
        {
            gameWindow.GameSession = currentGameSession;
            currentSaver = new(currentGameSession, scoreboard, dataService);

            currentWindow = gameWindow;
        }
    );
}
mainMenuWindow.AddItem("View Statistics", () =>
    {
        currentWindow = statisticWindow;
    }
);
mainMenuWindow.AddItem("Exit", () => shouldClose = true);

// --- Pause Menu Setup ---
pauseMenuWindow.AddInput(InputAction.ESCAPE, () => currentWindow = gameWindow);
pauseMenuWindow.AddItem("Continue", () => currentWindow = gameWindow);
pauseMenuWindow.AddItem("Restart", () =>
    {
        var stats = new GameStatistics(scoreboard.NextId);
        var level = new Level(1, new Player());

        currentGameSession = new(stats, level);
        gameWindow.GameSession = currentGameSession;
        currentSaver = new(currentGameSession, scoreboard, dataService);

        currentWindow = gameWindow;
    }
);
pauseMenuWindow.AddItem("View Statistics", () =>
    {
        statisticWindow.Stats = gameWindow.GameSession.Stats;

        currentWindow = statisticWindow;
    }
);
pauseMenuWindow.AddItem("Main menu", () =>
    {
        fromPausedMenu = false;
        currentWindow = mainMenuWindow;
    }
);
pauseMenuWindow.AddItem("Exit", () =>
    {
        currentSaver?.Dispose();
        shouldClose = true;
    }
);

// --- Statistic Menu Setup ---
statisticWindow.AddInput(InputAction.ESCAPE, () => currentWindow = fromPausedMenu ? pauseMenuWindow : mainMenuWindow);
statisticWindow.AddInput(InputAction.ENTER, () => currentWindow = fromPausedMenu ? pauseMenuWindow : mainMenuWindow);

currentWindow = mainMenuWindow;

while (!shouldClose)
{
    currentWindow.Render();
    NCurses.Refresh();
    
    Input(currentWindow);
}

NCurses.EndWin();

static void Input(Window window)
{
    int key = NCurses.GetChar();
    // if (key >= '0' && key <= '9')
    // {
    //     window.Input(InputAction.NONE, key - 48);
    //     return;
    // }
    switch (key)
    {
        case 'w':
        case 'W':
        case CursesKey.UP:
            window.Input(InputAction.UP);
            break;
        case 's':
        case 'S':
        case CursesKey.DOWN:
            window.Input(InputAction.DOWN);
            break;
        case 'a':
        case 'A':
        case CursesKey.LEFT:
            window.Input(InputAction.LEFT);
            break;
        case 'd':
        case 'D':
        case CursesKey.RIGHT:
            window.Input(InputAction.RIGHT);
            break;
        case 'i':
        case 'I':
        case '\t':
            window.Input(InputAction.BACKPACK);
            break;
        case 'h':
        case 'H':
            window.Input(InputAction.WEAPON);
            break;
        case 'j':
        case 'J':
            window.Input(InputAction.FOOD);
            break;
        case 'k':
        case 'K':
            window.Input(InputAction.ELIXIR);
            break;
        case 'e':
        case 'E':
            window.Input(InputAction.SCROLL);
            break;
        case 't':
        case 'T':
            window.Input(InputAction.DROP);
            break;
        case 'r':
        case 'R':
            window.Input(InputAction.RESTART);
            break;
        case '\n':
        case '\r':
        case CursesKey.ENTER:
            window.Input(InputAction.ENTER);
            break;
        case CursesKey.ESC:
            window.Input(InputAction.ESCAPE);
            break;
        default:
            break;
    }
}

static void ResizeTerminal()
{
    int width = Level.WIDTH;
    int height = Level.HEIGHT + 3; //+2 for the UI +1 cause otherwise it fucking explodes
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Console.BufferWidth = width;
        Console.BufferHeight = height;
        Console.SetWindowSize(width, height);
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
             RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        Console.Write($"\x1b[8;{height};{width}t");
        Console.Out.Flush();
        Thread.Sleep(100); // give terminal time to react
    }
}