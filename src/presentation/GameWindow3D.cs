using Mindmagma.Curses;
using domain;
using System;
using domain.generation;

namespace presentation;

class GameWindow3D : Window
{
    private const int FOV_DEGREES = 60;
    // private const int FOV_DEGREES = 70;
    private const double FOV_RADIANS = FOV_DEGREES * Math.PI / 180.0;
    private const int RAY_COUNT = 128; // Resolution (lower = faster)
    private const double MOVE_SPEED = 0.5;
    private const double ROT_SPEED = 0.05; // Radians

    private const int MINIMAP_SIZE = 15;
    private const int MINIMAP_OFFSET_X = 2;
    private const int MINIMAP_OFFSET_Y = 2;

    // Texture Characters (Unicode Block Elements)
    // private const char WALL_NEAR = '█';   // U+2588 Full Block
    // private const char WALL_MED = '▓';    // U+2593 Dark Shade
    // private const char WALL_FAR = '▒';    // U+2592 Medium Shade
    // private const char WALL_VERY_FAR = '░'; // U+2591 Light Shade
    // private const char WALL_EDGE = '│';   // Vertical Line for edges

    private const char WALL_NEAR = '#';   // Solid wall (closest)
    private const char WALL_MED = '%';    // Dense texture (medium-close)
    private const char WALL_FAR = '+';    // Medium texture (medium-far)
    private const char WALL_VERY_FAR = '.'; // Light texture (farthest)
    private const char WALL_EDGE = '|';   // Vertical line for edges

    // private const char WALL_NEAR = '#';
    // private const char WALL_MED = 'X';
    // private const char WALL_FAR = 'x';
    // private const char WALL_VERY_FAR = ':';
    // private const char WALL_EDGE = '|';


    public GameSession? GameSession { get; set; }

    public GameWindow3D() : base("3D Rogue Mode")
    {
        // AddInput(InputAction.UP, () => MovePlayer(1));
        // AddInput(InputAction.DOWN, () => MovePlayer(-1));
        AddInput(InputAction.LEFT, () => RotatePlayer(-1));
        AddInput(InputAction.RIGHT, () => RotatePlayer(1));
    }

    public override void Render()
    {
        if (GameSession == null || GameSession.Level == null) return;

        NCurses.Clear();

        Level level = GameSession.Level;
        Player player = GameSession.Player;

        Render3DView(level, player);
        // LevelRenderer.PrintLevel(GameSession.Level);
        // RenderMinimap(level, player);
        // RenderUI(GameSession);
    }

    static private void Render3DView(Level level, Player player)
    {
        int screenWidth = Level.WIDTH;
        int screenHeight = Level.HEIGHT;

        // int screenWidth = 222;
        // int screenHeight = 50;

        double currentAngle = player.Angle;

        double deltaAngle = FOV_RADIANS / RAY_COUNT;
        double rayAngle = currentAngle - FOV_RADIANS / 2.0;

        for (int i = 0; i < RAY_COUNT; i++)
        {
            // Cast Ray and get detailed info
            (double distance, double hitX, double hitY) = CastRayDetailed(level, player.Position, rayAngle);

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
            if (correctedDist < 3.0)
                wallChar = WALL_NEAR;
            else if (correctedDist < 6.0)
                wallChar = WALL_MED;
            else if (correctedDist < 10.0)
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

    static private (double distance, double hitX, double hitY) CastRayDetailed(Level level, Vector2i pos, double angle)
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

    private void RotatePlayer(int direction)
    {
        if (GameSession == null) return;
        GameSession.Level.Player.Angle += ROT_SPEED * direction;

        if (GameSession.Level.Player.Angle > 2 * Math.PI)
            GameSession.Level.Player.Angle -= 2 * Math.PI;
        if (GameSession.Level.Player.Angle < 0)
            GameSession.Level.Player.Angle += 2 * Math.PI;
    }
}