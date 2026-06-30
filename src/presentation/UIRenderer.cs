using Mindmagma.Curses;
using domain;

class UIRenderer
{

    private static void PrintBackpack(Backpack backpack)
    {
        int x = (int)Math.Round(Level.WIDTH * 0.6);
        int y = 1;
        if (backpack.action == Backpack.Action.WEAPON) NCurses.MoveAddString(y++, x, "0) Unequip current weapon");

        for (int i = 0; i < backpack.Count; i++)
        {
            NCurses.MoveAddString(y++, x, $"{i + 1}) {backpack[i].Name}");
        }
        if (backpack.Treasure > 0) NCurses.MoveAddString(y, x, $"{backpack.Count + 1}) Treasure worth {backpack.Treasure}");

    }

    public static void PrintUI(GameSession game)
    {
        switch (game.state)
        {
            case GameSession.State.MENU:
            case GameSession.State.PAUSED:
                PrintMenu(game.CurrentMenu);
                break;
            case GameSession.State.STATS:
                PrintStats(game);
                break;
            case GameSession.State.PLAYING:
                PrintGameUI(game);
                break;
            case GameSession.State.OVER:
                PrintGameOver();
                break;
        }
    }

    private static void PrintMenu(Menu? menu)
    {
        if (menu == null) return;

        NCurses.Clear();
        int centerX = Level.WIDTH / 2;
        int centerY = Level.HEIGHT / 2;

        // Draw Title
        if (!string.IsNullOrEmpty(menu.Title))
        {
            NCurses.MoveAddString(centerY - 4, centerX - menu.Title.Length / 2, menu.Title);
        }

        // Draw Options
        for (int i = 0; i < menu.Count; i++)
        {
            int y = centerY + i * 2;
            string label = menu.GetLabel(i);
            int x = centerX - label.Length / 2;

            if (i == menu.SelectionIndex)
            {
                NCurses.AttributeSet(NCurses.ColorPair(5)); // Highlight color
                NCurses.MoveAddString(y, x - 2, "> " + label + " <");
                NCurses.AttributeSet(NCurses.ColorPair(0));
            }
            else
            {
                NCurses.MoveAddString(y, x, label);
            }
        }

        NCurses.MoveAddString(centerY + 10, centerX - 19, "Use UP/DOWN to select, ENTER to confirm");
    }


    private static void PrintGameUI(GameSession game)
    {
        if (game.Level == null) return;

        string log = GameLog.GetLog();
        if (log.Length > Level.WIDTH) log = log[..Level.WIDTH];
        NCurses.MoveAddString(0, 0, log.PadRight(Level.WIDTH));

        Player p = game.Player;
        string weapon = p.GetWeapon() == null ? "None" : p.GetWeapon()!.Name;
        string stats = $"Lvl: {game.Level.Floor}, HP: {p.CurrentHealth}/{p.MaxHealth}, STR: {p.Strength}, AG: {p.Agility}, Wpn: {weapon}";

        if (stats.Length > Level.WIDTH) stats = stats[..Level.WIDTH];
        NCurses.MoveAddString(Level.HEIGHT + LevelRenderer.Y_OFFSET, 0, stats.PadRight(Level.WIDTH));

        Backpack b = p.GetBackpack();
        if (b.Opened) PrintBackpack(b);
    }

    private static void PrintGameOver()
    {
        int centerX = Level.WIDTH / 2;
        int centerY = Level.HEIGHT / 2;

        NCurses.MoveAddString(centerY - 2, centerX - 10, "GAME OVER");
        NCurses.MoveAddString(centerY, centerX - 12, "Press [R] to Restart");
        NCurses.MoveAddString(centerY + 1, centerX - 12, "Press [ESC] to Exit");
    }

    public static void PrintStats(GameSession game)
    {
        NCurses.Clear();
        int y = 2;
        int x = 5;

        NCurses.MoveAddString(y++, x, "=== CURRENT RUN STATISTICS ===");
        y++;
        NCurses.MoveAddString(y++, x, $"Treasures Collected: {game.Stats.TreasuresCollected}");
        NCurses.MoveAddString(y++, x, $"Highest Level:       {game.Stats.HighestLevelReached}");
        NCurses.MoveAddString(y++, x, $"Enemies Defeated:    {game.Stats.EnemiesDefeated}");
        NCurses.MoveAddString(y++, x, $"Food Eaten:          {game.Stats.FoodEaten}");
        NCurses.MoveAddString(y++, x, $"Elixirs Drunk:       {game.Stats.ElixirsDrunk}");
        NCurses.MoveAddString(y++, x, $"Scrolls Read:        {game.Stats.ScrollsRead}");
        NCurses.MoveAddString(y++, x, $"Hits Dealt:          {game.Stats.HitsDealt}");
        NCurses.MoveAddString(y++, x, $"Hits Taken:          {game.Stats.HitsTaken}");
        NCurses.MoveAddString(y++, x, $"Tiles Traversed:     {game.Stats.TilesTraversed}");

        y += 2;
        NCurses.MoveAddString(y++, x, "=== HIGH SCORES (Top Treasures) ===");

        // var scores = game.GetHighScores();
        // for (int i = 0; i < Math.Min(scores.Count, 10); i++)
        // {
        //     NCurses.MoveAddString(y++, x, $"{i + 1}. Lvl: {scores[i].HighestLevelReached}, Gold: {scores[i].TreasuresCollected}");
        // }

        NCurses.MoveAddString(NCurses.Lines - 2, 2, "Press any key to return...");
        NCurses.Refresh();
    }
}