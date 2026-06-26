namespace domain;

public class SaveData
{
    // Current Game State
    public int Floor { get; set; }
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int Treasure { get; set; }
    public List<string> InventoryNames { get; set; } = new();

    // Statistics for current run
    public GameStatistics CurrentRunStats { get; set; } = new GameStatistics();

    // History of all runs (High Scores)
    public List<GameStatistics> HighScores { get; set; } = [];
}