namespace domain;

class GameStatistics(int id = 0)
{
    public int Id { get; set; } = id;
    public int TreasuresCollected { get; set; } = 0;
    public int HighestLevelReached { get; set; } = 1;
    public int EnemiesDefeated { get; set; } = 0;
    public int FoodEaten { get; set; } = 0;
    public int ElixirsDrunk { get; set; } = 0;
    public int ScrollsRead { get; set; } = 0;
    public int HitsDealt { get; set; } = 0;
    public int HitsTaken { get; set; } = 0;
    public int TilesTraversed { get; set; } = 0;

    public void Reset()
    {
        TreasuresCollected = 0;
        HighestLevelReached = 1;
        EnemiesDefeated = 0;
        FoodEaten = 0;
        ElixirsDrunk = 0;
        ScrollsRead = 0;
        HitsDealt = 0;
        HitsTaken = 0;
        TilesTraversed = 0;
    }

    public override string ToString()
    {
        return $"Stats:\n" +
               $"  Treasures: {TreasuresCollected}\n" +
               $"  Level: {HighestLevelReached}\n" +
               $"  Enemies Defeated: {EnemiesDefeated}\n" +
               $"  Food Eaten: {FoodEaten}\n" +
               $"  Elixirs Drunk: {ElixirsDrunk}\n" +
               $"  Scrolls Read: {ScrollsRead}\n" +
               $"  Hits Dealt: {HitsDealt}\n" +
               $"  Hits Taken: {HitsTaken}\n" +
               $"  Tiles Traversed: {TilesTraversed}";
    }
}