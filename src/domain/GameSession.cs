using domain.generation;

namespace domain
{
    class GameSession
    {
        public enum InputAction { UP, DOWN, LEFT, RIGHT };
        public readonly Dictionary<InputAction, Vector2i> InputDirectionMap = new()
    {
        {InputAction.UP, Vector2i.UP},
        {InputAction.DOWN, Vector2i.DOWN},
        {InputAction.LEFT, Vector2i.LEFT},
        {InputAction.RIGHT, Vector2i.RIGHT}
    };

        private Level level;
        private Player player; //lookup not to look for it all the time


        public GameSession()
        {
            level = new Level();
            player = new Player(8, 8);
            level.entities.Add(player);
        }

        public Level GetLevel()
        {
            return level;
        }

        public void Input(InputAction action)
        {
            if (action == InputAction.UP || action == InputAction.DOWN || action == InputAction.LEFT || action == InputAction.RIGHT)
            {
                Vector2i new_pos = player.position + InputDirectionMap[action];
                if (!level.IsPositionValid(new_pos)) return;
                player.position = new_pos;

            }
            //handle non-movement input here:

        }

    }

    class Level
    {
        public const int WIDTH = 128; //used to resize the terminal
        public const int HEIGHT = 32; //used to resize the terminal
        public readonly bool[,] is_wall;
        public List<Entity> entities;

        public Level()
        {
            var rooms = LevelGenerator.GenerateRooms();
            var passages = LevelGenerator.GeneratePassages(rooms);

            is_wall = new bool[HEIGHT, WIDTH]; // [Y, X]

            // Initialize all to false
            Array.Clear(is_wall, 0, is_wall.Length);

            // 1. Draw Rooms
            foreach (var room in rooms)
            {
                if (room == null) continue;

                int startX = room.Position.X;
                int startY = room.Position.Y;
                int endX = startX + room.Size.X;
                int endY = startY + room.Size.Y;

                // Safety check to ensure we don't draw outside the map
                if (startX < 0 || startY < 0 || endX > WIDTH || endY > HEIGHT)
                {
                    Console.WriteLine($"Warning: Room at {startX},{startY} is out of bounds ({WIDTH}x{HEIGHT})");
                    continue;
                }

                // Draw Top and Bottom Walls (Horizontal lines)
                // Loop X from start to end. Index array as [Y, X]
                for (int x = startX; x < endX; x++)
                {
                    if (startY >= 0 && startY < HEIGHT) is_wall[startY, x] = true;             // Top
                    if (endY - 1 >= 0 && endY - 1 < HEIGHT) is_wall[endY - 1, x] = true;       // Bottom
                }

                // Draw Left and Right Walls (Vertical lines)
                // Loop Y from start to end. Index array as [Y, X]
                for (int y = startY; y < endY; y++)
                {
                    if (startX >= 0 && startX < WIDTH) is_wall[y, startX] = true;              // Left
                    if (endX - 1 >= 0 && endX - 1 < WIDTH) is_wall[y, endX - 1] = true;        // Right
                }
            }

            foreach (var passageGroup in passages) // Your List<List<Passage>>
            {
                foreach (var pass in passageGroup)
                {
                    if (pass == null) continue;

                    int startX = pass.Position.X;
                    int startY = pass.Position.Y;
                    int endX = startX + pass.Size.X;
                    int endY = startY + pass.Size.Y;

                    // Bounds check
                    if (startX < 0 || startY < 0 || endX > WIDTH || endY > HEIGHT) continue;

                    // 1. Draw the Border (Walls)
                    // Top and Bottom rows
                    for (int x = startX; x < endX; x++)
                    {
                        if (startY >= 0 && startY < HEIGHT) is_wall[startY, x] = true;       // Top
                        if (endY - 1 >= 0 && endY - 1 < HEIGHT) is_wall[endY - 1, x] = true; // Bottom
                    }
                    // Left and Right columns
                    for (int y = startY; y < endY; y++)
                    {
                        if (startX >= 0 && startX < WIDTH) is_wall[y, startX] = true;       // Left
                        if (endX - 1 >= 0 && endX - 1 < WIDTH) is_wall[y, endX - 1] = true; // Right
                    }

                    // 2. Clear the Inside (Make it hollow/walkable)
                    // Iterate from startX+1 to endX-1 and startY+1 to endY-1
                    for (int y = startY + 1; y < endY - 1; y++)
                    {
                        for (int x = startX + 1; x < endX - 1; x++)
                        {
                            if (y >= 0 && y < HEIGHT && x >= 0 && x < WIDTH)
                            {
                                is_wall[y, x] = false; // False = Walkable space
                            }
                        }
                    }
                }
            }

            entities = new List<Entity>();
        }

        public bool IsPositionValid(Vector2i pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= WIDTH || pos.y >= HEIGHT || is_wall[pos.y, pos.x]) return false;
            return true;
        }
    }

    class Vector2i(int _x, int _y)
    {
        public int x = _x;
        public int y = _y;

        public static readonly Vector2i ZERO = new(0, 0);

        public static readonly Vector2i UP = new(0, -1);
        public static readonly Vector2i DOWN = new(0, 1);
        public static readonly Vector2i LEFT = new(-1, 0);
        public static readonly Vector2i RIGHT = new(1, 0);

        public static Vector2i operator +(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x + b.x, a.y + b.y);
        }

        public static Vector2i operator -(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x - b.x, a.y - b.y);
        }

        public double DistanceTo(Vector2i other)
        {
            return Math.Sqrt(Math.Pow(other.x - x, 2) + Math.Pow(other.y - y, 2));
        }

    };

} //domain
