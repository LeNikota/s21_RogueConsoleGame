using Mindmagma.Curses;
using System.Runtime.InteropServices;
using domain;
// using data;

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

bool shouldClose = false;
GameSession game = new();

// IDataService dataService = new JsonDataService("rogue_save.json");

while (!shouldClose)
{
    if (game.state == GameSession.State.EXITING) break;

    if (game.state == GameSession.State.PLAYING || game.state == GameSession.State.PAUSED)
        LevelRenderer.PrintLevel(game.GetLevel());

    UIRenderer.PrintUI(game);
    NCurses.Refresh();

    Input(game);
}
NCurses.EndWin();


void Input(GameSession game)
{
    int key = NCurses.GetChar();

    if (key >= '0' && key <= '9')
    {
        game.Input(GameSession.InputAction.NONE, key - 48);
        return;
    }
    switch (key)
    {
        case 'w':
        case 'W':
        case CursesKey.UP:
            game.Input(GameSession.InputAction.UP);
            break;
        case 's':
        case 'S':
        case CursesKey.DOWN:
            game.Input(GameSession.InputAction.DOWN);
            break;
        case 'a':
        case 'A':
        case CursesKey.LEFT:
            game.Input(GameSession.InputAction.LEFT);
            break;
        case 'd':
        case 'D':
        case CursesKey.RIGHT:
            game.Input(GameSession.InputAction.RIGHT);
            break;
        case 'i':
        case 'I':
        case '\t':
            game.Input(GameSession.InputAction.BACKPACK);
            break;
        case 'h':
        case 'H':
            game.Input(GameSession.InputAction.WEAPON);
            break;
        case 'j':
        case 'J':
            game.Input(GameSession.InputAction.FOOD);
            break;
        case 'k':
        case 'K':
            game.Input(GameSession.InputAction.ELIXIR);
            break;
        case 'e':
        case 'E':
            game.Input(GameSession.InputAction.SCROLL);
            break;
        case 't':
        case 'T':
            game.Input(GameSession.InputAction.DROP);
            break;
        case 'p':
        case 'P':
            UIRenderer.PrintStats(game.Stats);
            NCurses.GetChar();
            break;
        case 'r':
        case 'R':
            game.Input(GameSession.InputAction.RESTART);
            break;
        case '\n':
        case '\r':
        case CursesKey.ENTER:
            game.Input(GameSession.InputAction.ENTER);
            break;
        case CursesKey.ESC:
            game.Input(GameSession.InputAction.ESCAPE);
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

