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
        public readonly Player player; //lookup not to look for it all the time
        public readonly int floor;
        readonly Room[] rooms;
        readonly List<List<Passage>> passages;

        public Level(int _floor, Player _player, int enemy_num = 1, int item_num = 3)
        {
            player = _player;
            floor = _floor;

            rooms = LevelGenerator.GenerateRooms();
            passages= LevelGenerator.GeneratePassages(rooms);
            is_wall = LevelGenerator.ConvertToArray(rooms, passages, WIDTH, HEIGHT);

            int player_room_index = Random.Shared.Next(LevelGenerator.ROOM_COUNT);
            player.position = GetRandPosInRoom([player_room_index]);
            entities.Add(player);

            enemy_num += floor;
            item_num += floor;

            List<int> available_rooms = [.. Enumerable.Range(0, LevelGenerator.ROOM_COUNT).Where(r => r != player_room_index)];
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

            List<Door> doors = GenerateDoorsAndKeys(player_room_index);
            entities.AddRange(doors);
            doors.ForEach(door => entities.Add(door.Key));

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

        public Vector2i GetRandPosInRoom(List<int>? available_rooms = null)
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


        List<Door> GenerateDoorsAndKeys(int player_room_index)
        {
            List<Door> doors = [];
            List<Color> colors = [Color.Red, Color.Blue, Color.Yellow];
            int doorsCount = Random.Shared.Next(0, colors.Count);

            // Find player room by index
            Room player_room = rooms[player_room_index];

            // 1. From player room start searching for a passage to block with a door
            // 2. Place a door somewhere, not behind the existing doors
            // 3. Place a key, not behind the existing doors
            // 4. Repeat for part 1, try to place more doors
            double door_placement_likelihood = 0;
            var visited = new List<int>();
            var queue = new Queue<Room>();
            queue.Enqueue(player_room);
            visited.Add(player_room.Index);
            bool is_done = false;
            while (queue.Count > 0)
            {
                double r = Random.Shared.NextDouble();
                Room current_room = queue.Dequeue();

                foreach (var current_passage in current_room.Passages)
                {
                    if (current_passage.Door != null)
                        continue;

                    if (r < door_placement_likelihood)
                    {
                        Color color = colors[Random.Shared.Next(0, colors.Count)];
                        colors.Remove(color);
                        Vector2i pos = GetRandPosInRoom(visited);
                        Key key = new(pos.X, pos.Y, color);

                        Door door = new(current_passage.Entrance.X, current_passage.Entrance.Y, key: key);
                        doors.Add(door);

                        is_done = true;
                        break;
                    }

                    Room next_room = current_passage.Rooms[1] == current_room ? current_passage.Rooms[0] : current_passage.Rooms[1];
                    if (visited.Contains(next_room.Index))
                        continue;

                    visited.Add(next_room.Index);
                    queue.Enqueue(next_room);
                }
                if (is_done) break;

                door_placement_likelihood += 0.2;
            }



            return doors;
        }
    }
}