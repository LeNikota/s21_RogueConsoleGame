using System.ComponentModel.DataAnnotations;
using System.Numerics;
using domain.generation;
namespace domain
{

    class Level
    {
        public const int WIDTH = 128; //used to resize the terminal
        public const int HEIGHT = 32; //used to resize the terminal
        public readonly bool[,] is_wall;

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
    }
}