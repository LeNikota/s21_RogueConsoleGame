// namespace domain;

// using domain.generation;

// class Ogre : Enemy
// {
//     public bool IsResting { get; set; } = false;
//     public bool AfterRest { get; set; } = false;

//     public Ogre() : base() { }
//     public Ogre(int x = 0, int y = 0) : base(x, y, 'O', StatFactor.VERY_HIGH, StatFactor.VERY_HIGH, StatFactor.LOW, StatFactor.MEDIUM, Color.Yellow) { }

//     public override void MovePeacefully(Level level)
//     {
//         var directions = Vector2i.SIMPLE_DIRECTIONS;

//         Tools.Shuffle(directions);

//         foreach (var dir in directions)
//         {
//             Vector2i newPos = Position + dir * 2;

//             if (level.IsPositionValid(newPos))
//             {
//                 if (level.GetEntityByPos(newPos) == null)
//                 {
//                     Position = newPos;
//                     return;
//                 }
//             }
//         }

//     }

//     public override void Attack(Character other)
//     {
//         if (AfterRest)
//         {
//             other.TakeDamage(GetDamage());
//             IsResting = true;
//             return;
//         }

//         base.Attack(other);
//         IsResting = true;
//         GameLog.Add("Ogre rests for a turn.");
//     }

//     public override void Act(Player player, Level level, GameStatistics stats)
//     {
//         if (IsResting)
//         {
//             IsResting = false;
//             AfterRest = true;
//             return;
//         }

//         base.Act(player, level, stats);
//     }
// }




namespace domain;

using domain.generation;

class Ogre : Enemy
{
    public bool IsResting { get; set; } = false;
    public bool NextAttackGuaranteed { get; set; } = false;

    public Ogre() : base() { }
    public Ogre(int x = 0, int y = 0) : base(x, y, 'O', StatFactor.VERY_HIGH, StatFactor.VERY_HIGH, StatFactor.LOW, StatFactor.MEDIUM, Color.Yellow) { }

    public override void MovePeacefully(Level level)
    {
        var directions = new List<Vector2i> { Vector2i.UP, Vector2i.DOWN, Vector2i.LEFT, Vector2i.RIGHT };
        Tools.Shuffle(directions);

        foreach (var dir in directions)
        {
            Vector2i step1 = Position + dir;
            Vector2i step2 = Position + dir * 2;

            if (level.IsPositionValid(step1) && level.GetEntityByPos(step1) == null &&
                level.IsPositionValid(step2) && level.GetEntityByPos(step2) == null)
            {
                Position = step2;
                return;
            }
        }
    }

    public override void Attack(Character other)
    {
        string my_name = "Ogre";
        string other_name = other is Player ? "you" : other.GetType().Name;

        if (NextAttackGuaranteed)
        {
            int damage = GetDamage();
            other.TakeDamage(damage);

            GameLog.Add($"{my_name} counterattacks {other_name} for {damage} damage! (Guaranteed Hit)");

            NextAttackGuaranteed = false;
            IsResting = true;

            return;
        }

        base.Attack(other);
    }

    public override void Act(Player player, Level level, GameStatistics stats)
    {
        if (IsResting)
        {
            IsResting = false;
            NextAttackGuaranteed = true;

            GameLog.Add("Ogre finishes resting and prepares to counter.");

            return;
        }

        base.Act(player, level, stats);
    }
}