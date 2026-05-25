using Mindmagma.Curses;
using domain;

class UIRenderer
{
    // static nint TopWindow;
    // static nint BottomWindow;

    // public static void InitUI()
    // {
    //     TopWindow = NCurses.NewWindow(1, Level.WIDTH, 0, 0);
    //     BottomWindow = NCurses.NewWindow(1, Level.WIDTH, Level.HEIGHT, 0);
    // }

    // public static void DeleteUI()
    // {
    //     NCurses.DeleteWindow(TopWindow);
    //     NCurses.DeleteWindow(BottomWindow);
    // }

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
        // NCurses.ClearWindow(TopWindow);
        // NCurses.MoveWindowAddString(TopWindow, 0, 0, game.GameLog);
        // NCurses.WindowRefresh(TopWindow);
  
        // Player p = game.GetPlayer();
        // string s = $"Level: 1, HP: {p.CurrentHealth}/{p.MaxHealth}, STR: {p.Strength}, AG: {p.Agility}";

        // //NCurses.ClearWindow(BottomWindow);
        // NCurses.MoveWindowAddString(BottomWindow, 0, 0, s);
        // NCurses.WindowRefresh(BottomWindow);

        string log = game.GameLog ?? "";
        if (log.Length > Level.WIDTH) log = log[..Level.WIDTH]; 
        NCurses.MoveAddString(0, 0, log.PadRight(Level.WIDTH));

        Player p = game.GetPlayer();
        string weapon = p.GetWeapon() == null ? "None" : p.GetWeapon()!.name;
        string stats = $"Level: 1, HP: {p.CurrentHealth}/{p.MaxHealth}, STR: {p.Strength}, AG: {p.Agility}, Weapon: {weapon}";
        if (stats.Length > Level.WIDTH) stats = stats[..Level.WIDTH];
        
        NCurses.MoveAddString(Level.HEIGHT + LevelRenderer.Y_OFFSET, 0, stats.PadRight(Level.WIDTH));
        //NCurses.MoveAddString(Level.HEIGHT, 0, $"   {NCurses.Lines - 1}"); //stats.PadRight(Level.WIDTH));

        if (game.backpackOpened) PrintBackpack(game.GetPlayer().GetBackpack(), game.backpackAction == GameSession.BackpackAction.WEAPON);
    }
}