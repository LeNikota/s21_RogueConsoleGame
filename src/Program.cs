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

static void ResizeTerminal(int width = 0, int height = 0)
{
    if (width == 0 || height == 0)
    {
        width = Level.WIDTH;
        height = Level.HEIGHT + 3;
    }
    width = 222;
    height = 52;

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

static void WaitForResize(int expectedWidth = 0, int expectedHeight = 0)
{
    if (expectedWidth == 0 || expectedHeight == 0)
    {
        expectedWidth = Level.WIDTH;
        expectedHeight = Level.HEIGHT + 3;
    }
    expectedWidth = 222;
    expectedHeight = 52;

    while (Console.WindowWidth < expectedWidth || Console.WindowHeight < expectedHeight)
    {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine($"Please resize terminal to {expectedWidth}x{expectedHeight} (currently {Console.WindowWidth}x{Console.WindowHeight})");
        Console.WriteLine("Press any key when done...");
        Console.ReadKey(true);
    }
    Console.Clear();
}