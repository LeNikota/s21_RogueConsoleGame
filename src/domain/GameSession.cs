using System.Numerics;

namespace domain {
class GameSession
{
    enum InputActions { UP, DOWN, LEFT, RIGHT};
    private Level level;


    public GameSession()
    {
        level = new Level();
        level.entities.Add(new Player(8, 8, "P"));
    }

    public Level GetLevel()
    {
        return level;
    }

}

class Level
{
    public const int LEVEL_SIZE = 16; //BOTH LENGTH AND WIDTH
    public readonly bool[,] is_wall;
    public List<Entity> entities;

    public Level()
    {
        is_wall = new bool[LEVEL_SIZE, LEVEL_SIZE];
        for (int i = 0; i < LEVEL_SIZE; i++)
        {
            for (int j = 0; j < LEVEL_SIZE; j++)
            {
                is_wall[i, j] = false;
            }
        }

        for (int i = 4; i < 13; i++)
        {
            is_wall[i, 2] = true;
            is_wall[i, 12] = true;
            is_wall[2, i] = true;
            is_wall[12, i] = true;
        }
        entities = [];
    }
}

class Entity
{
    public Vector2i position;
    public string icon; //the one to be printed in console

    public Entity(int x, int y, string _icon)
    {
        position = new(x, y);
        icon = _icon;
    }

}

class Character(int x, int y, string _icon, int h, int s, int a) : Entity(x, y, _icon)
{
    public int max_health = h;
    public int current_health = h;
    public int strength = s;
    public int agility = a;
}

class Player(int x, int y, string _icon) : Character(x, y, _icon, 100, 5, 5)
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
