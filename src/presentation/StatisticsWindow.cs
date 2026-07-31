using Mindmagma.Curses;
using domain;
using data;

namespace presentation;

class StatisticsWindow(GameStatistics stats, Scoreboard scoreboard) : Window("Statistics")
{
    public GameStatistics Stats { get; set; } = stats;
    public Scoreboard Scoreboard { get; set; } = scoreboard;

    public override void Render()
    {
        NCurses.Clear();

        int y = 2;
        int x = 5;

        // Current Run Statistics
        NCurses.MoveAddString(y++, x, "=== CURRENT RUN STATISTICS ===");
        y++;


        NCurses.MoveAddString(y++, x, $"Id: {Stats.Id}");
        NCurses.MoveAddString(y++, x, $"Treasures Collected: {Stats.TreasuresCollected}");
        NCurses.MoveAddString(y++, x, $"Highest Level:       {Stats.HighestLevelReached}");
        NCurses.MoveAddString(y++, x, $"Enemies Defeated:    {Stats.EnemiesDefeated}");
        NCurses.MoveAddString(y++, x, $"Food Eaten:          {Stats.FoodEaten}");
        NCurses.MoveAddString(y++, x, $"Elixirs Drunk:       {Stats.ElixirsDrunk}");
        NCurses.MoveAddString(y++, x, $"Scrolls Read:        {Stats.ScrollsRead}");
        NCurses.MoveAddString(y++, x, $"Hits Dealt:          {Stats.HitsDealt}");
        NCurses.MoveAddString(y++, x, $"Hits Taken:          {Stats.HitsTaken}");
        NCurses.MoveAddString(y++, x, $"Tiles Traversed:     {Stats.TilesTraversed}");


        y += 2;

        // High Scores / Scoreboard
        NCurses.MoveAddString(y++, x, "=== HIGH SCORES (Top Treasures) ===");

        if (Scoreboard.Count > 0)
        {
            int i = 1;
            foreach (var score in Scoreboard.Scores)
            {
                string statsLine = $"{i++}. Id: {score.Id} " +
                                   $"Lvl: {score.HighestLevelReached,-3} " +
                                   $"Gold: {score.TreasuresCollected,-6} " +
                                   $"Enemies: {score.EnemiesDefeated,-3} " +
                                   $"F/E/S: {score.FoodEaten}/{score.ElixirsDrunk}/{score.ScrollsRead}";

                NCurses.MoveAddString(y++, x, statsLine);
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