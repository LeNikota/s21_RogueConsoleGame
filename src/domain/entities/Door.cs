using domain;

class Door : Entity
{
    public bool IsExit { get; set; }
    private Key? key;
    public Key? Key
    {
        get => key;
        set
        {
            key = value;
            Color = value?.Color ?? Color.None;
        }
    }

    public Door() : base() { }
    public Door(int x, int y, bool exit = false, Key? key = null) : base(x, y, '#', key?.Color ?? Color.None)
    {
        IsExit = exit;
        Key = key;
    }

    public bool Open(Player player, Level level)
    {
        if (IsExit) return true;

        bool can_open = player.GetBackpack().Items.Any(item => item is Key k && k.Color == Key?.Color);
        if (can_open)
        {
            GameLog.Add($"You open the {Key?.Color} door.");
            level.Entities.Remove(this);
        }
        else
        {
            GameLog.Add($"The door is locked. You need a {Key?.Color} key.");
        }
        return can_open;
    }
}