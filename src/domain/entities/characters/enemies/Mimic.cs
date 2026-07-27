using domain.generation;

namespace domain;


class Mimic : Enemy
{
    public bool WasAttacked { get; set; } = false;

    public Mimic() : base() { }
    public Mimic(int x = 0, int y = 0) : base(x, y, 'm', StatFactor.HIGH, StatFactor.LOW, StatFactor.HIGH, StatFactor.LOW, Color.White) { }

    public override void MovePeacefully(Level level)
    {
        if (Icon != 'm')
            return;

        if (level.Entities.Find(e => e is Key) is Key key)
        {
            if (Random.Shared.NextDouble() > 0.5)
            {
                Icon = key.Icon;
                Color = key.Color;
                return;
            }
        }

        Icon = Item.GetRandomItem().Icon;
    }

    public override void Act(Player player, Level level, GameStatistics stats)
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

        Icon = 'm';
        Color = Color.White;

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