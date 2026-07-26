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
        public const int WIDTH = 128; // used to resize the terminal
        public const int HEIGHT = 32; // used to resize the terminal
        public bool[,] IsWall { get; set; }
        public VisibilityState[,] Visibility { get; set; }

        public List<Entity> Entities { get; set; } = [];
        private Player? player;
        public Player? Player
        {
            get => player;
            set
            {
                if (player != null && Entities.Contains(player))
                    Entities.Remove(player);

                player = value;

                if (player != null)
                    Entities.Add(player);
            }
        }

        public int Floor { get; set; }
        Room[] Rooms { get; set; }
        List<List<Passage>> Passages { get; set; }

        public Level(int floor, Player player)
        {
            Floor = floor;
            Player = player;

            (Rooms, Passages, Entities, IsWall) = LevelGenerator.Generate(WIDTH, HEIGHT, floor, player);

            // Initialize visibility to Unexplored
            Visibility = new VisibilityState[HEIGHT, WIDTH];
            for (int y = 0; y < HEIGHT; y++)
                for (int x = 0; x < WIDTH; x++)
                    Visibility[y, x] = VisibilityState.Visible; //VisibilityState.Unexplored;

            // Initial reveal around player
            //UpdateVisibility();

            if (floor > 1) GameLog.Log($"Welcome to floor {floor}!");
        }

        public Level(int floor, Player player, bool[,] isWall, VisibilityState[,] visibility, List<Entity> entities)
        {
            Floor = floor;
            IsWall = isWall;
            Visibility = visibility;

            Entities = entities ?? [];
            if (!Entities.Contains(player))
                Player = player;

            Rooms = new Room[9];
            Passages = [];
        }

        public bool IsPositionValid(Vector2i pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= WIDTH || pos.Y >= HEIGHT || IsWall[pos.Y, pos.X]) return false;
            return true;
        }

        public Entity? GetEntityByPos(Vector2i pos)
        {
            // if there are several entities in one spot, will return first fount entity
            foreach (var e in Entities)
            {
                if (e.Position == pos) return e;
            }
            return null;

        }


        // Depending on the VIEW_RADIUS value following scenarios can be emulated:
        //  10 VIEW_RADIUS - dark dungeons
        //  4 VIEW_RADIUS - very dark dungeons or player blinded by enemy
        //  VIEW_RADIUS changes dynamically like from 2-40 - when exiting a pitch black room and entering a lit room
        public const int VIEW_RADIUS = 40;
        public void UpdateVisibility()
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    if (Visibility[y, x] == VisibilityState.Visible)
                        Visibility[y, x] = VisibilityState.Explored;
                }
            }

            int px = Player.Position.X;
            int py = Player.Position.Y;

            for (int y = py - VIEW_RADIUS; y <= py + VIEW_RADIUS; y++)
            {
                for (int x = px - VIEW_RADIUS; x <= px + VIEW_RADIUS; x++)
                {
                    if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT) continue;

                    // Optimization: check points only inside the circle (distance smaller than radius)
                    int dx = x - px;
                    int dy = y - py;
                    if (dx * dx + dy * dy > VIEW_RADIUS * VIEW_RADIUS) continue;

                    CastRay(px, py, x, y);
                }
            }

            // Player always sees himself
            Visibility[py, px] = VisibilityState.Visible;
        }

        /// <summary>
        /// Checks, does the point (x0, y0) sees the point (x1, y1)
        /// Uses Bresenham's algorithm for ray casting
        /// 
        /// Current implementations can cause non-displaying corners if a player to close to a room wall
        /// Rays are not reaching that corner
        ///      ────────────┘ │
        ///    │            @  └
        ///    │
        ///    └───────────┐ ┌────────
        /// 
        /// There is a better implementations that fix this problem
        /// https://www.albertford.com/shadowcasting/#mark_visible
        /// </summary>
        private void CastRay(int x0, int y0, int x1, int y1)
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
                // The point is reached, there is a direct line of sight to the point
                if (x == x1 && y == y1)
                {
                    Visibility[y, x] = VisibilityState.Visible;
                    return;
                }

                // There is a wall blocking line of sight
                if (IsWall[y, x])
                {
                    Visibility[y, x] = VisibilityState.Visible;
                    return;
                }

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