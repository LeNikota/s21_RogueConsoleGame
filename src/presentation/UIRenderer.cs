using Mindmagma.Curses;
using domain;
using presentation;

class UIRenderer
{
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

    public static void PrintUI(Window window)
    {
        switch (window)
        {
            case GameWindow gw:
                if (gw.GameSession != null)
                {
                    switch (gw.GameSession.state)
                    {
                        case GameSession.State.PLAYING:
                            PrintGameUI(gw.GameSession);
                            break;
                        case GameSession.State.OVER:
                            PrintGameOver();
                            break;
                        case GameSession.State.STATS:
                            PrintStats(gw.GameSession);
                            break;
                    }
                }
                break;

            case MenuWindow mw:
                PrintMenu(mw);
                break;

            case StatisticsWindow sw:
                sw.Render();
                break;

            default:
                break;
        }
    }

    private static void PrintMenu(MenuWindow menu)
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
                NCurses.AttributeSet(NCurses.ColorPair(5)); // Highlight color (Blue)
                NCurses.MoveAddString(y, x - 2, "> " + label + " <");
                NCurses.AttributeSet(NCurses.ColorPair(0)); // Reset
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

        // 1. Print Log Line (Top)
        string log = GameLog.GetLog();
        if (log.Length > Level.WIDTH)
            log = log[..Level.WIDTH];
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

        if (game.Stats != null)
        {
            NCurses.MoveAddString(y++, x, $"Treasures Collected: {game.Stats.TreasuresCollected}");
            NCurses.MoveAddString(y++, x, $"Highest Level:       {game.Stats.HighestLevelReached}");
            NCurses.MoveAddString(y++, x, $"Enemies Defeated:    {game.Stats.EnemiesDefeated}");
            NCurses.MoveAddString(y++, x, $"Food Eaten:          {game.Stats.FoodEaten}");
            NCurses.MoveAddString(y++, x, $"Elixirs Drunk:       {game.Stats.ElixirsDrunk}");
            NCurses.MoveAddString(y++, x, $"Scrolls Read:        {game.Stats.ScrollsRead}");
            NCurses.MoveAddString(y++, x, $"Hits Dealt:          {game.Stats.HitsDealt}");
            NCurses.MoveAddString(y++, x, $"Hits Taken:          {game.Stats.HitsTaken}");
            NCurses.MoveAddString(y++, x, $"Tiles Traversed:     {game.Stats.TilesTraversed}");
        }

        y += 2;
        NCurses.MoveAddString(y++, x, "=== HIGH SCORES (Top Treasures) ===");

        // if (game.Scoreboard != null)
        // {
        //     for (int i = 0; i < Math.Min(game.Scoreboard.Count, 10); i++)
        //     {
        //         var score = game.Scoreboard[i];
        //         NCurses.MoveAddString(y++, x, $"{i + 1}. Lvl: {score.HighestLevelReached}, Gold: {score.TreasuresCollected}");
        //     }
        // }

        NCurses.MoveAddString(NCurses.Lines - 2, 2, "Press any key to return...");
    }
}