using System.Text.Json.Serialization;

namespace domain;

[JsonDerivedType(typeof(Zombie))]
[JsonDerivedType(typeof(Vampire))]
[JsonDerivedType(typeof(Ghost))]
[JsonDerivedType(typeof(Ogre))]
[JsonDerivedType(typeof(SnakeMage))]

class Enemy : Character
{
    public enum StatFactor { LOW, MEDIUM, HIGH, VERY_HIGH };

    public int Hostility { get; set; }

    // Parameterless constructor required for deserialization
    public Enemy() : base() { }

    // Primary constructor for game logic
    public Enemy(int x, int y, uint _icon, (int hp, int s, int a, int h) stats, Color color = Color.None)
        : base(x, y, _icon, stats.hp, stats.s, stats.a, color)
    {
        Hostility = stats.h;
    }

    // Secondary constructor using StatFactors
    public Enemy(int x, int y, uint _icon, StatFactor hp_f, StatFactor st_f, StatFactor ag_f, StatFactor host_f, Color color = Color.None) :
        this(x, y, _icon, GenerateStats(hp_f, st_f, ag_f, host_f), color)
    { }

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
        int x = pos?.X ?? 0;
        int y = pos?.Y ?? 0;

        return Random.Shared.Next(0, 5) switch
        {
            0 => new Zombie(x, y),
            1 => new Vampire(x, y),
            2 => new Ghost(x, y),
            3 => new Ogre(x, y),
            4 => new SnakeMage(x, y),
            _ => new Zombie(x, y),
        };
    }

    public virtual void MovePeacefully(Level level)
    {
        // Default implementation: do nothing or move randomly
        return;
    }
}

class Zombie : Enemy
{
    public Zombie() : base() { }

    public Zombie(int x = 0, int y = 0) : base(x, y, 'z', StatFactor.HIGH, StatFactor.MEDIUM, StatFactor.LOW, StatFactor.MEDIUM, Color.Green) { }
}

class Vampire : Enemy
{
    public bool WasAttacked { get; set; } = false;

    public Vampire() : base() { }

    public Vampire(int x = 0, int y = 0) : base(x, y, 'v', StatFactor.HIGH, StatFactor.MEDIUM, StatFactor.HIGH, StatFactor.HIGH, Color.Red) { }
}

class Ghost : Enemy
{
    public Ghost() : base() { }

    public Ghost(int x = 0, int y = 0) : base(x, y, 'g', StatFactor.LOW, StatFactor.LOW, StatFactor.HIGH, StatFactor.LOW, Color.White) { }
}

class Ogre : Enemy
{
    public Ogre() : base() { }

    public Ogre(int x = 0, int y = 0) : base(x, y, 'o', StatFactor.VERY_HIGH, StatFactor.VERY_HIGH, StatFactor.LOW, StatFactor.MEDIUM, Color.Yellow) { }
}

class SnakeMage : Enemy
{
    public SnakeMage() : base() { }

    public SnakeMage(int x = 0, int y = 0) : base(x, y, 's', StatFactor.MEDIUM, StatFactor.MEDIUM, StatFactor.VERY_HIGH, StatFactor.HIGH, Color.White) { }
}