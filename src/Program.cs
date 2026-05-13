using Mindmagma.Curses;
using domain;

nint screen = NCurses.InitScreen();

// NCurses.GetMaxYX(screen, out int maxY, out int maxX);

NCurses.AddChar((int)CursesLineAcs.LLCORNER);
NCurses.AddChar((int)CursesLineAcs.BTEE);
NCurses.MoveAddChar(4, 4, (int)CursesLineAcs.HLINE);
NCurses.Refresh();
NCurses.GetChar();

NCurses.Keypad(screen, true);   // Enable special key detection
//NCurses.NoDelay(screen, true);
NCurses.NoEcho();

int key;
bool shouldClose = false;

GameSession game = new();

while (!shouldClose)
{
    key = NCurses.GetChar();
    NCurses.Clear(); 
    switch (key)
    {
        // case 'w': case 'W': case CursesKey.UP:
        //     NCurses.MoveAddString(0, 0, "Move UP (WASD)");
        //     break;
        // case 's': case 'S': case CursesKey.DOWN:
        //     NCurses.MoveAddString(0, 0, "Move DOWN (WASD)");
        //     break;
        // case 'a': case 'A': case CursesKey.LEFT:
        //     NCurses.MoveAddString(0, 0, "Move LEFT (WASD)");
        //     break;
        // case 'd': case 'D': case CursesKey.RIGHT:
        //     NCurses.MoveAddString(0, 0, "Move RIGHT (WASD)");
        //     break;
        case CursesKey.ESC:
            shouldClose = true;
            break;
        default:
            //NCurses.MoveAddString(0, 0, "Unknown input");
            break;
    }
    LevelRenderer.PrintLevel(game.GetLevel());
    NCurses.Refresh(); // Make changes visible
}

NCurses.EndWin();