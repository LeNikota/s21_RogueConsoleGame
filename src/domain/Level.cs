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

        public void UpdateVisibility()
        {
            // 1. Mark all currently Visible tiles as Explored
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    if (visibility[y, x] == VisibilityState.Visible)
                    {
                        visibility[y, x] = VisibilityState.Explored;
                    }
                }
            }

            // 2. Calculate new Visible tiles using Raycasting
            // Cast rays from the player to the edges of the map/rooms
            foreach (var dir in Vector2i.ALL_DIRECTIONS)
            {
                CastRay(player.position, dir);
            }

            visibility[player.position.Y, player.position.X] = VisibilityState.Visible;
        }

        private void CastRay(Vector2i start, Vector2i direction)
        {
            Vector2i current = start;

            // Step until hit a wall or boundary
            while (true)
            {
                current += direction;

                // Check bounds
                if (current.X < 0 || current.X >= WIDTH || current.Y < 0 || current.Y >= HEIGHT)
                    break;

                // Mark as visible
                visibility[current.Y, current.X] = VisibilityState.Visible;

                // Stop if we hit a wall (walls block vision)
                if (is_wall[current.Y, current.X])
                    break;
            }
        }
    }
}