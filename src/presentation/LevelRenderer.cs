using Mindmagma.Curses;
using domain;

class LevelRenderer
{
    private static readonly uint[] WallAcsLookup =
    [
        '#',                         // isolated wall (fallback)
        CursesLineAcs.VLINE,         // North only
        CursesLineAcs.VLINE,         // South only
        CursesLineAcs.VLINE,         // North+South
        CursesLineAcs.HLINE,         // West only
        CursesLineAcs.LRCORNER,      // North+West  → '┘'
        CursesLineAcs.URCORNER,      // South+West  → '┐'
        CursesLineAcs.RTEE,          // N+S+W (missing East) → '┤'
        CursesLineAcs.HLINE,         // East only
        CursesLineAcs.LLCORNER,      // North+East  → '└'
        CursesLineAcs.ULCORNER,     // South+East  → '┌'
        CursesLineAcs.LTEE,         // N+S+E (missing West) → '├'
        CursesLineAcs.HLINE,        // West+East
        CursesLineAcs.BTEE,         // N+W+E (missing South) → '┴'
        CursesLineAcs.TTEE,         // S+W+E (missing North) → '┬'
        CursesLineAcs.PLUS          // N+S+W+E → '┼'
    ];

    enum WallDir
    {
        None  = 0,
        North = 1,
        South = 2,
        West  = 4,
        East  = 8
    }

    public static void PrintLevel(nint screen, Level level)
    {
        bool[,] is_wall = level.is_wall;
        // int height = NCurses.Lines; 
        // int width = NCurses.Columns;

        int width = Level.WIDTH;
        int height = Level.HEIGHT;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!is_wall[y, x])
                {
                    NCurses.MoveAddChar(y, x, ' ');
                    continue;
                }

                // Compute the bitmask for this wall cell
                int mask = ((y > 0 && is_wall[y - 1, x]) ? (int)WallDir.North : 0) |
                           ((y < height - 1 && is_wall[y + 1, x]) ? (int)WallDir.South : 0) |
                           ((x > 0 && is_wall[y, x - 1]) ? (int)WallDir.West : 0) |
                           ((x < width - 1 && is_wall[y, x + 1]) ? (int)WallDir.East : 0);

                // Draw the appropriate ACS character
                NCurses.MoveAddChar(y, x, WallAcsLookup[mask]);
            }
        }

        foreach (var entity in level.entities)
        {
            NCurses.MoveAddChar(entity.position.y, entity.position.x, entity.icon);
        }


    }
}