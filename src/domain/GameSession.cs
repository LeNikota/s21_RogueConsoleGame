

namespace domain {
class GameSession
{
    public enum InputAction { UP, DOWN, LEFT, RIGHT};
    public readonly Dictionary<InputAction, Vector2i> InputDirectionMap = new()
    {
        {InputAction.UP, Vector2i.UP},
        {InputAction.DOWN, Vector2i.DOWN},
        {InputAction.LEFT, Vector2i.LEFT},
        {InputAction.RIGHT, Vector2i.RIGHT}
    };

    private readonly Level level;
    private readonly Player player; //lookup not to look for it all the time

    public bool isOver = false;

    public GameSession()
    {
        level = new Level();
        player = new Player(8, 8);
        level.entities.Add(player);
        level.entities.Add(new Zombie(8, 10));
        level.entities.Add(new Vampire(10, 8));
        level.entities.Add(new Ogre(10, 10));
        level.entities.Add(new Ghost(12, 12));
    }

    public Level GetLevel()
    {
        return level;
    }

    public void Input(InputAction action)
    {
        if (action == InputAction.UP || action == InputAction.DOWN || action == InputAction.LEFT || action == InputAction.RIGHT)
        {
            Vector2i new_pos = player.position + InputDirectionMap[action];
            if (!level.IsPositionValid(new_pos)) return;
            Entity? entity = level.GetEntityByPos(new_pos);
            if (entity == null)
            {
                player.position = new_pos;
            } 
            else if (entity is Enemy)
            {
                bool isDead = Attack(player, (Character)entity);
                if (isDead)
                {
                    level.entities.Remove(entity);
                    //TO DO: GIVE PLAYER A TREASURE here
                    return;
                }
                isDead = Attack((Character)entity, player);
                isOver = isDead;
            } 
            //else (entity as items are not implemented yet)
            

        }
        //handle non-movement input here:

    }

    private static bool Attack(Character c1, Character c2)
    {
        var rand = new Random();
        int roll = rand.Next(21);
        if (roll + c1.Agility >= 10 + c2.Agility)
        {
            return c2.Hit(c1.GetDamage()); //returns if it is dead
        }
        return false;
    }

}

class Vector2i(int _x, int _y)
{
    public int x = _x;
    public int y = _y;

    public static readonly Vector2i ZERO = new(0, 0);

    public static readonly Vector2i UP = new(0, -1);
    public static readonly Vector2i DOWN = new(0, 1);
    public static readonly Vector2i LEFT = new(-1, 0);
    public static readonly Vector2i RIGHT = new(1, 0);

    public static Vector2i operator +(Vector2i a, Vector2i b)
    {
        return new Vector2i(a.x + b.x, a.y + b.y);
    }

    public static bool operator ==(Vector2i a, Vector2i b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Vector2i a, Vector2i b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public double DistanceTo(Vector2i other)
    {
        return Math.Sqrt(Math.Pow(other.x - x, 2) + Math.Pow(other.y - y, 2));
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return this == (Vector2i)obj;
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}


} //domain
