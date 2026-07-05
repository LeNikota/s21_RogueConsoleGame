using domain;
namespace data;

class SaveData
{
    public GameStatistics Statistics { get; set; } = new();
    public int Floor { get; set; }
    public Player? Player { get; set; }
    public List<Entity>? Entities { get; set; }
    
    // Changed from bool[,] to List<bool>
    public List<bool>? WallsFlat { get; set; }
    // Changed from byte[,] to List<byte>
    public List<byte>? VisibilityFlat { get; set; }

    public List<GameStatistics> Scoreboard { get; set; } = [];

}