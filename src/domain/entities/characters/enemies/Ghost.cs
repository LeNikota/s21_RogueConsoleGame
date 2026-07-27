using domain.generation;

namespace domain;

class Ghost : Enemy
{
    public bool IsInvisible { get; set; } = false;
    private int _turnCounter = 0;

    public Ghost() : base() { }
    public Ghost(int x = 0, int y = 0) : base(x, y, 'g', StatFactor.LOW, StatFactor.LOW, StatFactor.HIGH, StatFactor.LOW, Color.White) { }

    private void Teleport(Level level)
    {
        IsInvisible = true;

        Room? room = level.GetRoomByPos(Position);

        if (room == null)
        {
            base.MovePeacefully(level);

            return;
        }

        Vector2i pos = level.GetRandPosInRoom(room);

        if (pos == Vector2i.ZERO)
        {
            base.MovePeacefully(level);
            return;
        }

        Position = pos;
    }

    public override void MovePeacefully(Level level)
    {
        _turnCounter++;

        if (_turnCounter % 3 == 0)
        {
            Teleport(level);

            _turnCounter = 0;
            return;
        }

        // Toggle invisibility randomly
        if (Random.Shared.NextDouble() < 0.5)
        {
            IsInvisible = !IsInvisible;
        }
    }

    public override void Attack(Character other)
    {
        // Ghost becomes visible when attacking
        IsInvisible = false;
        base.Attack(other);
    }
}
