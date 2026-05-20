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

    class Room(Vector2i position, Vector2i size) : Object(position, size)
    {
        // info about items
        // info about player
        // info about monsters
    }

    class Passage(Vector2i position, Vector2i size) : Object(position, size)
    {
        // info about items
        // info about player
        // info about monsters
    }

    public readonly record struct Edge(int V1, int V2);

    static class LevelGenerator
    {
        // -- Room generation --
        const int ROOM_COUNT = 9;
        const int MIN_ROOM_WIDTH = 3;
        const int MIN_ROOM_HEIGH = 3;
        const int MAX_ROOM_WIDTH = 12;
        const int MAX_ROOM_HEIGH = 9;

        // must be greater than or equal sqrt(ROOM_COUNT)
        const int GRID_DIMENSIONS = 3;
        const int ROOMS_IN_ROW = 3;
        const int ROOMS_IN_COLUMN = 3;

        // These consts must be greater than MAX_ROOM_WIDTH and MAX_ROOM_HEIGH
        const int REGION_WIDTH = 15;
        const int REGION_HEIGHT = 10;

        const int MAX_PASSAGE_PARTS = 3;
        const int MAX_PASSAGE_COUNT = (ROOM_COUNT - 1) * MAX_PASSAGE_PARTS;

        static readonly Random rand = new();


        static void GenerateLevel()
        {
            Level level = new();
            Player player = new(0, 0);

            Room[] rooms = GenerateRooms(/*level*/);
            Passage[] passages = GeneratePassages(rooms);
        }

        public static Room[] GenerateRooms(/*Level level*/)
        {
            var rooms = new Room[ROOM_COUNT];

            for (int i = 0; i < ROOM_COUNT; i++)
            {
                int room_width = rand.Next(MIN_ROOM_WIDTH, MAX_ROOM_WIDTH);
                int room_height = rand.Next(MIN_ROOM_HEIGH, MAX_ROOM_HEIGH);


                int left_range_x = (i % GRID_DIMENSIONS) * REGION_WIDTH + 1;
                int right_range_x = (i % GRID_DIMENSIONS + 1) * REGION_WIDTH - room_width - 1;
                int x = rand.Next(left_range_x, right_range_x + 1);

                int up_range_y = (i / GRID_DIMENSIONS) * REGION_HEIGHT + 1;
                int down_range_y = (i / GRID_DIMENSIONS + 1) * REGION_HEIGHT - room_height - 1;
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
            var position = new Vector2i(x - 1, y - 1);
            var size = new Vector2i(width + 2, height + 2);

            return new Passage(position, size);
        }

        static Passage GenerateHorizontalPassage(Room first_room, Room second_room)
        {
            int first_x = first_room.Position.X + first_room.Size.X - 1;
            int up_range_y = first_room.Position.Y + 1;
            int bottom_range_y = first_room.Position.Y + first_room.Size.Y - 2;
            int first_y = rand.Next(up_range_y, bottom_range_y + 1);

            int second_x = second_room.Position.X;
            up_range_y = second_room.Position.Y + 1;
            bottom_range_y = second_room.Position.Y + second_room.Size.Y - 2;
            int second_y = rand.Next(up_range_y, bottom_range_y + 1);

            // if (first_y == second_y)
            // {
                // return GeneratePassage(first_x, first_y, Math.Abs(second_x - first_x) + 1, 1);
                return GeneratePassage(first_x, up_range_y, Math.Abs(second_x - first_x) + 1, 1); // Test
            // }
            // else
            // {
            //     return null;// Non-linear passage
            // }
        }

        static Passage GenerateVerticalPassage(Room first_room, Room second_room)
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
                return GeneratePassage(first_y, left_range_x, 1, Math.Abs(second_y - first_y) + 1);
            // }
            // else
            // {
            //     return null;// Non-linear passage
            // }
        }

        public static Passage[] GeneratePassages(Room[] rooms)
        {
            var passages = new Passage[ROOM_COUNT];

            var edges = GenerateRoomsEdges();
            Tools.Shuffle(edges);

            var union = new UnionFind(ROOM_COUNT);
            for (int i = 0; i < passages.Length; i++)
            {
                if (union.Find(edges[i].V1) != union.Find(edges[i].V2))
                {
                    union.Union(edges[i].V1, edges[i].V2);
                    if (Math.Abs(edges[i].V1 - edges[i].V2) == 1)
                    {
                        passages[i] = GenerateHorizontalPassage(rooms[edges[i].V1], rooms[edges[i].V2]);
                    }
                    else
                    {
                        passages[i] = GenerateVerticalPassage(rooms[edges[i].V1], rooms[edges[i].V2]);
                    }
                }
            }

            return passages;
        }
    }
}