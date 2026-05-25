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
    readonly int floor;

    readonly Room[] rooms;
    readonly List<List<Passage>> passages;
    

    public Level(int _floor, Player _player, int enemy_num = 5, int item_num = 3)
    {
        player = _player;
        floor = _floor;
        
        rooms = LevelGenerator.GenerateRooms();
        passages = LevelGenerator.GeneratePassages(rooms);
        is_wall = LevelGenerator.ConvertToArray(rooms, passages, WIDTH, HEIGHT);
        
        player.position = GetRandPosInRoom();
        entities.Add(player);

        for (int i = 0; i < enemy_num; i++)
        {
            Enemy e = Enemy.GetRandomEnemy();
            e.position = GetRandPosInRoom();
            while(e.position == player.position) e.position = GetRandPosInRoom();
            entities.Add(e);  
        }

        for (int j = 0; j < item_num; j++)
        {
            Item i = Item.GetRandomItem();
            i.position = GetRandPosInRoom();
            while(i.position == player.position) i.position = GetRandPosInRoom();
            entities.Add(i);  
        }
    }

    public bool IsPositionValid(Vector2i pos)
    {
        if (pos.x < 0 || pos.y < 0 || pos.x >= WIDTH || pos.y >= HEIGHT || is_wall[pos.y, pos.x]) return false;
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

    public Vector2i GetRandPosInRoom(int? room_id = null)
    {
        Room room = room_id == null ? rooms[Random.Shared.Next(0, LevelGenerator.ROOM_COUNT)] : rooms[(int)room_id];
        
        int upper_left_x = room.Position.X + 1;
        int upper_left_y = room.Position.Y + 1;

        int bottom_right_x = upper_left_x + room.Size.X - 2;
        int bottom_right_y = upper_left_y + room.Size.Y - 2;

        return new Vector2i(
            Random.Shared.Next(upper_left_x, bottom_right_x),
            Random.Shared.Next(upper_left_y, bottom_right_y));
        
        // entity.position.x = Random.Shared.Next(upper_left_x, bottom_right_x);
        // entity.position.y = Random.Shared.Next(upper_left_y, bottom_right_y);
    }
}


}