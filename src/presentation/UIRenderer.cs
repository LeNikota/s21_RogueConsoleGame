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
            NCurses.MoveAddString(y++, x, $"{i + 1}) {backpack[i].name}");
        }
        if (backpack.treasure > 0) NCurses.MoveAddString(y, x, $"{backpack.Count + 1}) Treasure worth {backpack.treasure}");

    }

    public static void PrintUI(GameSession game)
    {
        switch (game.state)
        {
            case GameSession.State.MENU:
                PrintMainMenu(game);
                break;
            case GameSession.State.PLAYING:
                PrintGameUI(game);
                break;
            case GameSession.State.PAUSED:
                // Draws game underneath
                PrintGameUI(game);
                PrintPauseMenu(game);
                break;
            case GameSession.State.OVER:
                PrintGameOver();
                break;
        }
    }


    private static void PrintMainMenu(GameSession game)
    {
        NCurses.Clear();
        int centerX = Level.WIDTH / 2;
        int centerY = Level.HEIGHT / 2;

        string title = "==== S21 ROGUE GAME ====";
        NCurses.MoveAddString(centerY - 4, centerX - title.Length / 2, title);

        for (int i = 0; i < game.MenuOptions.Length; i++)
        {
            int y = centerY + i * 2;
            string option = game.MenuOptions[i];
            int x = centerX - option.Length / 2;

            if (i == game.MenuSelectionIndex)
            {
                NCurses.AttributeSet(NCurses.ColorPair(5));
                NCurses.MoveAddString(y, x - 2, "> " + option + " <");
                NCurses.AttributeSet(NCurses.ColorPair(0));
            }
            else
            {
                NCurses.MoveAddString(y, x, option);
            }
        }

        NCurses.MoveAddString(centerY + 10, centerX - 19, "Use UP/DOWN to select, ENTER to confirm");
    }

    private static void PrintGameUI(GameSession game)
    {
        if (game.GetLevel() == null) return;

        string log = GameLog.GetLog();
        if (log.Length > Level.WIDTH) log = log[..Level.WIDTH];
        NCurses.MoveAddString(0, 0, log.PadRight(Level.WIDTH));

        Player p = game.Player;
        string weapon = p.GetWeapon() == null ? "None" : p.GetWeapon()!.name;
        string stats = $"Lvl: {game.GetLevel().floor}, HP: {p.CurrentHealth}/{p.MaxHealth}, STR: {p.Strength}, AG: {p.Agility}, Wpn: {weapon}";

        if (stats.Length > Level.WIDTH) stats = stats[..Level.WIDTH];
        NCurses.MoveAddString(Level.HEIGHT + LevelRenderer.Y_OFFSET, 0, stats.PadRight(Level.WIDTH));

        Backpack b = p.GetBackpack();
        if (b.Opened) PrintBackpack(b);
    }

    private static void PrintPauseMenu(GameSession game)
    {
        int width = 30;
        int height = 8;
        int startX = (Level.WIDTH - width) / 2;
        int startY = (Level.HEIGHT - height) / 2;

        // Draw Box Background
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (y == 0 || y == height - 1)
                    NCurses.MoveAddChar(startY + y, startX + x, '-');
                else if (x == 0 || x == width - 1)
                    NCurses.MoveAddChar(startY + y, startX + x, '|');
                else
                    NCurses.MoveAddChar(startY + y, startX + x, ' ');
            }
        }

        NCurses.MoveAddString(startY + 2, startX + 12, "PAUSED");

        // Draw Options Dynamically
        for (int i = 0; i < game.PauseOptions.Length; i++)
        {
            string option = game.PauseOptions[i];
            int x = startX + (width - option.Length) / 2;
            int y = startY + 4 + i;

            if (i == game.PauseSelectionIndex)
            {
                NCurses.AttributeSet(NCurses.ColorPair(5)); // Highlight color
                NCurses.MoveAddString(y, x - 2, "> " + option + " <");
                NCurses.AttributeSet(NCurses.ColorPair(0)); // Reset
            }
            else
            {
                NCurses.MoveAddString(y, x, option);
            }
        }
    }

    private static void PrintGameOver()
    {
        int centerX = Level.WIDTH / 2;
        int centerY = Level.HEIGHT / 2;

        NCurses.MoveAddString(centerY - 2, centerX - 10, "GAME OVER");
        NCurses.MoveAddString(centerY, centerX - 12, "Press [R] to Restart");
        NCurses.MoveAddString(centerY + 1, centerX - 12, "Press [ESC] to Exit");
    }

    public static void PrintStats(GameStatistics stats)
    {
        NCurses.Clear();
        int y = 2;
        int x = 5;

        NCurses.MoveAddString(y++, x, "=== GAME STATISTICS ===");
        y++;
        NCurses.MoveAddString(y++, x, $"Treasures Collected: {stats.TreasuresCollected}");
        NCurses.MoveAddString(y++, x, $"Highest Level:       {stats.HighestLevelReached}");
        NCurses.MoveAddString(y++, x, $"Enemies Defeated:    {stats.EnemiesDefeated}");
        NCurses.MoveAddString(y++, x, $"Food Eaten:          {stats.FoodEaten}");
        NCurses.MoveAddString(y++, x, $"Elixirs Drunk:       {stats.ElixirsDrunk}");
        NCurses.MoveAddString(y++, x, $"Scrolls Read:        {stats.ScrollsRead}");
        NCurses.MoveAddString(y++, x, $"Hits Dealt:          {stats.HitsDealt}");
        NCurses.MoveAddString(y++, x, $"Hits Taken:          {stats.HitsTaken}");
        NCurses.MoveAddString(y++, x, $"Tiles Traversed:     {stats.TilesTraversed}");

        NCurses.MoveAddString(NCurses.Lines - 2, 2, "Press any key to return...");
        NCurses.Refresh();
    }
}