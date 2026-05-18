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
        string stats = $"Level: 1, HP: {p.CurrentHealth}/{p.MaxHealth}, STR: {p.Strength}, AG: {p.Agility}";
        if (stats.Length > Level.WIDTH) stats = stats[..Level.WIDTH];
        NCurses.MoveAddString(Level.HEIGHT, 0, stats.PadRight(Level.WIDTH));
    }
}