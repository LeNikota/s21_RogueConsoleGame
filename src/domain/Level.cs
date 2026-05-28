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
        passages = LevelGenerator.GeneratePassages(rooms);
        is_wall = LevelGenerator.ConvertToArray(rooms, passages, WIDTH, HEIGHT);
        
        int player_room = Random.Shared.Next(LevelGenerator.ROOM_COUNT);
        player.position = GetRandPosInRoom([player_room]);
        entities.Add(player);
        
        enemy_num += floor;
        item_num += floor;

        List<int> available_rooms = [..Enumerable.Range(0, LevelGenerator.ROOM_COUNT).Where(r => r != player_room)];
        entities.Add(new Door(0, 0, true) {position = GetRandPosInRoom(available_rooms)});
        
        for (int i = 0; i < enemy_num; i++)
        {
            Vector2i pos = GetRandPosInRoom(available_rooms);
            entities.Add(Enemy.GetRandomEnemy(pos));  
        }

        for (int j = 0; j < item_num; j++)
        {
            entities.Add(Item.GetRandomItem(GetRandPosInRoom(available_rooms)));  
        }
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
        foreach(var e in entities)
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
            while(available_rooms.Count > 0)
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
        HashSet<Vector2i> taken = [..entities.Where(e => room.HasPoint(e.position)).Select(e => e.position)];
        
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