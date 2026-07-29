using Mindmagma.Curses;
using System.Text;
using System.Runtime.InteropServices;
using domain;
using data;
using presentation;

WaitForResize();
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
// //! TODO: Modify behaver
// var gameWindow = new GameWindow();
var gameWindow = new GameWindow3D();
var statisticWindow = new StatisticsWindow(currentGameSession?.Stats ?? new GameStatistics(scoreboard.NextId), scoreboard);

gameWindow.AddInput(InputAction.ESCAPE, () =>
{
    gameWindow.GameSession.RequestSave();

    fromPausedMenu = true;
    currentWindow = pauseMenuWindow;
});
// //! TODO: DELETE
// gameWindow.AddInput(InputAction.NEXT, () => gameWindow.GameSession.OnLevelExited());

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
//! TODO: DELETE
mainMenuWindow.AddItem("Test Entities and View", () =>
{
    var stats = new GameStatistics(scoreboard.NextId);

    var player = new Player(2, 4);
    var level = new Level(1, player);

    Array.Clear(level.IsWall, 0, level.IsWall.Length);
    for (int y = 0; y < Level.HEIGHT; y++)
        for (int x = 0; x < Level.WIDTH; x++)
            level.Visibility[y, x] = VisibilityState.Visible;

    int roomLeft = 0;
    int roomRight = 40;
    int roomTop = 0;
    int roomBottom = 25;

    for (int x = roomLeft; x <= roomRight; x++)
    {
        level.IsWall[roomTop, x] = true;     // Top Wall
        level.IsWall[roomBottom, x] = true;  // Bottom Wall
    }
    for (int y = roomTop; y <= roomBottom; y++)
    {
        level.IsWall[y, roomLeft] = true;    // Left Wall
        level.IsWall[y, roomRight] = true;   // Right Wall
    }

    for (int y = 3; y <= 8; y++)
    {
        level.IsWall[y, 5] = true;
    }

    level.Entities.Clear();
    player.Position = new Vector2i(2, 4);
    level.Entities.Add(player);

    level.Entities.Add(new Zombie(15, 10));      // z
    level.Entities.Add(new Vampire(20, 10));     // v
    level.Entities.Add(new Ghost(25, 10));       // g
    level.Entities.Add(new Ogre(30, 10));        // O
    level.Entities.Add(new SnakeMage(35, 10));   // s
    level.Entities.Add(new Mimic(15, 15));       // m

    level.Entities.Add(new Weapon(20, 15));      // !
    level.Entities.Add(new Food(25, 15));        // F
    level.Entities.Add(new Scroll(30, 15));      // I
    level.Entities.Add(new Elixir(35, 15));      // &

    var key = new Key(2, 7, Color.Cyan);
    level.Entities.Add(key);

    level.Entities.Add(new Door(4, 8, false, key) );

    level.Entities.Add(new Door(35, 20, true) );

    currentGameSession = new GameSession(stats, level);
    gameWindow.GameSession = currentGameSession;
    currentSaver = new GameSaver(currentGameSession, scoreboard, dataService);

    currentWindow = gameWindow;
});
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
        case 'n':
        case 'N':
            window.Input(InputAction.NEXT);
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

static void ResizeTerminal(int width = 0, int height = 0)
{
    if (width == 0 || height == 0)
    {
        width = Level.WIDTH;
        height = Level.HEIGHT + 3; //+2 for the UI +1 cause otherwise it fucking explodes
    }

    // 1/4 Of my full screen 
    width = 222;
    height = 52;

    // My full screen
    // width = 506;
    // height = 108;

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

static void WaitForResize(int expectedWidth = 0, int expectedHeight = 0)
{
    //! TODO: MODIFY
    // FOR 2D
    //      MINIMAL SCREEN WARNING
    //      LEVEL SIZE CHANGE FOR BIGGER?
    // FOR 3D
    //      AUTO SCREEN UPDATE

    if (expectedWidth == 0 || expectedHeight == 0)
    {
        expectedWidth = Level.WIDTH;
        expectedHeight = Level.HEIGHT + 3;
    }
    // 1/4 Of my full screen 
    expectedWidth = 222;
    expectedHeight = 52;

    // // My full screen
    // expectedWidth = 506;
    // expectedHeight = 108;


    while (Console.WindowWidth < expectedWidth || Console.WindowHeight < expectedHeight)
    {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine($"Please resize terminal to {expectedWidth}x{expectedHeight} (currently {Console.WindowWidth}x{Console.WindowHeight})");
        Console.WriteLine("Press any key when done...");
        Console.ReadKey(true);
    }
    Console.Clear();
}