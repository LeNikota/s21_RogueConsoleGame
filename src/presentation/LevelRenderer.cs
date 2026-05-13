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

    static LevelRenderer()
    {
        // Initialize the lookup table for masks 0..15
        // Mask 0 (isolated wall) – use a solid block; fallback to '#'
        WallAcsLookup[0] = '#';

        // Simple vertical/horizontal lines

    }

    enum WallDir
    {
        None  = 0,
        North = 1,
        South = 2,
        West  = 4,
        East  = 8
    }

    public static void PrintLevel(Level level)
    {
        bool[,] is_wall = level.is_wall;
        int size = Level.LEVEL_SIZE;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (!is_wall[x, y])
                {
                    // Floor tile – draw a space
                    NCurses.MoveAddChar(y, x, ' ');
                    continue;
                }

                // Compute the bitmask for this wall cell
                int mask = ((y > 0 && is_wall[x, y - 1]) ? (int)WallDir.North : 0) |
                           ((y < size - 1 && is_wall[x, y + 1]) ? (int)WallDir.South : 0) |
                           ((x > 0 && is_wall[x - 1, y]) ? (int)WallDir.West : 0) |
                           ((x < size - 1 && is_wall[x + 1, y]) ? (int)WallDir.East : 0);

                // Draw the appropriate ACS character
                NCurses.MoveAddChar(y, x, WallAcsLookup[mask]);
            }
        }
    }
}