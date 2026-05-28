using Mindmagma.Curses;
using domain;

class UIRenderer
{

    private static void PrintBackpack(Backpack backpack, bool isWeaponSetting)
    {
        int x = (int)Math.Round(Level.WIDTH * 0.6);
        int y = 1;
        if (isWeaponSetting) NCurses.MoveAddString(y++, x, "0) Unequip current weapon");

        for (int i = 0; i < backpack.Count; i++)
        {
            NCurses.MoveAddString(y++, x, $"{i+1}) {backpack[i].name}");
        }

    }

    public static void PrintUI(GameSession game)
    {

        string log = GameLog.GetLog();
        if (log.Length > Level.WIDTH) log = log[..Level.WIDTH]; 
        NCurses.MoveAddString(0, 0, log.PadRight(Level.WIDTH));

        Player p = game.Player;
        string weapon = p.GetWeapon() == null ? "None" : p.GetWeapon()!.name;
        string stats = $"Level: 1, HP: {p.CurrentHealth}/{p.MaxHealth}, STR: {p.Strength}, AG: {p.Agility}, Weapon: {weapon}";
        if (stats.Length > Level.WIDTH) stats = stats[..Level.WIDTH];
        
        NCurses.MoveAddString(Level.HEIGHT + LevelRenderer.Y_OFFSET, 0, stats.PadRight(Level.WIDTH));
        //NCurses.MoveAddString(Level.HEIGHT, 0, $"   {NCurses.Lines - 1}"); //stats.PadRight(Level.WIDTH));

        if (game.backpackOpened) PrintBackpack(game.Player.GetBackpack(), game.backpackAction == GameSession.BackpackAction.WEAPON);
    }
}