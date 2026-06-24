namespace domain.generation
{
    // enum Placement { Left, Up, Right, Down}
    // enum Orientation { Horizontal, Vertical}

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

    class Room(Vector2i position, Vector2i size, int index) : Object(position, size)
    {
        public int Index = index;
        public List<Passage> Passages = [];
    }

    class Passage(Vector2i position, Vector2i size, Vector2i entrance, Vector2i exit, List<Room> rooms) : Object(position, size)
    {
        public Vector2i Entrance = entrance;
        public Vector2i Exit = exit;
        public List<Room> Rooms = rooms;
        public Door? Door = null;

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

        // Global variables for functions
        static Room[]? rooms;
        static List<List<Passage>>? passages;
        static List<Entity>? entities;

        // Changes on level number and player gameplay
        static int enemy_num = 1;
        static int item_num = 3;

        static public (Room[], List<List<Passage>>, List<Entity>, bool[,]) Generate(int width, int height, int floor, Player player)
        {
            rooms = GenerateRooms();
            passages = GeneratePassages();
            entities = [];
            bool[,] is_wall = ConvertToArray(width, height);


            int player_room_index = Random.Shared.Next(ROOM_COUNT);
            var player_position = GetRandPosInRoom([player_room_index]);
            player.position.X = player_position.X;
            player.position.Y = player_position.Y;
            entities.Add(player);

            // TODO: place into separate functions
            // Entity and items generations
            // enemy_num += floor;
            // item_num += floor;

            List<int> available_rooms = [.. Enumerable.Range(0, ROOM_COUNT).Where(r => r != player_room_index)];
            entities.Add(new Door(0, 0, true) { position = GetRandPosInRoom(available_rooms) });

            for (int i = 0; i < enemy_num; i++)
            {
                Vector2i pos = GetRandPosInRoom(available_rooms);
                entities.Add(Enemy.GetRandomEnemy(pos));
            }

            for (int j = 0; j < item_num; j++)
            {
                entities.Add(Item.GetRandomItem(GetRandPosInRoom(available_rooms)));
            }

            List<Door> doors = GenerateDoorsAndKeys(rooms[player_room_index]);
            entities.AddRange(doors);
            doors.ForEach(door =>
            {
                if (door.Key != null)
                    entities.Add(door.Key);
            });

            return (rooms, passages, entities, is_wall);
        }

        static Room[] GenerateRooms()
        {
            var rooms = new Room[ROOM_COUNT];

            for (int i = 0; i < ROOM_COUNT; i++)
            {
                int room_width = Random.Shared.Next(MIN_ROOM_WIDTH, MAX_ROOM_WIDTH + 1);
                int room_height = Random.Shared.Next(MIN_ROOM_HEIGH, MAX_ROOM_HEIGH + 1);


                int left_range_x = (i % GRID_DIMENSIONS) * REGION_WIDTH;
                int right_range_x = (i % GRID_DIMENSIONS + 1) * REGION_WIDTH - room_width - GRID_OFFSET_WIDTH;
                int x = Random.Shared.Next(left_range_x, right_range_x + 1);

                int up_range_y = (i / GRID_DIMENSIONS) * REGION_HEIGHT;
                int down_range_y = (i / GRID_DIMENSIONS + 1) * REGION_HEIGHT - room_height - GRID_OFFSET_HEIGHT;
                int y = Random.Shared.Next(up_range_y, down_range_y + 1);

                rooms[i] = new Room(new Vector2i(x, y), new Vector2i(room_width, room_height), i);
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

        static Passage GeneratePassage(int x, int y, int width, int height, Vector2i entrance, Vector2i exit, List<Room> room)
        {
            var position = new Vector2i(x, y);
            var size = new Vector2i(width, height);

            return new Passage(position, size, entrance, exit, room);
        }

        static List<Passage> GenerateHorizontalPassage(Room first_room, Room second_room)
        {
            List<Passage> passages = [];

            int first_x = first_room.Position.X + first_room.Size.X - 1;
            int up_range_y = first_room.Position.Y;
            int bottom_range_y = first_room.Position.Y + first_room.Size.Y - 3;
            int first_y = Random.Shared.Next(up_range_y, bottom_range_y + 1);

            int second_x = second_room.Position.X;
            up_range_y = second_room.Position.Y;
            bottom_range_y = second_room.Position.Y + second_room.Size.Y - 3;
            int second_y = Random.Shared.Next(up_range_y, bottom_range_y + 1);


            int min_x = Math.Min(first_x, second_x);
            int max_x = Math.Max(first_x, second_x);
            int min_y = Math.Min(first_y, second_y);
            int max_y = Math.Max(first_y, second_y);
            int x_diff = Math.Abs(max_x - min_x);

            // Generate linear or non-linear passage
            if (first_y == second_y)
            {
                int width = Math.Abs(second_x - first_x) + 1;
                Passage passage = GeneratePassage(first_x, first_y, width, 3, new Vector2i(first_x, first_y + 1), new Vector2i(second_x, first_y + 1), new List<Room>([first_room, second_room]));
                first_room.Passages.Add(passage);
                second_room.Passages.Add(passage);
                passages.Add(passage);
            }
            else if (x_diff == 2)
            {
                int height = Math.Abs(second_y - first_y) + 3;
                Passage passage = GeneratePassage(first_x, min_y, 3, height, new Vector2i(first_x, first_y + 1), new Vector2i(second_x, second_y + 1), new List<Room>([first_room, second_room]));
                first_room.Passages.Add(passage);
                second_room.Passages.Add(passage);
                passages.Add(passage);
            }
            else if (x_diff >= 3 && x_diff <= 5)
            {
                int height = Math.Abs(second_y - first_y) + 3;
                Passage passage = GeneratePassage(first_x, min_y, x_diff + 1, height, new Vector2i(first_x, first_y + 1), new Vector2i(second_x, second_y + 1), new List<Room>([first_room, second_room]));
                first_room.Passages.Add(passage);
                second_room.Passages.Add(passage);
                passages.Add(passage);

                // Old: Two separate passage
                // passages.Add(GeneratePassage(first_x, min_y, 3, height, new Vector2i(), new Vector2i()));
                // passages.Add(GeneratePassage(first_x + 2, second_y, x_diff - 1, 3, new Vector2i(), new Vector2i()));
            }
            else
            {
                // 1. Horizontal from first room to bend
                int bend_x = Random.Shared.Next(min_x + 2, max_x - 3);
                Passage passage = GeneratePassage(first_x, first_y, Math.Abs(bend_x - first_x) + 1, 3, new Vector2i(first_x, first_y + 1), new Vector2i(bend_x, first_y + 1), new List<Room>([first_room, second_room]));
                first_room.Passages.Add(passage);
                second_room.Passages.Add(passage);
                passages.Add(passage);

                // 2. Vertical bend
                passage = GeneratePassage(bend_x, min_y, 3, Math.Abs(max_y - min_y) + 3, new Vector2i(bend_x, first_y + 1), new Vector2i(bend_x + 2, second_y + 1), new List<Room>([first_room, second_room]));
                passages.Add(passage);

                // 3. Horizontal from bend to second room
                passage = GeneratePassage(bend_x + 2, second_y, Math.Abs(second_x - bend_x) - 1, 3, new Vector2i(bend_x + 2, second_y + 1), new Vector2i(second_x, second_y + 1), new List<Room>([first_room, second_room]));
                passages.Add(passage);
            }

            return passages;
        }

        static List<Passage> GenerateVerticalPassage(Room first_room, Room second_room)
        {
            List<Passage> passages = [];

            int first_y = first_room.Position.Y + first_room.Size.Y - 1;
            int left_range_x = first_room.Position.X;
            int right_range_x = first_room.Position.X + first_room.Size.X - 3;
            int first_x = Random.Shared.Next(left_range_x, right_range_x + 1);

            int second_y = second_room.Position.Y;
            left_range_x = second_room.Position.X;
            right_range_x = second_room.Position.X + second_room.Size.X - 3;
            int second_x = Random.Shared.Next(left_range_x, right_range_x + 1);

            int min_x = Math.Min(first_x, second_x);
            int max_x = Math.Max(first_x, second_x);
            int min_y = Math.Min(first_y, second_y);
            int max_y = Math.Max(first_y, second_y);
            int y_diff = Math.Abs(max_y - min_y);

            // Generate linear or non-linear passage
            if (first_x == second_x)
            {
                int height = Math.Abs(second_y - first_y) + 1;
                Passage passage = GeneratePassage(first_x, first_y, 3, height, new Vector2i(first_x + 1, first_y), new Vector2i(second_x + 1, second_y), new List<Room>([first_room, second_room]));
                first_room.Passages.Add(passage);
                second_room.Passages.Add(passage);
                passages.Add(passage);
            }
            else if (y_diff == 2)
            {
                int width = Math.Abs(max_x - min_x) + 3;
                Passage passage = GeneratePassage(min_x, first_y, width, 3, new Vector2i(first_x + 1, first_y), new Vector2i(second_x + 1, second_y), new List<Room>([first_room, second_room]));
                first_room.Passages.Add(passage);
                second_room.Passages.Add(passage);
                passages.Add(passage);
            }
            else if (y_diff >= 3 && y_diff <= 5)
            {
                int width = Math.Abs(max_x - min_x) + 3;
                Passage passage = GeneratePassage(min_x, first_y, width, y_diff + 1, new Vector2i(first_x + 1, first_y), new Vector2i(second_x + 1, second_y), new List<Room>([first_room, second_room]));
                first_room.Passages.Add(passage);
                second_room.Passages.Add(passage);
                passages.Add(passage);

                // Old: Two separate passage
                // passages.Add(GeneratePassage(min_x, first_y, width, 3, new Vector2i(), new Vector2i()));
                // passages.Add(GeneratePassage(second_x, first_y + 2, 3, y_diff - 1, new Vector2i(), new Vector2i()));
            }
            else
            {
                // 1. Horizontal from first room to bend
                int bend_y = Random.Shared.Next(min_y + 2, max_y - 3);
                Passage passage = GeneratePassage(first_x, first_y, 3, Math.Abs(bend_y - first_y) + 1, new Vector2i(first_x + 1, first_y), new Vector2i(first_x + 1, bend_y), new List<Room>([first_room, second_room]));
                first_room.Passages.Add(passage);
                second_room.Passages.Add(passage);
                passages.Add(passage);

                // // 2. Vertical bend
                passage = GeneratePassage(min_x, bend_y, Math.Abs(max_x - min_x) + 3, 3, new Vector2i(first_x + 1, bend_y), new Vector2i(second_x + 1, bend_y + 2), new List<Room>([first_room, second_room]));
                passages.Add(passage);

                // // 3. Horizontal from bend to second room
                passage = GeneratePassage(second_x, bend_y + 2, 3, Math.Abs(second_y - bend_y) - 1, new Vector2i(second_x + 1, bend_y + 2), new Vector2i(second_x + 1, second_y), new List<Room>([first_room, second_room]));
                passages.Add(passage);
            }

            return passages;
        }

        static List<List<Passage>> GeneratePassages()
        {
            var passages = new List<List<Passage>>();

            var edges = GenerateRoomsEdges();
            Tools.Shuffle(edges);

            var union = new UnionFind(ROOM_COUNT);
            foreach (var edge in edges)
            {
                int v1 = edge.V1;
                int v2 = edge.V2;

                if (union.Find(v1) != union.Find(v2))
                {
                    union.Union(v1, v2);

                    if (Math.Abs(v1 - v2) == 1)
                    {
                        passages.Add(GenerateHorizontalPassage(rooms[v1], rooms[v2]));
                    }
                    else
                    {
                        passages.Add(GenerateVerticalPassage(rooms[v1], rooms[v2]));
                    }
                }
            }

            return passages;
        }

        static bool[,] ConvertToArray(int width, int height)
        {
            bool[,] is_wall = new bool[height, width];

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
                if (startX < 0 || startY < 0 || endX > width || endY > height) continue;

                // Draw Top and Bottom Walls (Horizontal lines)
                // Loop X from start to end. Index array as [Y, X]
                for (int x = startX; x < endX; x++)
                {
                    if (startY >= 0 && startY < height) is_wall[startY, x] = true;             // Top
                    if (endY - 1 >= 0 && endY - 1 < height) is_wall[endY - 1, x] = true;       // Bottom
                }

                // Draw Left and Right Walls (Vertical lines)
                // Loop Y from start to end. Index array as [Y, X]
                for (int y = startY; y < endY; y++)
                {
                    if (startX >= 0 && startX < width) is_wall[y, startX] = true;              // Left
                    if (endX - 1 >= 0 && endX - 1 < width) is_wall[y, endX - 1] = true;        // Right
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
                    if (startX < 0 || startY < 0 || endX > width || endY > height) continue;

                    // 1. Draw the Border (Walls)
                    // Top and Bottom rows
                    for (int x = startX; x < endX; x++)
                    {
                        if (startY >= 0 && startY < height) is_wall[startY, x] = true;       // Top
                        if (endY - 1 >= 0 && endY - 1 < height) is_wall[endY - 1, x] = true; // Bottom
                    }
                    // Left and Right columns
                    for (int y = startY; y < endY; y++)
                    {
                        if (startX >= 0 && startX < width) is_wall[y, startX] = true;       // Left
                        if (endX - 1 >= 0 && endX - 1 < width) is_wall[y, endX - 1] = true; // Right
                    }

                    is_wall[pass.Entrance.Y, pass.Entrance.X] = false;
                    is_wall[pass.Exit.Y, pass.Exit.X] = false;
                }
            }

            // Removes walls at the intersection of two passages when two passages overlap
            foreach (var passageGroup in passages)
            {
                foreach (var pass in passageGroup)
                {
                    if (pass == null) continue;

                    int startX = pass.Position.X + 1;
                    int startY = pass.Position.Y + 1;
                    int endX = startX + pass.Size.X - 2;
                    int endY = startY + pass.Size.Y - 2;

                    if (startX < 0 || startY < 0 || endX > width || endY > height) continue;

                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            is_wall[y, x] = false;
                        }
                    }
                }
            }

            return is_wall;
        }

        static List<Door> GenerateDoorsAndKeys(Room player_room)
        {
            List<Door> doors = [];
            List<Color> colors = [Color.Blue, Color.Magenta];
            int doorCount = Random.Shared.Next(0, colors.Count + 1);

            // 1. From player room start searching for a passage to block with a door
            // 2. Place a door somewhere, not behind the existing doors
            // 3. Place a key, not behind the existing doors
            // 4. Repeat for part 1, try to place more doors
            for (int i = 0; i < doorCount; i++)
            {
                //Place a door far away from the player room
                var passagesByDistance = GetPassagesByDistance(player_room);
                int maxDistance = passagesByDistance.Keys.Max();
                var availablePassages = passagesByDistance
                    .Where(kvp => kvp.Key == maxDistance)
                    .SelectMany(kvp => kvp.Value)
                    .ToList();
                Passage randomPassage = availablePassages[Random.Shared.Next(availablePassages.Count)];

                Door door = new(randomPassage.Entrance.X, randomPassage.Entrance.Y);
                doors.Add(door);
                randomPassage.Door = door;

                // Place a key, not behind the existing doors
                var roomsByDistance = GetRoomsByDistance(player_room);
                // Not near the player
                roomsByDistance.Remove(0);
                roomsByDistance.Remove(1);
                var availableRooms = roomsByDistance
                    .SelectMany(kvp => kvp.Value)
                    .ToList();

                // Not near the placed door
                availableRooms.Remove(randomPassage.Rooms[0]);
                availableRooms.Remove(randomPassage.Rooms[1]);
                Room randomRoom = availableRooms[Random.Shared.Next(availableRooms.Count)];

                Color color = colors[Random.Shared.Next(0, colors.Count)];
                colors.Remove(color);
                Vector2i pos = GetRandPosInRoom([randomRoom.Index]);
                Key key = new(pos.X, pos.Y, color);
                door.Key = key;
            }


            return doors;
        }

        static Dictionary<int, List<Passage>> GetPassagesByDistance(Room startRoom)
        {
            var result = new Dictionary<int, List<Passage>>();
            var visitedRooms = new HashSet<Room>();
            var queue = new Queue<(Room Room, int Distance)>();

            queue.Enqueue((startRoom, 0));
            visitedRooms.Add(startRoom);

            while (queue.Count > 0)
            {
                var (currentRoom, distance) = queue.Dequeue();

                if (!result.ContainsKey(distance))
                {
                    result[distance] = [];
                }

                // Add all passages connected to this room to the current distance bucket
                foreach (var passage in currentRoom.Passages)
                {
                    if (!result[distance].Contains(passage) && passage.Door == null)
                    {
                        result[distance].Add(passage);
                    }
                }

                // Explore neighbors
                foreach (var passage in currentRoom.Passages)
                {
                    if (passage.Door != null)
                        continue;

                    // Determine the other room connected by this passage
                    Room nextRoom = passage.Rooms[0] == currentRoom ? passage.Rooms[1] : passage.Rooms[0];

                    if (visitedRooms.Contains(nextRoom))
                        continue;

                    visitedRooms.Add(nextRoom);
                    queue.Enqueue((nextRoom, distance + 1));
                }
            }

            return result;
        }

        static Dictionary<int, List<Room>> GetRoomsByDistance(Room startRoom)
        {
            var result = new Dictionary<int, List<Room>>();
            var visited = new HashSet<int>();
            var queue = new Queue<(Room Room, int Distance)>();

            queue.Enqueue((startRoom, 0));
            visited.Add(startRoom.Index);

            result[0] = [startRoom];

            while (queue.Count > 0)
            {
                var (currentRoom, distance) = queue.Dequeue();
                int nextDistance = distance + 1;

                // Explore all passages connected to the current room
                foreach (var passage in currentRoom.Passages)
                {
                    if (passage.Door != null)
                        continue;

                    // Identify the neighbor room
                    Room neighbor = passage.Rooms[0] == currentRoom ? passage.Rooms[1] : passage.Rooms[0];

                    if (!visited.Contains(neighbor.Index))
                    {
                        visited.Add(neighbor.Index);
                        queue.Enqueue((neighbor, nextDistance));

                        // Add to result dictionary
                        if (!result.ContainsKey(nextDistance))
                            result[nextDistance] = [];

                        result[nextDistance].Add(neighbor);
                    }
                }
            }

            return result;
        }

        static Vector2i GetRandPosInRoom(List<int>? available_rooms = null)
        {
            Vector2i result = Vector2i.ZERO;

            if (available_rooms is null || available_rooms.Count == 0)
            {
                available_rooms = [.. Enumerable.Range(0, LevelGenerator.ROOM_COUNT)];
                while (available_rooms.Count > 0)
                {
                    if (result != Vector2i.ZERO) break;
                    int chosen = available_rooms[Random.Shared.Next(available_rooms.Count)];
                    available_rooms.Remove(chosen);
                    result = GetRandPosInRoom([chosen]);
                }
                return result;
            }

            Room room = rooms[available_rooms[Random.Shared.Next(available_rooms.Count)]];
            int upper_left_x = room.Position.X + 1;
            int upper_left_y = room.Position.Y + 1;

            int bottom_right_x = upper_left_x + room.Size.X - 2;
            int bottom_right_y = upper_left_y + room.Size.Y - 2;

            int max_tries = (bottom_right_x - upper_left_x) * (bottom_right_y - upper_left_y);
            HashSet<Vector2i> taken = [.. entities.Where(e => room.HasPoint(e.position)).Select(e => e.position)];

            for (int i = 0; i < max_tries; i++)
            {
                result = new(
                    Random.Shared.Next(upper_left_x, bottom_right_x),
                    Random.Shared.Next(upper_left_y, bottom_right_y));
                if (!taken.Contains(result)) break;
            }
            return result;
        }
    }
}