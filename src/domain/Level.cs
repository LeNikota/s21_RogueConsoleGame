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
        // Tells to the app the minimal required terminal dimensions to render the 2D level
        public const int WIDTH = 128;
        public const int HEIGHT = 32;
        // First Y, then X
        public bool[,] IsWall { get; set; }
        public VisibilityState[,] Visibility { get; set; }

        public List<Entity> Entities { get; set; } = [];
        private Player player;
        public Player Player
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
        // public Room[] Rooms { get; protected set; }
        Room[] Rooms { get; set; }
        List<List<Passage>> Passages { get; set; }

        public Level(int floor, Player player, GameStatistics? stats = null)
        {
            Floor = floor;
            this.player = player;

            (Rooms, Passages, Entities, IsWall) = LevelGenerator.Generate(WIDTH, HEIGHT, floor, player, stats);

            // Initialize visibility to Unexplored
            Visibility = new VisibilityState[HEIGHT, WIDTH];
            for (int y = 0; y < HEIGHT; y++)
                for (int x = 0; x < WIDTH; x++)
                    Visibility[y, x] = VisibilityState.Unexplored;

            // Initial reveal around player
            UpdateVisibility();

            if (floor > 1) GameLog.Log($"Welcome to floor {floor}!");
        }

        public Level(int floor, Player player, bool[,] isWall, VisibilityState[,] visibility, List<Entity> entities)
        {
            Floor = floor;
            IsWall = isWall;
            Visibility = visibility;

            Entities = entities ?? [];
            var foundPlayer = Entities.Find(e => e is Player);
            if (foundPlayer is Player existingPlayer)
            {
                this.player = existingPlayer;
            }
            else
            {
                this.player = player;
                Entities.Add(player);
            }


            Rooms = new Room[9];
            Passages = [];
        }

        public bool IsPositionValid(Vector2i pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= WIDTH || pos.Y >= HEIGHT || IsWall[pos.Y, pos.X]) return false;
            return true;
        }

        public bool IsPositionValid(int x, int y)
        {
            if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT || IsWall[y, x]) return false;
            return true;
        }

        public bool IsWallCollision(Vector2d pos)
        {
            int x = (int)Math.Floor(pos.X);
            int y = (int)Math.Floor(pos.Y);

            if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT)
                return true;

            return IsWall[y, x];
        }

        public bool IsWallCollision(double x, double y)
        {
            int X = (int)Math.Floor(x);
            int Y = (int)Math.Floor(y);

            if (X < 0 || X >= WIDTH || Y < 0 || Y >= HEIGHT)
                return true;

            return IsWall[Y, X];
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

        public Entity? GetEntityByPos(int x, int y)
        {
            // if there are several entities in one spot, will return first fount entity
            foreach (var e in Entities)
            {
                if (e.Position.X == x && e.Position.Y == y) return e;
            }

            return null;
        }

        public Room? GetRoomByPos(Vector2i pos)
        {
            if (Rooms == null) return null;

            foreach (var room in Rooms)
            {
                if (room != null && room.HasPoint(pos))
                {
                    return room;
                }
            }

            return null;
        }

        public Vector2i GetRandPosInRoom(Room room)
        {
            Vector2i result = Vector2i.ZERO;

            int upper_left_x = room.Position.X + 1;
            int upper_left_y = room.Position.Y + 1;

            int bottom_right_x = upper_left_x + room.Size.X - 2;
            int bottom_right_y = upper_left_y + room.Size.Y - 2;

            int max_tries = (bottom_right_x - upper_left_x) * (bottom_right_y - upper_left_y);
            HashSet<Vector2i> taken = [.. Entities.Where(e => room.HasPoint(e.Position)).Select(e => e.Position)];

            for (int i = 0; i < max_tries; i++)
            {
                result = new(
                    Random.Shared.Next(upper_left_x, bottom_right_x),
                    Random.Shared.Next(upper_left_y, bottom_right_y));
                if (!taken.Contains(result)) break;
            }

            return result;
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