using domain;
namespace data;

class SaveData
{
    public int Floor { get; set; }
    public Player? Player { get; set; }
    public List<Entity>? Entities { get; set; }
    public GameStatistics Statistics { get; set; } = new();

    public List<bool>? WallsFlat { get; set; }
    public List<byte>? VisibilityFlat { get; set; }
}