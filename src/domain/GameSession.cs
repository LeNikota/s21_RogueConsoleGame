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

            // Initialize all to false (optional, default is false)
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

            // 2. Draw Passages (Fill them as empty space, or draw walls around them)
            // Assuming passages are hollow corridors, we usually just ensure they are NOT walls.
            // If your Passage object includes walls, use similar logic to above.
            foreach (var passage in passages)
            {
                if (passage == null) continue;

                int startX = passage.Position.X;
                int startY = passage.Position.Y;
                int endX = startX + passage.Size.X;
                int endY = startY + passage.Size.Y;

                // Clamp to bounds
                startX = Math.Max(0, startX);
                startY = Math.Max(0, startY);
                endX = Math.Min(WIDTH, endX);
                endY = Math.Min(HEIGHT, endY);

                for (int y = startY; y < endY; y++)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        // Passages are walkable, so ensure they are FALSE (not walls)
                        // Or if you want passage walls, set specific borders to true
                        is_wall[y, x] = true;
                    }
                }
            }

            entities = new List<Entity>();

            // Debug Output
            Console.WriteLine($"Generated {rooms.Length} rooms. Map size: {WIDTH}x{HEIGHT}");
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
