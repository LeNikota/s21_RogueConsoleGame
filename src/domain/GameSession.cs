
namespace domain {
class GameSession
{
    public enum InputAction { UP, DOWN, LEFT, RIGHT};
    private Level level;
    private Player player; //lookup not to look for it all the time


    public GameSession()
    {
        level = new Level();
        player = new Player(8, 8);
        level.entities.Add(player);
    }

    public Level GetLevel()
    {
        return level;
    }

    public void Input(InputAction action)
    {
        switch(action)
        {
            case InputAction.UP:
                if (player.position.y > 0) player.position.y -= 1;
                break;
            case InputAction.DOWN:
                if (player.position.y < Level.HEIGHT) player.position.y += 1;
                break;
            case InputAction.LEFT:
                if (player.position.x > 0) player.position.x -= 1;
                break;
            case InputAction.RIGHT:
                if (player.position.x < Level.WIDTH) player.position.x += 1;
                break;

        }
    }


}

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

        for (int i = 2; i <= 12; i++)
        {
            is_wall[i, 2] = true;   
            is_wall[i, 12] = true;
            is_wall[2, i] = true;  
            is_wall[12, i] = true; 
        }
        entities = [];
    }
}

class Entity(int x, int y, uint _icon)
    {
    public Vector2i position = new(x, y);
    public uint icon = _icon; //the one to be printed in console

    }

class Character(int x, int y, uint _icon, int h, int s, int a) : Entity(x, y, _icon)
{
    public int max_health = h;
    public int current_health = h;
    public int strength = s;
    public int agility = a;
}

class Player(int x, int y) : Character(x, y, '@', 100, 5, 5)
{

}

class Vector2i(int _x, int _y)
{
    public int x = _x;
    public int y = _y;

    public double DistanceTo(Vector2i other)
    {
        return Math.Sqrt(Math.Pow(other.x - x, 2) + Math.Pow(other.y - y, 2));
    }

};

} //domain
