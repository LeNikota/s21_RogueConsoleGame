using System.ComponentModel.DataAnnotations;
using System.Numerics;
using domain.generation;
namespace domain
{
    public enum VisibilityState
    {
        Unexplored,
        Explored, // Seen before, but not currently visible
        Visible   // Currently in line of sight
    }

    class Level
    {
        public const int WIDTH = 128; //used to resize the terminal
        public const int HEIGHT = 32; //used to resize the terminal
        public readonly bool[,] is_wall;
        public readonly VisibilityState[,] visibility;

        public List<Entity> entities = [];
        public readonly Player player; // lookup not to look for it all the time
        public readonly int floor;
        readonly Room[] rooms;
        readonly List<List<Passage>> passages;

        public Level(int floor, Player player)
        {
            this.floor = floor;
            this.player = player;

            (rooms, passages, entities, is_wall) = LevelGenerator.Generate(WIDTH, HEIGHT, floor, player);

            // Initialize visibility to Unexplored
            visibility = new VisibilityState[HEIGHT, WIDTH];
            for (int y = 0; y < HEIGHT; y++)
                for (int x = 0; x < WIDTH; x++)
                    visibility[y, x] = VisibilityState.Unexplored;

            // Initial reveal around player
            UpdateVisibility();

            if (floor > 1) GameLog.Log($"Welcome to floor {floor}!");
        }

        public bool IsPositionValid(Vector2i pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= WIDTH || pos.Y >= HEIGHT || is_wall[pos.Y, pos.X]) return false;
            return true;
        }

        public Entity? GetEntityByPos(Vector2i pos)
        {
            //if there are several entities here, we might have a problem
            foreach (var e in entities)
            {
                if (e.position == pos) return e;
            }
            return null;

        }


        public const int VIEW_RADIUS = 20;
        public void UpdateVisibility()
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    if (visibility[y, x] == VisibilityState.Visible)
                        visibility[y, x] = VisibilityState.Explored;
                }
            }

            int px = player.position.X;
            int py = player.position.Y;

            for (int y = py - VIEW_RADIUS; y <= py + VIEW_RADIUS; y++)
            {
                for (int x = px - VIEW_RADIUS; x <= px + VIEW_RADIUS; x++)
                {
                    if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT) continue;

                    // Optimization: check points only inside the circle (distance smaller than radius)
                    int dx = x - px;
                    int dy = y - py;
                    if (dx * dx + dy * dy > VIEW_RADIUS * VIEW_RADIUS) continue;

                    // If there is a direct sight from player point and given point
                    if (HasLineOfSight(px, py, x, y))
                    {
                        visibility[y, x] = VisibilityState.Visible;
                    }
                }
            }

            // Player always sees himself
            visibility[py, px] = VisibilityState.Visible;
        }

        /// <summary>
        /// Checks, does the point (x0, y0) sees the point (x1, y1)
        /// Uses Bresenham's algorithm for ray casting    
        /// </summary>
        private bool HasLineOfSight(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int x = x0;
            int y = y0;

            while (true)
            {
                // The point is reached, sight is clear
                if (x == x1 && y == y1) return true;

                // There is a wall blocking sight
                if (is_wall[y, x]) return false;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
        }
    }
}