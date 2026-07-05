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

IDataService dataService = new JsonDataService();
var savedGame = await dataService.LoadGameAsync();
// (GameStatistics stats, Level level) = await dataService.LoadGameAsync();
List<GameStatistics>? scoreboard = await dataService.LoadScoreboardAsync();

Window currentWindow;
var mainMenuWindow = new MenuWindow("==== S21 ROGUE GAME ====");
var pauseMenuWindow = new MenuWindow();
var gameWindow = new GameWindow();
// var statisticWindow = new StatisticsWindow(scoreboard);

gameWindow.AddInput(InputAction.ESCAPE, () => currentWindow = pauseMenuWindow);

mainMenuWindow.AddItem(
    "Start New Game", () =>
    {
        gameWindow.GameSession = new GameSession();
        currentWindow = gameWindow;

        // statisticWindow.GameSession = gameWindow.GameSession;


        GameLog.Log("Welcome to s21_rogue! Use WASD to move.");
    }
);
if (savedGame.HasValue)
{
    mainMenuWindow.AddItem(
        "Continue Game", () =>
        {
            var (stats, level) = savedGame.Value;
            gameWindow.GameSession = new GameSession(stats, level);
            currentWindow = gameWindow;
            GameLog.Log("Welcome to s21_rogue! Use WASD to move.");
        }
    );
}
// mainMenuWindow.AddItem("View Statistics", () => currentWindow = statisticWindow );
mainMenuWindow.AddItem(
    "Exit", () =>
    {
        shouldClose = true;
    }
);

pauseMenuWindow.AddInput(InputAction.ESCAPE, () => currentWindow = gameWindow);
pauseMenuWindow.AddItem("Continue", () => currentWindow = gameWindow);
pauseMenuWindow.AddItem("Restart", () =>
{
    var stats = gameWindow.GameSession.Stats;
    var level = gameWindow.GameSession.Level;
    dataService.SaveGameAsync(stats, level);

    gameWindow.GameSession = new();
    currentWindow = gameWindow;
});
// pauseMenuWindow.AddItem( "View Statistics", () => currentWindow = statisticWindow );
pauseMenuWindow.AddItem(
    "Main menu", () =>
    {
        var stats = gameWindow.GameSession.Stats;
        var level = gameWindow.GameSession.Level;
        dataService.SaveGameAsync(stats, level);
        savedGame = (stats, level);

        currentWindow = mainMenuWindow;
    }
);
pauseMenuWindow.AddItem(
    "Exit game", () =>
    {
        var stats = gameWindow.GameSession.Stats;
        var level = gameWindow.GameSession.Level;
        dataService.SaveGameAsync(stats, level);

        // needs to update scoreboard

        shouldClose = true;
    }
);

currentWindow = mainMenuWindow;


while (!shouldClose)
{
    if (currentWindow is GameWindow)
        LevelRenderer.PrintLevel(gameWindow.GameSession.Level);

    UIRenderer.PrintUI(currentWindow);
    NCurses.Refresh();

    Input(currentWindow);
}
NCurses.EndWin();


static void Input(Window window)
{
    int key = NCurses.GetChar();

    if (key >= '0' && key <= '9')
    {
        // window.Input(InputAction.NONE, key - 48);
        window.Input(InputAction.NONE);
        return;
    }
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
            //NCurses.MoveAddString(0, 0, "Unknown input");
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

