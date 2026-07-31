namespace domain;

class SnakeMage : Enemy
{
    // public Vector2i PreviousDirection = Vector2i.DIAGONAL_DIRECTIONS[Random.Shared.Next(Vector2i.DIAGONAL_DIRECTIONS.Length)];
    public SnakeMage() : base() { }
    public SnakeMage(int x = 0, int y = 0) : base(x, y, 's', StatFactor.LOW, StatFactor.LOW, StatFactor.VERY_HIGH, StatFactor.HIGH, Color.White) { }

    public override void MovePeacefully(Level level)
    {
        var directions = Vector2i.DIAGONAL_DIRECTIONS;

        Tools.Shuffle(directions);

        foreach (var dir in directions)
        {
            Vector2i newPos = Position + dir;

            if (level.IsPositionValid(newPos))
            {
                if (level.GetEntityByPos(newPos) == null)
                {
                    Position = newPos;
                    // PreviousDirection = dir;
                    return;
                }
            }
        }
    }

    public override void Attack(Character other)
    {
        string my_name = "Snake-Mage";
        string other_name = other is Player ? "you" : other.GetType().Name;

        int roll = Random.Shared.Next(20);
        if (roll + Agility >= 10 + other.Agility)
        {
            other.TakeDamage(GetDamage());

            // Sleep Effect
            if (other is Player player && Random.Shared.Next(0, 100) < 30) // 30% chance
            {
                player.IsAsleep = true;
                GameLog.Add($"{my_name} hits {other_name} and puts {other_name} to sleep!");
            }
            else
            {
                GameLog.Add($"{my_name} hits {other_name}.");
            }
        }
        else
        {
            GameLog.Add($"{my_name} misses {other_name}.");
        }
    }
}
