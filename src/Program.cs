using Mindmagma.Curses;
using System.Runtime.InteropServices;
using domain;
using data;
using presentation;
using System.Security;

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

IDataService dataService = new JsonDataService();
var app = new AppConfig(dataService);
await app.InitializeAsync();

// Main Game Loop
while (!app.ShouldClose)
{
    app.Render();
    int key = NCurses.GetChar();
    app.HandleInput(key);
}

NCurses.EndWin();

static void ResizeTerminal(int width = Level.WIDTH, int height = Level.HEIGHT + 3)
{
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
        Thread.Sleep(100);
    }
}

static void WaitForResize(int expectedWidth = Level.WIDTH, int expectedHeight = Level.HEIGHT + 3)
{
    Console.Clear();
    while (Console.WindowWidth < expectedWidth || Console.WindowHeight < expectedHeight)
    {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine($"Please resize terminal to {expectedWidth}x{expectedHeight} (currently {Console.WindowWidth}x{Console.WindowHeight})\n");
        Console.WriteLine("Press any key when done...");
        Console.ReadKey(true);
    }
    Console.Clear();
}