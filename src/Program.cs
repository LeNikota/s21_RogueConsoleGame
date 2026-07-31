using Mindmagma.Curses;
using data;
using presentation;

nint screen = NCurses.InitScreen();
// Tells the ncurses library to hide the text cursor in the terminal
NCurses.SetCursor(CursesCursorState.INVISIBLE);

if (!NCurses.HasColors())
{
    NCurses.MoveAddString(NCurses.Lines / 2, NCurses.Columns / 2, "Your terminal doesn't support any colors");
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