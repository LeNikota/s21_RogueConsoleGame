namespace domain.generation
{
    // public readonly record struct Object /*Entity*/(Vector2i Position, Vector2i Size);
    // public readonly record struct Rectangle(Vector2i Position, Vector2i Size); // As one struct replacing Object, Room and Passage

    class Object /*Entity*/(Vector2i position, Vector2i size)
    {
        // Upper left position point of an object
        public Vector2i Position = position;
        public Vector2i Size = size;

        public bool HasPoint(Vector2i point)
        {
            return point.X >= Position.X && point.X <= Size.X && point.Y >= Position.Y && point.Y <= Size.Y;
        }

        public override string ToString()
        {
            return $"Position {Position.X}, {Position.Y};\nSize {Size.X}, {Size.Y}";
        }
    }

    class Room(Vector2i position, Vector2i size) : Object(position, size) { }

    class Passage(Vector2i position, Vector2i size, Vector2i entrance, Vector2i exit) : Object(position, size)
    {
        public Vector2i Entrance = entrance;
        public Vector2i Exit = exit;
    }

    public readonly record struct Edge(int V1, int V2);

    static class LevelGenerator
    {
        // -- Room generation --
        public const int ROOM_COUNT = 9;
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

        public static Room[] GenerateRooms()
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

        static Passage GeneratePassage(int x, int y, int width, int height, Vector2i entrance, Vector2i exit)
        {
            var position = new Vector2i(x, y);
            var size = new Vector2i(width, height);

            return new Passage(position, size, entrance, exit);
        }

        static List<Passage> GenerateHorizontalPassage(Room first_room, Room second_room)
        {
            List<Passage> passages = [];

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
                passages.Add(GeneratePassage(first_x, first_y, width, 3, new Vector2i(first_x, first_y + 1), new Vector2i(second_x, first_y + 1)));
            }
            else if (x_diff == 2)
            {
                int height = Math.Abs(second_y - first_y) + 3;
                passages.Add(GeneratePassage(first_x, min_y, 3, height, new Vector2i(first_x, first_y + 1), new Vector2i(second_x, second_y + 1)));
            }
            else if (x_diff >= 3 && x_diff <= 5)
            {
                int height = Math.Abs(second_y - first_y) + 3;
                passages.Add(GeneratePassage(first_x, min_y, x_diff + 1, height, new Vector2i(first_x, first_y + 1), new Vector2i(second_x, second_y + 1)));

                // Old: Two separate passage
                // passages.Add(GeneratePassage(first_x, min_y, 3, height, new Vector2i(), new Vector2i()));
                // passages.Add(GeneratePassage(first_x + 2, second_y, x_diff - 1, 3, new Vector2i(), new Vector2i()));
            }
            else
            {
                // 1. Horizontal from first room to bend
                int bend_x = rand.Next(min_x + 2, max_x - 3);
                passages.Add(GeneratePassage(first_x, first_y, Math.Abs(bend_x - first_x) + 1, 3, new Vector2i(first_x, first_y + 1), new Vector2i(bend_x, first_y + 1)));

                // 2. Vertical bend
                passages.Add(GeneratePassage(bend_x, min_y, 3, Math.Abs(max_y - min_y) + 3, new Vector2i(bend_x, first_y + 1), new Vector2i(bend_x + 2, second_y + 1)));

                // 3. Horizontal from bend to second room
                passages.Add(GeneratePassage(bend_x + 2, second_y, Math.Abs(second_x - bend_x) - 1, 3, new Vector2i(bend_x + 2, second_y + 1), new Vector2i(second_x, second_y + 1)));
            }

            return passages;
        }

        static List<Passage> GenerateVerticalPassage(Room first_room, Room second_room)
        {
            List<Passage> passages = [];

            int first_y = first_room.Position.Y + first_room.Size.Y - 1;
            int left_range_x = first_room.Position.X;
            int right_range_x = first_room.Position.X + first_room.Size.X - 3;
            int first_x = rand.Next(left_range_x, right_range_x + 1);

            int second_y = second_room.Position.Y;
            left_range_x = second_room.Position.X;
            right_range_x = second_room.Position.X + second_room.Size.X - 3;
            int second_x = rand.Next(left_range_x, right_range_x + 1);

            int min_x = Math.Min(first_x, second_x);
            int max_x = Math.Max(first_x, second_x);
            int min_y = Math.Min(first_y, second_y);
            int max_y = Math.Max(first_y, second_y);
            int y_diff = Math.Abs(max_y - min_y);

            // Generate linear or non-linear passage
            if (first_x == second_x)
            {
                int height = Math.Abs(second_y - first_y) + 1;
                passages.Add(GeneratePassage(first_x, first_y, 3, height, new Vector2i(first_x + 1, first_y), new Vector2i(second_x + 1, second_y)));
            }
            else if (y_diff == 2)
            {
                int width = Math.Abs(max_x - min_x) + 3;
                passages.Add(GeneratePassage(min_x, first_y, width, 3, new Vector2i(first_x + 1, first_y), new Vector2i(second_x + 1, second_y)));
            }
            else if (y_diff >= 3 && y_diff <= 5)
            {
                int width = Math.Abs(max_x - min_x) + 3;
                passages.Add(GeneratePassage(min_x, first_y, width, y_diff + 1, new Vector2i(first_x + 1, first_y), new Vector2i(second_x + 1, second_y)));

                // Old: Two separate passage
                // passages.Add(GeneratePassage(min_x, first_y, width, 3, new Vector2i(), new Vector2i()));
                // passages.Add(GeneratePassage(second_x, first_y + 2, 3, y_diff - 1, new Vector2i(), new Vector2i()));
            }
            else
            {
                // 1. Horizontal from first room to bend
                int bend_y = rand.Next(min_y + 2, max_y - 3);
                passages.Add(GeneratePassage(first_x, first_y, 3, Math.Abs(bend_y - first_y) + 1, new Vector2i(first_x + 1, first_y), new Vector2i(first_x + 1, bend_y)));

                // // 2. Vertical bend
                passages.Add(GeneratePassage(min_x, bend_y, Math.Abs(max_x - min_x) + 3, 3, new Vector2i(first_x + 1, bend_y), new Vector2i(second_x + 1, bend_y + 2)));

                // // 3. Horizontal from bend to second room
                passages.Add(GeneratePassage(second_x, bend_y + 2, 3, Math.Abs(second_y - bend_y) - 1, new Vector2i(second_x + 1, bend_y + 2), new Vector2i(second_x + 1, second_y)));
            }

            return passages;
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
                        passages.Add(GenerateHorizontalPassage(rooms[edge.V1], rooms[edge.V2]));
                    }
                    else
                    {
                        passages.Add(GenerateVerticalPassage(rooms[edge.V1], rooms[edge.V2]));
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


        public static bool[,] ConvertToArray(Room[] rooms, List<List<Passage>> passages, int WIDTH, int HEIGHT)
        {
            bool[,] is_wall = new bool[HEIGHT, WIDTH];

            // Initialize all to false
            Array.Clear(is_wall, 0, is_wall.Length);

            foreach (var room in rooms)
            {
                if (room == null) continue;

                int startX = room.Position.X;
                int startY = room.Position.Y;
                int endX = startX + room.Size.X;
                int endY = startY + room.Size.Y;

                // Bounds check
                if (startX < 0 || startY < 0 || endX > WIDTH || endY > HEIGHT) continue;

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

            foreach (var passageGroup in passages)
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

                    is_wall[pass.Entrance.Y, pass.Entrance.X] = false;
                    is_wall[pass.Exit.Y, pass.Exit.X] = false;
                }
            }

            return is_wall;
        }
    }
}