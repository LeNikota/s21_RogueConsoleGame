using Mindmagma.Curses;
using domain;

namespace presentation;

class GameWindow(string title = "Game", Dictionary<InputAction, Action>? actions = null) : Window(title, actions)
{
    public GameSession? GameSession { get; set; } = null;

    public override void Input(InputAction input)
    {
        if (_actions.TryGetValue(input, out Action? action))
        {
            action?.Invoke();
        }

        GameSession?.Input(input);
    }

    public override void Render()
    {
        if (GameSession == null) return;

        LevelRenderer.PrintLevel(GameSession.Level);

        switch (GameSession.state)
        {
            case GameSession.State.PLAYING:
                PrintGameUI(GameSession);
                break;
            
            case GameSession.State.OVER:
                PrintGameUI(GameSession);
                PrintGameOver();
                break;
                
            default:
                break;
        }
    }

    private static void PrintBackpack(Backpack backpack)
    {
        int x = (int)Math.Round(Level.WIDTH * 0.6);
        int y = 1;

        if (backpack.action == Backpack.Action.WEAPON)
            NCurses.MoveAddString(y++, x, "0) Unequip current weapon");

        for (int i = 0; i < backpack.Count; i++)
        {
            string itemName = backpack[i].Name;
            // Truncates if too long to prevent overflow
            if (itemName.Length > Level.WIDTH - x - 5)
                itemName = itemName[..(Level.WIDTH - x - 5)] + "...";

            NCurses.MoveAddString(y++, x, $"{i + 1}) {itemName}");
        }

        if (backpack.Treasure > 0)
            NCurses.MoveAddString(y, x, $"{backpack.Count + 1}) Treasure worth {backpack.Treasure}");
    }

    private static void PrintGameUI(GameSession game)
    {
        if (game.Level == null) return;

        // 1. Print Log Line (Top)
        string log = GameLog.GetLog();
        if (log.Length > Level.WIDTH)
            log = log[..Level.WIDTH];
        
        // Pad with spaces to clear previous longer logs
        NCurses.MoveAddString(0, 0, log.PadRight(Level.WIDTH));

        // 2. Print Stats Line (Bottom)
        Player p = game.Player;
        string weaponName = p.GetWeapon() == null ? "None" : p.GetWeapon()!.Name;
        string stats = $"Lvl: {game.Level.Floor}, HP: {p.CurrentHealth}/{p.MaxHealth}, STR: {p.Strength}, AG: {p.Agility}, Wpn: {weaponName}";

        if (stats.Length > Level.WIDTH)
            stats = stats[..Level.WIDTH];

        NCurses.MoveAddString(Level.HEIGHT + LevelRenderer.Y_OFFSET, 0, stats.PadRight(Level.WIDTH));

        // 3. Print Backpack if Open
        Backpack b = p.GetBackpack();
        if (b.Opened)
            PrintBackpack(b);
    }

    private static void PrintGameOver()
    {
        int centerX = Level.WIDTH / 2;
        int centerY = Level.HEIGHT / 2;

        NCurses.AttributeSet(NCurses.ColorPair(2)); // Red for Game Over
        NCurses.MoveAddString(centerY - 2, centerX - 10, "GAME OVER");
        NCurses.AttributeSet(NCurses.ColorPair(0)); // Reset
        
        NCurses.MoveAddString(centerY, centerX - 12, "Press [R] to Restart");
        NCurses.MoveAddString(centerY + 1, centerX - 12, "Press [ESC] to Exit");
    }
}