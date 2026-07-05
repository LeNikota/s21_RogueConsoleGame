using Mindmagma.Curses;
using domain;

namespace presentation;

class StatisticsWindow : Window
{
    public GameSession? GameSession { get; set; } = new();
    public List<GameStatistics> Scoreboard { get; set; } = [];


    StatisticsWindow(string title = "Statistics", Dictionary<InputAction, Action>? actions = null) : base(title, actions) { }

    StatisticsWindow(List<GameStatistics> scoreboard) : base("Statistics")
    {
        Scoreboard = scoreboard;
    }

    public void Render()
    {
        NCurses.Clear();

        int y = 2;
        int x = 5;

        // --- Current Run Statistics ---
        NCurses.AttributeSet(NCurses.ColorPair(1)); // White
        NCurses.MoveAddString(y++, x, "=== CURRENT RUN STATISTICS ===");
        y++;
        NCurses.AttributeSet(NCurses.ColorPair(0)); // Reset

        if (GameSession != null)
        {
            NCurses.MoveAddString(y++, x, $"Treasures Collected: {GameSession.Stats.TreasuresCollected}");
            NCurses.MoveAddString(y++, x, $"Highest Level:       {GameSession.Stats.HighestLevelReached}");
            NCurses.MoveAddString(y++, x, $"Enemies Defeated:    {GameSession.Stats.EnemiesDefeated}");
            NCurses.MoveAddString(y++, x, $"Food Eaten:          {GameSession.Stats.FoodEaten}");
            NCurses.MoveAddString(y++, x, $"Elixirs Drunk:       {GameSession.Stats.ElixirsDrunk}");
            NCurses.MoveAddString(y++, x, $"Scrolls Read:        {GameSession.Stats.ScrollsRead}");
            NCurses.MoveAddString(y++, x, $"Hits Dealt:          {GameSession.Stats.HitsDealt}");
            NCurses.MoveAddString(y++, x, $"Hits Taken:          {GameSession.Stats.HitsTaken}");
            NCurses.MoveAddString(y++, x, $"Tiles Traversed:     {GameSession.Stats.TilesTraversed}");
        }

        y += 2;

        // --- High Scores / Scoreboard ---
        NCurses.AttributeSet(NCurses.ColorPair(1));
        NCurses.MoveAddString(y++, x, "=== HIGH SCORES (Top Treasures) ===");
        NCurses.AttributeSet(NCurses.ColorPair(0));

        if (Scoreboard != null && Scoreboard.Count > 0)
        {
            // Sort by treasures collected descending
            var sortedScores = Scoreboard.OrderByDescending(s => s.TreasuresCollected).Take(10).ToList();

            for (int i = 0; i < sortedScores.Count; i++)
            {
                var score = sortedScores[i];
                NCurses.MoveAddString(y++, x, $"{i + 1}. Lvl: {score.HighestLevelReached,-3} Gold: {score.TreasuresCollected,-6} Enemies: {score.EnemiesDefeated}");
            }
        }
        else
        {
            NCurses.MoveAddString(y++, x, "No records yet.");
        }

        // Footer
        NCurses.MoveAddString(NCurses.Lines - 2, 2, "Press ESC or ENTER to return...");
    }
}