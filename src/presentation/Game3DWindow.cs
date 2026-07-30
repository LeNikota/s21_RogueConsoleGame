using Mindmagma.Curses;
using domain;
using System;
using domain.generation;

namespace presentation;

class Game3DWindow : Window
{
    private const int FOV_DEGREES = 60;
    // private const int FOV_DEGREES = 70;
    private const double FOV_RADIANS = FOV_DEGREES * Math.PI / 180.0;
    private const int RAY_COUNT = 128; // Resolution (lower = faster)

    // Texture Characters (Unicode Block Elements)
    // private const char WALL_NEAR = '█';   // U+2588 Full Block
    // private const char WALL_MED = '▓';    // U+2593 Dark Shade
    // private const char WALL_FAR = '▒';    // U+2592 Medium Shade
    // private const char WALL_VERY_FAR = '░'; // U+2591 Light Shade
    // private const char WALL_EDGE = '│';   // Vertical Line for edges

    private const char WALL_NEAR = '#';   // Solid wall
    private const char WALL_MED = '%';    // Dense texture 
    private const char WALL_FAR = '+';    // Medium texture 
    private const char WALL_VERY_FAR = '.'; // Light texture 

    // private const char WALL_NEAR = '#';
    // private const char WALL_MED = '+';
    // private const char WALL_FAR = '.';
    // private const char WALL_VERY_FAR = ' ';

    private const char WALL_EDGE = '|';   // Vertical line for edges

    // private const char WALL_NEAR = '#';
    // private const char WALL_MED = 'X';
    // private const char WALL_FAR = 'x';
    // private const char WALL_VERY_FAR = ':';
    // private const char WALL_EDGE = '|';


    public GameSession? GameSession { get; set; }

    public Game3DWindow() : base("3D Rogue Mode") { }

    public override void Input(InputAction input)
    {
        if (_actions.TryGetValue(input, out Action? action))
        {
            action?.Invoke();
        }

        GameSession?.Input(input);
    }

    public override void Render()
    {
        if (GameSession == null || GameSession.Level == null) return;

        NCurses.Clear();

        Level level = GameSession.Level;
        Player player = GameSession.Player;

        Render3DWord(level, player);
        Render3DEnteties(level, player);
        LevelRenderer.PrintLevelAsMinimap(level);
        LevelRenderer.PrintGameUI(GameSession);

        switch (GameSession.state)
        {
            case GameSession.State.WIN:
                LevelRenderer.PrintGameWin();
                break;
            case GameSession.State.OVER:
                LevelRenderer.PrintGameOver();
                break;
        }
    }

    static private void Render3DWord(Level level, Player player)
    {
        //! TODO: MODIFY
        // FOR 3D
        //      AUTO SCREEN UPDATE

        // int screenWidth = Level.WIDTH;
        // int screenHeight = Level.HEIGHT;

        int screenWidth = 222;
        int screenHeight = 50;

        double currentAngle = player.Angle;

        double deltaAngle = FOV_RADIANS / RAY_COUNT;
        double rayAngle = currentAngle - FOV_RADIANS / 2.0;

        for (int i = 0; i < RAY_COUNT; i++)
        {
            // Cast Ray and get detailed info
            (double distance, double hitX, double hitY) = CastRayDetailed(level, player.PositionDouble, rayAngle);

            // Fix Fish-Eye Effect
            double correctedDist = distance * Math.Cos(rayAngle - currentAngle);

            // Calculate Wall Height
            int wallHeight = (int)(screenHeight / correctedDist);
            if (wallHeight > screenHeight) wallHeight = screenHeight;
            if (wallHeight < 1) wallHeight = 1;

            int startY = (screenHeight - wallHeight) / 2;
            int endY = startY + wallHeight;

            // Select Texture Character based on distance
            char wallChar;
            if (correctedDist < 6.0)
                wallChar = WALL_NEAR;
            else if (correctedDist < 12.0)
                wallChar = WALL_MED;
            else if (correctedDist < 15.0)
                wallChar = WALL_FAR;
            else
                wallChar = WALL_VERY_FAR;

            // Check for Edge Hit (Texture Separator)
            if (IsEdgeHit(hitX, hitY))
            {
                wallChar = WALL_EDGE;
            }

            // Draw Vertical Strip
            // This maps the current ray index i (0 to RAY_COUNT) to a specific column on the screen (0 to screenWidth).
            //     If you have 80 rays and a 128-wide screen, Ray 0 starts at column 0, Ray 40 starts around column 64, etc.
            int x = (int)((double)i / RAY_COUNT * screenWidth);

            // This determines how many character columns this single ray should occupy.
            //     If screenWidth is 128 and RAY_COUNT is 80, stripWidth will be 1.
            //     If screenWidth is 128 and RAY_COUNT is 40, stripWidth will be 3.This ensures the whole screen is filled even if we cast fewer rays for performance.
            int stripWidth = Math.Max(1, screenWidth / RAY_COUNT);

            // Outer Loop (w): Iterates through the width of the strip (usually 1 or 2 characters wide).
            // Inner Loop (y): Iterates from the top of the screen to the bottom.
            for (int w = 0; w < stripWidth; w++)
            {
                int drawX = x + w;
                if (drawX >= screenWidth) break;

                for (int y = 0; y < screenHeight; y++)
                {
                    int screenY = y + LevelRenderer.Y_OFFSET;

                    if (y >= startY && y < endY)
                    {
                        // Draw Wall
                        NCurses.MoveAddChar(screenY, drawX, wallChar);
                    }
                    else
                    {
                        // Clear Floor/Ceiling
                        // NCurses.MoveAddChar(screenY, drawX, ' ');
                    }
                }
            }

            rayAngle += deltaAngle;
        }
    }

    static private (double distance, double hitX, double hitY) CastRayDetailed(Level level, Vector2d pos, double angle)
    {
        double sin = Math.Sin(angle);
        double cos = Math.Cos(angle);

        double stepSize = 0.05; // Smaller step for better edge detection
        double dist = 0;
        double x = pos.X;
        double y = pos.Y;

        double maxDist = 20.0;

        while (dist < maxDist)
        {
            x += cos * stepSize;
            y += sin * stepSize;
            dist += stepSize;

            int ix = (int)Math.Floor(x);
            int iy = (int)Math.Floor(y);

            // Boundary check
            if (ix < 0 || ix >= Level.WIDTH || iy < 0 || iy >= Level.HEIGHT)
                return (maxDist, x, y);

            if (level.IsWall[iy, ix])
            {
                return (dist, x, y);
            }
        }

        return (maxDist, x, y);
    }

    static private bool IsEdgeHit(double x, double y)
    {
        double eps = 0.01;
        return Math.Abs(x - Math.Round(x)) < eps || Math.Abs(y - Math.Round(y)) < eps;
    }

    static private void Render3DEnteties(Level level, Player player)
    {
        double currentAngle = player.Angle;
        double cosA = Math.Cos(currentAngle);
        double sinA = Math.Sin(currentAngle);

        // Half FOV for checking visibility
        double halfFov = FOV_RADIANS / 2.0;
        int screenWidth = NCurses.Columns;
        int screenHeight = NCurses.Lines - LevelRenderer.Y_OFFSET;

        // We collect entities to draw so we can sort them by distance (Painter's Algorithm)
        List<(int screenX, int startY, int endY, char icon, short colorPair, double dist)> entitiesToDraw = [];

        foreach (var entity in level.Entities)
        {
            if (entity == player) continue;

            if (level.Visibility[entity.Position.Y, entity.Position.X] != VisibilityState.Visible) continue;

            if (entity is Ghost ghost && ghost.IsInvisible) continue;

            // 1. Transform Entity Position to Relative to Player
            double dx = entity.Position.X - player.PositionDouble.X;
            double dy = entity.Position.Y - player.PositionDouble.Y;

            // 2. Rotate around Player (World -> Camera Space)
            double rotX = -dx * sinA + dy * cosA; // Left/Right
            double rotY = dx * cosA + dy * sinA; // Forward/Backward

            // 3. Check if entity is behind the player or too far
            if (rotY <= 0.1) continue; // Behind camera plane or too close

            // 4. Check if within Horizontal FOV
            double entityAngle = Math.Atan2(rotX, rotY);

            if (Math.Abs(entityAngle) > halfFov) continue; // Outside FOV

            // 5. Project to Screen Coordinates
            double scale = (screenWidth / 2.0) / Math.Tan(halfFov);

            int screenX = (int)((rotX / rotY) * scale + screenWidth / 2.0);


            // 6. Calculate Sprite Height based on Perspective
            // The further away, the smaller the sprite.
            // We use a base factor (e.g., screenHeight / 2) to determine max size at close range.
            double dist = Math.Sqrt(dx * dx + dy * dy);
            int spriteHeight = (int)(screenHeight / dist);
            if (spriteHeight < 1) spriteHeight = 1;
            // if (spriteHeight > screenHeight) spriteHeight = screenHeight;
            if (spriteHeight > screenHeight / 2) spriteHeight = screenHeight / 2;

            // Center the sprite vertically
            int startY = (screenHeight - spriteHeight) / 2;
            int endY = startY + spriteHeight;

            // Bounds check for X
            if (screenX < 0 || screenX >= screenWidth) continue;

            // Determine Color
            short pair = 0;
            if (LevelRenderer.ColorPairMap.TryGetValue(entity.Color, out short p))
                pair = p;

            entitiesToDraw.Add((screenX, startY, endY, (char)entity.Icon, pair, dist));
        }

        // Sort by distance descending so closer ones draw on top
        entitiesToDraw.Sort((a, b) => b.dist.CompareTo(a.dist));

        // Draw entities
        foreach (var e in entitiesToDraw)
        {
            NCurses.AttributeSet(NCurses.ColorPair(e.colorPair));

            int spriteWidth = 1;
            if (e.dist < 5.0) spriteWidth = 13;
            else if (e.dist < 7.0) spriteWidth = 7;
            else if (e.dist < 10.0) spriteWidth = 3;

            int halfWidth = spriteWidth / 2;
            int startX = e.screenX - halfWidth;
            int endX = startX + spriteWidth;

            for (int x = startX; x < endX; x++)
            {
                if (x < 0 || x >= NCurses.Columns) continue;

                for (int y = e.startY; y < e.endY; y++)
                {
                    int screenY = y + LevelRenderer.Y_OFFSET;

                    if (screenY >= 0 && screenY < NCurses.Lines)
                    {
                        //? May be use different chars for left/center/right to simulate texture
                        NCurses.MoveAddChar(screenY, x, e.icon);
                    }
                }
            }
        }
    }
}