using System.Text.Json.Serialization;
using domain.generation;

namespace domain;

[JsonDerivedType(typeof(Zombie), "zombie")]
[JsonDerivedType(typeof(Vampire), "vampire")]
[JsonDerivedType(typeof(Ghost), "ghost")]
[JsonDerivedType(typeof(Ogre), "ogre")]
[JsonDerivedType(typeof(SnakeMage), "snakeMage")]
[JsonDerivedType(typeof(Mimic), "mimic")]

class Enemy : Character
{
    public enum StatFactor { LOW, MEDIUM, HIGH, VERY_HIGH };
    public int Hostility { get; set; }

    public Enemy() : base() { }

    public Enemy(int x, int y, uint _icon, (int hp, int s, int a, int h) stats, Color color = Color.None)
    : base(x, y, _icon, stats.hp, stats.s, stats.a, color)
    {
        Hostility = stats.h;
    }

    public Enemy(int x, int y, uint _icon, StatFactor hp_f, StatFactor st_f, StatFactor ag_f, StatFactor host_f, Color color = Color.None) :
    this(x, y, _icon, GenerateStats(hp_f, st_f, ag_f, host_f), color)
    { }

    private static (int hp, int s, int a, int h) GenerateStats(StatFactor hp_f, StatFactor st_f, StatFactor ag_f, StatFactor host_f, int floor = 0)
    {
        // Basic multipliers
        double health_factor = 1 + ((double)(hp_f + 1) * 0.1);
        double strength_factor = 1 + ((double)(st_f + 1) * 0.1);
        double agility_factor = 1 + ((double)(ag_f + 1) * 0.1);

        // Task 2: Global difficulty multiplier by floor
        // +5% stats for each floor
        double floorMultiplier = 1.0 + (floor * 0.05);

        int health = (int)(Random.Shared.Next((int)Math.Round(50 * health_factor), (int)Math.Round(100 * health_factor)) * floorMultiplier);
        int strength = (int)(((int)st_f + Random.Shared.Next((int)Math.Round(1 * strength_factor), (int)Math.Round(5 * strength_factor))) * floorMultiplier);
        int agility = (int)(((int)ag_f + Random.Shared.Next((int)Math.Round(1 * agility_factor), (int)Math.Round(5 * agility_factor))) * floorMultiplier);
        int hostility = ((int)host_f + 2) * 2;

        return (health, strength, agility, hostility);
    }

    public static Enemy GetRandomEnemy(Vector2i? pos = null)
    {
        int x = pos?.X ?? 0;
        int y = pos?.Y ?? 0;

        return Random.Shared.Next(6) switch
        {
            0 => new Zombie(x, y),
            1 => new Vampire(x, y),
            2 => new Ghost(x, y),
            3 => new Ogre(x, y),
            4 => new SnakeMage(x, y),
            5 => new Mimic(x, y),
            _ => new Zombie(x, y),
        };
    }

    public static Enemy GetRandomEnemyByFloor(Vector2i? pos = null, int floor = 1)
    {
        int x = pos?.X ?? 0;
        int y = pos?.Y ?? 0;

        int roll = Random.Shared.Next(100);

        if (floor < 5)
        {
            if (roll < 30) return new Zombie(x, y);
            if (roll < 70) return new Ghost(x, y);
            return new Mimic(x, y);
        }
        else if (floor < 12)
        {
            if (roll < 25) return new Zombie(x, y);
            if (roll < 50) return new Ghost(x, y);
            if (roll < 75) return new Mimic(x, y);
            if (roll < 85) return new Ogre(x, y);
            if (roll < 95) return new SnakeMage(x, y);
            return new Vampire(x, y);
        }
        else
        {
            if (roll < 15) return new Zombie(x, y);
            if (roll < 30) return new Ghost(x, y);
            if (roll < 45) return new Mimic(x, y);
            if (roll < 65) return new Ogre(x, y);
            if (roll < 85) return new SnakeMage(x, y);
            return new Vampire(x, y);
        }
    }

    public virtual void MovePeacefully(Level level)
    {
        var directions = Vector2i.SIMPLE_DIRECTIONS;

        Tools.Shuffle(directions);

        foreach (var dir in directions)
        {
            Vector2i newPos = Position + dir;

            if (level.IsPositionValid(newPos))
            {
                if (level.GetEntityByPos(newPos) == null)
                {
                    Position = newPos;
                    return;
                }
            }
        }
    }

    public virtual void Act(Player player, Level level, GameStatistics stats)
    {
        if (Position.DistanceTo(player.Position) > Hostility)
        {
            MovePeacefully(level);
            return;
        }

        List<Vector2i> path = PathFinder.FindPath(level, Position, player.Position);

        if (path.Count == 0)
        {
            MovePeacefully(level);
            return;
        }

        if (path[0] == player.Position)
        {
            Attack(player);
            stats.HitsTaken++;
        }
        else
        {
            Position = path[0];
        }
    }
}