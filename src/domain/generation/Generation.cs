using System.Numerics;


/// <summary>
///     Файл для работы с процедурной генерацией.
///     Содержит статический класс генерации уровня: комнат и коридоров.
/// </summary>
namespace domain.generation
{
    public readonly record struct Vector2i(int X, int Y)
    {
        public static Vector2i operator +(Vector2i a, Vector2i b) => new(a.X + b.X, a.Y + b.Y);
    }
    // public readonly record struct Object /*Entity*/(Vector2i Position, Vector2i Size);
    // public readonly record struct Rectangle(Vector2i Position, Vector2i Size); // As one struct replacing Object, Room and Passage

    class Object /*Entity*/(Vector2i position, Vector2i size)
    {
        // Upper left position point of an object
        public Vector2i Position = position;
        public Vector2i Size = size;

        public override string ToString()
        {
            return $"Position {Position.X}, {Position.Y};\nSize {Size.X}, {Size.Y}";
        }
    }

    class Room(Vector2i position, Vector2i size) : Object(position, size) { }

    // door_placement:
    // 0 - no doors
    // 1 - door on left end
    // 2 - door on right end
    // 3 - door on both ends 
    class Passage(Vector2i position, Vector2i size, int door_placement = 0) : Object(position, size) { }

    public readonly record struct Edge(int V1, int V2);

    static class LevelGenerator
    {
        // -- Room generation --
        const int ROOM_COUNT = 9;
        const int MIN_ROOM_WIDTH = 3;
        const int MIN_ROOM_HEIGH = 3;
        const int MAX_ROOM_WIDTH = (REGION_WIDTH - GRID_OFFSET_WIDTH - 2);
        const int MAX_ROOM_HEIGH = (REGION_HEIGHT - GRID_OFFSET_HEIGHT - 2);

        // must be greater than or equal sqrt(ROOM_COUNT)
        const int GRID_DIMENSIONS = 3;
        const int GRID_OFFSET_WIDTH = 1;
        const int GRID_OFFSET_HEIGHT = 1;
        const int ROOMS_IN_ROW = 3;
        const int ROOMS_IN_COLUMN = 3;

        // These consts must be greater than MAX_ROOM_WIDTH and MAX_ROOM_HEIGH
        const int REGION_WIDTH = 30;
        const int REGION_HEIGHT = 10;

        static readonly Random rand = new();


        static void GenerateLevel()
        {
            Level level = new();
            Player player = new(0, 0);

            Room[] rooms = GenerateRooms(/*level*/);
            List<List<Passage>> passages = GeneratePassages(rooms);
        }

        public static Room[] GenerateRooms(/*Level level*/)
        {
            var rooms = new Room[ROOM_COUNT];

            for (int i = 0; i < ROOM_COUNT; i++)
            {
                int room_width = rand.Next(MIN_ROOM_WIDTH, MAX_ROOM_WIDTH + 1);
                int room_height = rand.Next(MIN_ROOM_HEIGH, MAX_ROOM_HEIGH + 1);


                int left_range_x = (i % GRID_DIMENSIONS) * REGION_WIDTH;
                int right_range_x = (i % GRID_DIMENSIONS + 1) * REGION_WIDTH - room_width - GRID_OFFSET_WIDTH;
                int x = rand.Next(left_range_x, right_range_x + 1);

                int up_range_y = (i / GRID_DIMENSIONS) * REGION_HEIGHT;
                int down_range_y = (i / GRID_DIMENSIONS + 1) * REGION_HEIGHT - room_height - GRID_OFFSET_HEIGHT;
                int y = rand.Next(up_range_y, down_range_y + 1);

                rooms[i] = new Room(new Vector2i(x, y), new Vector2i(room_width, room_height));
            }

            return rooms;
        }

        static List<Edge> GenerateRoomsEdges()
        {
            List<Edge> edges = [];

            for (int i = 0; i < ROOMS_IN_COLUMN; i++)
            {
                for (int j = 0; j + 1 < ROOMS_IN_ROW; j++)
                {
                    int current_room = i * ROOMS_IN_COLUMN + j;
                    edges.Add(new Edge(current_room, current_room + 1));
                }
            }

            for (int i = 0; i + 1 < ROOMS_IN_COLUMN; i++)
            {
                for (int j = 0; j < ROOMS_IN_ROW; j++)
                {
                    int current_room = i * ROOMS_IN_COLUMN + j;
                    edges.Add(new Edge(current_room, current_room + ROOMS_IN_ROW));
                }
            }

            return edges;
        }

        static Passage GeneratePassage(int x, int y, int width, int height)
        {
            var position = new Vector2i(x, y);
            var size = new Vector2i(width, height);

            return new Passage(position, size);
        }

        static List<Passage> GenerateHorizontalPassage(Room first_room, Room second_room)
        {
            List<Passage> passage = [];

            int first_x = first_room.Position.X + first_room.Size.X - 1;
            int up_range_y = first_room.Position.Y;
            int bottom_range_y = first_room.Position.Y + first_room.Size.Y - 3;
            int first_y = rand.Next(up_range_y, bottom_range_y + 1);

            int second_x = second_room.Position.X;
            up_range_y = second_room.Position.Y;
            bottom_range_y = second_room.Position.Y + second_room.Size.Y - 3;
            int second_y = rand.Next(up_range_y, bottom_range_y + 1);


            int min_x = Math.Min(first_x, second_x);
            int max_x = Math.Max(first_x, second_x);
            int min_y = Math.Min(first_y, second_y);
            int max_y = Math.Max(first_y, second_y);
            int x_diff = Math.Abs(max_x - min_x);
            // Generate linear or non-linear passage
            if (first_y == second_y)
            {
                int width = Math.Abs(second_x - first_x) + 1;
                passage.Add(GeneratePassage(first_x, first_y, width, 3));
            }
            else if (x_diff == 2)
            {
                int height = Math.Abs(second_y - first_y) + 3;
                passage.Add(GeneratePassage(first_x, min_y, 3, height));
            }
            else if (x_diff >= 3 && x_diff <= 5)
            {
                int height = Math.Abs(second_y - first_y) + 3;
                passage.Add(GeneratePassage(first_x, min_y, 3, height));
                passage.Add(GeneratePassage(first_x + 2, second_y, x_diff - 1, 3));
            }
            else
            {
                // 1. Horizontal from first room to bend
                int bend_x = rand.Next(min_x + 2, max_x - 3);
                passage.Add(GeneratePassage(first_x, first_y, Math.Abs(bend_x - first_x) + 1, 3));

                // // 2. Vertical bend
                passage.Add(GeneratePassage(bend_x, min_y, 3, Math.Abs(max_y - min_y) + 3));

                // // 3. Horizontal from bend to second room
                passage.Add(GeneratePassage(bend_x + 2, second_y, Math.Abs(second_x - bend_x) - 1, 3));
            }

            return passage;
        }

        static List<Passage> GenerateVerticalPassage(Room first_room, Room second_room)
        {
            int first_y = first_room.Position.Y + first_room.Size.Y - 1;
            int left_range_x = first_room.Position.X + 1;
            int right_range_x = first_room.Position.X + first_room.Size.X - 2;
            int first_x = rand.Next(left_range_x, right_range_x + 1);

            int second_y = second_room.Position.Y;
            left_range_x = second_room.Position.X + 1;
            right_range_x = second_room.Position.X + second_room.Size.X - 2;
            int second_X = rand.Next(left_range_x, right_range_x + 1);

            // if (first_y == second_y)
            // {
            //     // return GeneratePassage(first_x, first_y, 1, Math.Abs(second_y - first_y) + 1);
            // return GeneratePassage(first_y, left_range_x, 1, Math.Abs(second_y - first_y) + 1);
            // }
            // else
            // {
            //     return null;// Non-linear passage
            // }
            return null;
        }

        public static List<List<Passage>> GeneratePassages(Room[] rooms)
        {
            var passages = new List<List<Passage>>();

            var edges = GenerateRoomsEdges();
            Tools.Shuffle(edges);

            var union = new UnionFind(ROOM_COUNT);
            foreach (var edge in edges)
            {
                if (union.Find(edge.V1) != union.Find(edge.V2))
                {
                    union.Union(edge.V1, edge.V2);

                    if (Math.Abs(edge.V1 - edge.V2) == 1)
                    {
                        // Horizontal neighbors
                        passages.Add(GenerateHorizontalPassage(rooms[edge.V1], rooms[edge.V2]));
                    }
                    else
                    {
                        // Vertical neighbors
                        // passages.Add(GenerateVerticalPassage(rooms[edge.V1], rooms[edge.V2]));
                    }
                }
            }

            return passages;
        }

        public static List<Door> GenerateDoors(Passage[] passages)
        {
            List<Door> doors = [];

            // foreach (var passage in passages)
            // {
            //     passage.Position.X;
            //     passage.Position.Y;
            //     doors.Add();
            // }

            return doors;
        }
    }
}