using Mindmagma.Curses;
using domain;

namespace presentation;

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

    // private static readonly Dictionary<Color, short> ColorPairMap = new()
    public static readonly Dictionary<Color, short> ColorPairMap = new()
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

    private static uint GetWallChar(bool[,] isWall, int x, int y)
    {
        if (!isWall[y, x]) return ' ';

        int height = isWall.GetLength(0);
        int width = isWall.GetLength(1);
        int mask = ((y > 0 && isWall[y - 1, x]) ? 1 : 0) |
                   ((y < height - 1 && isWall[y + 1, x]) ? 2 : 0) |
                   ((x > 0 && isWall[y, x - 1]) ? 4 : 0) |
                   ((x < width - 1 && isWall[y, x + 1]) ? 8 : 0);

        // Fallback to # if lookup fails
        if (mask >= 0 && mask < WallAcsLookup.Length)
            return WallAcsLookup[mask];
        return '#';
    }

    public static void PrintLevel(Level level)
    {
        bool[,] is_wall = level.IsWall;
        VisibilityState[,] visibility = level.Visibility; // NEW
        int width = Level.WIDTH;
        int height = Level.HEIGHT;

        NCurses.AttributeSet(NCurses.ColorPair(0));

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                VisibilityState vis = visibility[y, x];

                // 1. Unexplored: Draw nothing (or black space)
                if (vis == VisibilityState.Unexplored)
                {
                    NCurses.MoveAddChar(y + Y_OFFSET, x, ' ');
                    continue;
                }

                // 2. Explored but not Visible: Draw only walls
                if (vis == VisibilityState.Explored)
                {
                    if (is_wall[y, x])
                    {
                        // Draw wall with dim color or standard ACS
                        int mask = ((y > 0 && is_wall[y - 1, x]) ? (int)WallDir.North : 0) |
                                   ((y < height - 1 && is_wall[y + 1, x]) ? (int)WallDir.South : 0) |
                                   ((x > 0 && is_wall[y, x - 1]) ? (int)WallDir.West : 0) |
                                   ((x < width - 1 && is_wall[y, x + 1]) ? (int)WallDir.East : 0);

                        NCurses.MoveAddChar(y + Y_OFFSET, x, WallAcsLookup[mask]);
                    }
                    else
                    {
                        NCurses.MoveAddChar(y + Y_OFFSET, x, '.');
                    }
                    continue;
                }

                // 3. Visible: Draw everything normally
                if (vis == VisibilityState.Visible)
                {
                    if (is_wall[y, x])
                    {
                        uint mask = GetWallChar(is_wall, x, y);
                        NCurses.MoveAddChar(y + Y_OFFSET, x, mask);
                    }
                    else
                    {
                        NCurses.MoveAddChar(y + Y_OFFSET, x, ' ');
                    }
                }
            }
        }

        // Render Entities ONLY if they are in Visible tiles
        foreach (var entity in level.Entities)
        {
            // Check if the entity's position is currently visible
            if (visibility[entity.Position.Y, entity.Position.X] != VisibilityState.Visible)
                continue;

            if (entity is Ghost ghost && ghost.IsInvisible)
                continue;

            short pair = 0;
            if (ColorPairMap.TryGetValue(entity.Color, out short p))
            {
                pair = p;
            }

            NCurses.AttributeSet(NCurses.ColorPair(pair));
            NCurses.MoveAddChar(entity.Position.Y + Y_OFFSET, entity.Position.X, entity.Icon);
        }

        NCurses.AttributeSet(NCurses.ColorPair(0));
    }

    public static void PrintLevelAsMinimap(Level level)
    {
        // Both must be odd to have a perfect center
        const int MAP_WIDTH = 41;
        const int MAP_HEIGHT = 21;

        // Position of the minimap on the terminal screen
        // const int OFFSET_X = 2;
        int OFFSET_X = NCurses.Columns - MAP_WIDTH - 2;
        const int OFFSET_Y = 2;

        int width = Level.WIDTH;
        int height = Level.HEIGHT;
        bool[,] isWall = level.IsWall;
        VisibilityState[,] visibility = level.Visibility;
        Player player = level.Player;

        // Calculate the top-left corner of the view window relative to the world
        // MAP_WIDTH/MAP_HEIGHT makes player to be in the center of box
        int viewStartX = player.Position.X - (MAP_WIDTH / 2);
        int viewStartY = player.Position.Y - (MAP_HEIGHT / 2);

        NCurses.AttributeSet(NCurses.ColorPair(1));

        for (int y = 0; y < MAP_HEIGHT; y++)
        {
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                // Convert local minimap coordinates to global world coordinates
                int worldX = viewStartX + x;
                int worldY = viewStartY + y;

                // 1. Check World Bounds
                if (worldX < 0 || worldX >= width || worldY < 0 || worldY >= height)
                {
                    // Draw empty space for out-of-bounds areas
                    NCurses.MoveAddChar(OFFSET_Y + y, OFFSET_X + x, ' ');
                    continue;
                }

                // 2. Check Visibility
                VisibilityState vis = visibility[worldY, worldX];
                if (vis == VisibilityState.Unexplored)
                {
                    NCurses.MoveAddChar(OFFSET_Y + y, OFFSET_X + x, ' ');
                    continue;
                }

                // 3. Draw Walls or Floor
                if (isWall[worldY, worldX])
                {
                    uint wallChar = GetWallChar(isWall, worldX, worldY);
                    NCurses.MoveAddChar(OFFSET_Y + y, OFFSET_X + x, wallChar);
                }
                else
                {
                    if (vis == VisibilityState.Visible)
                        NCurses.MoveAddChar(OFFSET_Y + y, OFFSET_X + x, ' ');
                    else
                        NCurses.MoveAddChar(OFFSET_Y + y, OFFSET_X + x, '.');
                }
            }
        }

        foreach (var entity in level.Entities)
        {
            if (entity is Ghost g && g.IsInvisible) continue;

            if (visibility[entity.Position.Y, entity.Position.X] != VisibilityState.Visible)
                continue;

            // Calculate where this entity falls in the local minimap window
            int localX = entity.Position.X - viewStartX;
            int localY = entity.Position.Y - viewStartY;

            // Check if entity is within the minimap bounds
            if (localX >= 0 && localX < MAP_WIDTH && localY >= 0 && localY < MAP_HEIGHT)
            {
                short pair = 0;
                if (ColorPairMap.TryGetValue(entity.Color, out short p)) pair = p;

                NCurses.AttributeSet(NCurses.ColorPair(pair));
                // Draw at offset + local coordinate
                NCurses.MoveAddChar(OFFSET_Y + localY, OFFSET_X + localX, entity.Icon);
            }
        }

        NCurses.AttributeSet(NCurses.ColorPair(0));
    }
}