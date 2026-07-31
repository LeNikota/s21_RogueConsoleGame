namespace domain;


class Vampire : Enemy
{
    public bool WasAttacked { get; set; } = false;

    public Vampire() : base() { }
    public Vampire(int x = 0, int y = 0) : base(x, y, 'v', StatFactor.HIGH, StatFactor.MEDIUM, StatFactor.HIGH, StatFactor.HIGH, Color.Red) { }

    public override void Attack(Character other)
    {
        string my_name = "Vampire";
        string other_name = other is Player ? "you" : other.GetType().Name;

        int roll = Random.Shared.Next(20);
        if (roll + Agility < 10 + other.Agility)
        {
            GameLog.Add($"{my_name} misses {other_name}.");
            return;
        }

        int damage = GetDamage();
        other.TakeDamage(damage);

        // Life Steal: Steal Max Health
        if (other is Player player)
        {
            // Steal half damage as max HP
            int stealAmount = Math.Min(player.MaxHealth, damage / 2);
            if (stealAmount > 0)
            {
                player.StealMaxHealth(stealAmount);
                ModifyStat(Stat.MaxHealth, MaxHealth + stealAmount);
                CurrentHealth = Math.Min(MaxHealth, CurrentHealth + stealAmount);
                GameLog.Add($"{my_name} hits {other_name} and steals {stealAmount} max health!");
            }
            else
            {
                GameLog.Add($"{my_name} hits {other_name}.");
            }
        }
        else
        {
            GameLog.Add($"{my_name} hits {other_name}.");
        }
    }
    
    public override void MovePeacefully(Level level)
    {
        var directions = Vector2i.ALL_DIRECTIONS;

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
}