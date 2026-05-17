namespace domain
{
    
class Level
{
    public const int WIDTH = 128; //used to resize the terminal
    public const int HEIGHT = 32; //used to resize the terminal
    public readonly bool[,] is_wall;
    public List<Entity> entities;

    public Level()
    {
        is_wall = new bool[HEIGHT, WIDTH];
        for (int i = 0; i < HEIGHT; i++)
        {
            for (int j = 0; j < WIDTH; j++)
            {
                is_wall[i, j] = false;
            }
        }

        for (int i = 4; i <= 14; i++)
        {
            is_wall[i, 4] = true;   
            is_wall[i, 14] = true;
            is_wall[4, i] = true;  
            is_wall[14, i] = true; 
        }
        is_wall[3, 4] = true;
        is_wall[4, 3] = true;
        is_wall[4, 15] = true;
        is_wall[3, 14] = true;
        entities = [];
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
}


}