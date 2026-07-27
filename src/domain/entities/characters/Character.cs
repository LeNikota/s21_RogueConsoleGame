using System.Text.Json.Serialization;

namespace domain;

[JsonDerivedType(typeof(Player), "player")]
[JsonDerivedType(typeof(Enemy), "enemy")]
class Character : Entity
{
    public enum Stat { Strength, Agility, MaxHealth }

    public Dictionary<Stat, int> Stats { get; set; } = new();
    public int CurrentHealth { get; set; }

    public Character() : base() { }

    public Character(int x, int y, uint _icon, int maxHealth, int strength, int agility, Color color = Color.None)
        : base(x, y, _icon, color)
    {
        Stats = new Dictionary<Stat, int>
        {
            [Stat.Strength] = strength,
            [Stat.Agility] = agility,
            [Stat.MaxHealth] = maxHealth
        };
        CurrentHealth = maxHealth;
    }

    public int MaxHealth => GetStat(Stat.MaxHealth);
    public int Strength => GetStat(Stat.Strength);
    public int Agility => GetStat(Stat.Agility);

    public int GetStat(Stat stat)
    {
        return Stats.TryGetValue(stat, out var value) ? value : 0;
    }

    public void SetStat(Stat stat, int newValue)
    {
        int old = Stats[stat];
        Stats[stat] = newValue;
        if (stat == Stat.MaxHealth)
        {
            Stats[stat] = Math.Max(1, Stats[stat]); // no death by changing max_health
            if (CurrentHealth == old) CurrentHealth = Stats[stat];
            else CurrentHealth = Math.Clamp(CurrentHealth, 0, Stats[stat]);
        }
    }

    public void ModifyStat(Stat stat, int delta)
    {
        SetStat(stat, GetStat(stat) + delta);
    }

    public virtual void Attack(Character other)
    {
        string my_name = this is Player ? "You" : GetType().Name;
        string other_name = other is Player ? "you" : other.GetType().Name;

        // Simple hit chance calculation
        int roll = Random.Shared.Next(20);
        if (roll + Agility >= 10 + other.Agility)
        {
            other.TakeDamage(GetDamage());
            GameLog.Add($"{my_name} hit{(this is Player ? "" : "s")} {other_name}. ");
        }
        else
        {
            GameLog.Add($"{my_name} miss{(this is Player ? "" : "es")} {other_name}. ");
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
    }

    public bool IsDead()
    {
        return CurrentHealth <= 0;
    }

    public virtual int GetDamage()
    {
        return 5 * Strength;
    }
}