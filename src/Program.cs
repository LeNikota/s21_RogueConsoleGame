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
NCurses.InitPair(1, CursesColor.GREEN, CursesColor.BLACK);

//NCurses.GetMaxYX(screen, out int maxY, out int maxX);
//NCurses.AddString($"Max x is {maxX}, max y is {maxY}");
//NCurses.Refresh();
//NCurses.GetChar();

NCurses.Keypad(screen, true); 
//NCurses.NoDelay(screen, true);
NCurses.NoEcho();

bool shouldClose = false;
GameSession game = new();

while (!shouldClose)
{
    NCurses.Clear(); 
    NCurses.AttributeOn(NCurses.ColorPair(1));
    LevelRenderer.PrintLevel(game.GetLevel());
    NCurses.Refresh();
    int key = NCurses.GetChar();

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
        //! Delete - generates new level
        case 'n': case 'N':
            game = new();
            break;
        case CursesKey.ESC:
            shouldClose = true;
            break;
        default:
            //NCurses.MoveAddString(0, 0, "Unknown input");
            break;
    }

}

NCurses.EndWin();

static void ResizeTerminal()
{
    int width = Level.WIDTH; 
    int height = Level.HEIGHT + 1; //+1 otherwise it throws ERR (mb add extra for the UI)
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

