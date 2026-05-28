using System.Numerics;

namespace domain
{
class Enemy(int x, int y, uint _icon, (int hp, int s, int a, int h) stats) : Character(x, y, _icon, stats.hp, stats.s, stats.a)
{
    public enum StatFactor {LOW, MEDIUM, HIGH, VERY_HIGH};
    public int hostility = stats.h;

    public Enemy(int x, int y, uint _icon, StatFactor hp_f, StatFactor st_f, StatFactor ag_f, StatFactor host_f) : 
        this(x, y, _icon, GenerateStats(hp_f, st_f, ag_f, host_f)){}

    private static (int hp, int s, int a, int h) GenerateStats(StatFactor hp_f, StatFactor st_f, StatFactor ag_f, StatFactor host_f)
    {
        double health_factor = 1 + ((double)(hp_f + 1) * 0.1);
        double strength_factor = 1 + ((double)(st_f + 1) * 0.1);
        double agility_factor = 1 + ((double)(ag_f + 1) * 0.1);
        

        int health = Random.Shared.Next((int)Math.Round(50 * health_factor), (int)Math.Round(100 * health_factor));  
        int strength = (int)st_f + Random.Shared.Next((int)Math.Round(1 * strength_factor), (int)Math.Round(5 * strength_factor));  
        int agility = (int)ag_f + Random.Shared.Next((int)Math.Round(1 * agility_factor), (int)Math.Round(5 * agility_factor));
        int hostility = ((int)host_f + 2) * 2;
        return (health, strength, agility, hostility);
    }

    public static Enemy GetRandomEnemy(Vector2i? pos = null)
    {
        return Random.Shared.Next(0, 3) switch
        {
            0 => new Zombie(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
            1 => new Vampire(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
            2 => new Ghost(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
            3 => new Ogre(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
            _ => new Zombie(pos is null ? 0 : pos.X, pos is null ? 0 : pos.Y),
        };
    }

    public virtual void MovePeacefully(Level level)
    {
        return;
    }
}

class Zombie(int x = 0, int y = 0) : Enemy(x, y, 'z', StatFactor.HIGH, StatFactor.MEDIUM, StatFactor.LOW, StatFactor.MEDIUM)
{
    
}

class Vampire(int x = 0, int y = 0) : Enemy(x, y, 'v', StatFactor.HIGH, StatFactor.MEDIUM, StatFactor.HIGH, StatFactor.HIGH)
{
    public bool wasAttacked = false; //first hit is always a miss
}

class Ghost(int x = 0, int y = 0) : Enemy(x, y, 'g', StatFactor.LOW, StatFactor.LOW, StatFactor.HIGH, StatFactor.LOW)
{
    
}

class Ogre(int x = 0, int y = 0) : Enemy(x, y, 'O', StatFactor.VERY_HIGH, StatFactor.VERY_HIGH, StatFactor.LOW, StatFactor.MEDIUM)
{
    
}

class SnakeMage(int x = 0, int y = 0) : Enemy(x, y, 'z', StatFactor.MEDIUM, StatFactor.MEDIUM, StatFactor.VERY_HIGH, StatFactor.HIGH)
{
    
}

} //domain
