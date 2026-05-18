using Mindmagma.Curses;
using System.Runtime.InteropServices;
using domain;

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
//NCurses.TimeOut(10); //GetChar will wait for 10 ms (to save resources)
//NCurses.NoDelay(screen, true);
NCurses.NoEcho();

bool shouldClose = false;
GameSession game = new();

while (!shouldClose)
{
    if (game.isOver) NCurses.TimeOut(-1);

    //NCurses.Clear();
    LevelRenderer.PrintLevel(game.GetLevel());
    UIRenderer.PrintUI(game);
    NCurses.Refresh();
    
    shouldClose = Input(game);
}
NCurses.EndWin();


static bool Input(GameSession game)
{
    int key = NCurses.GetChar();
    //if (key == -1) return false; //instead of ERR
    if (game.isOver) return true;
    
    switch (key)
    {
        case 'w': case 'W': case CursesKey.UP:
            game.Input(GameSession.InputAction.UP);
            break;
        case 's': case 'S': case CursesKey.DOWN:
            game.Input(GameSession.InputAction.DOWN);
            break;
        case 'a': case 'A': case CursesKey.LEFT:
            game.Input(GameSession.InputAction.LEFT);
            break;
        case 'd': case 'D': case CursesKey.RIGHT:
            game.Input(GameSession.InputAction.RIGHT);
            break;
        case CursesKey.ESC:
            return true;
        default:
            //NCurses.MoveAddString(0, 0, "Unknown input");
            break;
    }
    return false;
}

static void ResizeTerminal()
{
    int width = Level.WIDTH; 
    int height = Level.HEIGHT + 2; //+2 for the UI
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