using Mindmagma.Curses;
using domain;

class LevelRenderer
{
    public const int Y_OFFSET = 1; //to render a line below (the very first line is UI)
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

    enum WallDir { North = 1, South = 2, West = 4, East = 8 }
    private static readonly Dictionary<Color, short> ColorPairMap = new()
    {
        { Color.None, 0 },
        { Color.White, 1 },
        { Color.Red, 2 },
        { Color.Green, 3 },
        { Color.Yellow, 4 },
        { Color.Blue, 5 },
        { Color.Cyan, 6 },
        { Color.Magenta, 7 },
        { Color.Black, 8 }
    };

    public static void InitColors()
    {
        NCurses.InitPair(1, CursesColor.WHITE, CursesColor.BLACK);
        NCurses.InitPair(2, CursesColor.RED, CursesColor.BLACK);
        NCurses.InitPair(3, CursesColor.GREEN, CursesColor.BLACK);
        NCurses.InitPair(4, CursesColor.YELLOW, CursesColor.BLACK);
        NCurses.InitPair(5, CursesColor.BLUE, CursesColor.BLACK);
        NCurses.InitPair(6, CursesColor.CYAN, CursesColor.BLACK);
        NCurses.InitPair(7, CursesColor.MAGENTA, CursesColor.BLACK);
        NCurses.InitPair(8, CursesColor.BLACK, CursesColor.BLACK);
    }


    public static void PrintLevel(Level level)
    {
        bool[,] is_wall = level.is_wall;
        int width = Level.WIDTH;
        int height = Level.HEIGHT;
        NCurses.AttributeSet(NCurses.ColorPair(0));

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!is_wall[y, x])
                {
                    NCurses.MoveAddChar(y + Y_OFFSET, x, ' ');
                    continue;
                }
                int mask = ((y > 0 && is_wall[y - 1, x]) ? (int)WallDir.North : 0) |
                           ((y < height - 1 && is_wall[y + 1, x]) ? (int)WallDir.South : 0) |
                           ((x > 0 && is_wall[y, x - 1]) ? (int)WallDir.West : 0) |
                           ((x < width - 1 && is_wall[y, x + 1]) ? (int)WallDir.East : 0);

                NCurses.MoveAddChar(y + Y_OFFSET, x, WallAcsLookup[mask]);
            }
        }

        foreach (var entity in level.entities)
        {
            short pair = 0;
            if (ColorPairMap.TryGetValue(entity.Color, out short p))
            {
                pair = p;
            }

            NCurses.AttributeSet(NCurses.ColorPair(pair));
            NCurses.MoveAddChar(entity.position.Y + Y_OFFSET, entity.position.X, entity.icon);
        }

        NCurses.AttributeSet(NCurses.ColorPair(0));
    }
}