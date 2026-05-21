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

            var rooms = LevelGenerator.GenerateRooms();
            var passages = LevelGenerator.GeneratePassages(rooms);
            LevelGenerator.PlacePlayer(rooms, player);
            level.is_wall = LevelGenerator.ConvertToArray(rooms, passages, Level.WIDTH, Level.HEIGHT);

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
        public bool[,] is_wall;
        public List<Entity> entities;

        public Level()
        {
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
