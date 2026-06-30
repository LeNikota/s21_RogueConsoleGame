using System.Text.Json;
using System.Text.Json.Serialization;
using domain;

namespace data;

class JsonDataService(string filePath = "game_save.json") : IDataService
{
    private readonly string filePath = filePath;

    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
    };

    public async Task<(GameStatistics, Level)?> LoadAsync()
    {
        if (!File.Exists(filePath)) return null;
        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            var saveData = JsonSerializer.Deserialize<SaveData>(json, options);

            if (saveData == null || saveData.Player == null || saveData.WallsFlat == null || saveData.VisibilityFlat == null)
                return null;

            bool[,] walls = ConvertFlatTo2DBool(saveData.WallsFlat, Level.HEIGHT, Level.WIDTH);
            byte[,] visibilityBytes = ConvertFlatTo2DByte(saveData.VisibilityFlat, Level.HEIGHT, Level.WIDTH);
            VisibilityState[,] visibility = new VisibilityState[Level.HEIGHT, Level.WIDTH];
            for (int y = 0; y < Level.HEIGHT; y++)
                for (int x = 0; x < Level.WIDTH; x++)
                    visibility[y, x] = (VisibilityState)visibilityBytes[y, x];


            var level = new Level(saveData.Floor, saveData.Player, walls, visibility, saveData.Entities ?? []);

            return (saveData.Statistics, level);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading save: {ex.Message}");
            return null;
        }
    }

    public async Task SaveAsync(GameSession gameSession)
    {
        if (gameSession.Level == null || gameSession.Player == null) return;

        var saveData = new SaveData
        {
            Statistics = gameSession.Stats,
            Floor = gameSession.Level.Floor,
            Player = gameSession.Player,
            Entities = gameSession.Level.Entities.Where(e => e is not Player).ToList(),

            // Convert 2D arrays to flat lists for JSON compatibility
            WallsFlat = Convert2DToFlat(gameSession.Level.IsWall),
            VisibilityFlat = Convert2DToFlat(ConvertVisibilityToByte(gameSession.Level.Visibility))
        };

        try
        {
            string json = JsonSerializer.Serialize(saveData, options);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game: {ex.Message}");
        }
    }

    // Helper methods
    private List<bool> Convert2DToFlat(bool[,] array)
    {
        List<bool> list = new(array.Length);
        for (int i = 0; i < array.GetLength(0); i++)
            for (int j = 0; j < array.GetLength(1); j++)
                list.Add(array[i, j]);
        return list;
    }

    private List<byte> Convert2DToFlat(byte[,] array)
    {
        List<byte> list = new(array.Length);
        for (int i = 0; i < array.GetLength(0); i++)
            for (int j = 0; j < array.GetLength(1); j++)
                list.Add(array[i, j]);
        return list;
    }

    private bool[,] ConvertFlatTo2DBool(List<bool> list, int height, int width)
    {
        bool[,] array = new bool[height, width];
        int index = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                array[y, x] = list[index++];
        return array;
    }

    private byte[,] ConvertFlatTo2DByte(List<byte> list, int height, int width)
    {
        byte[,] array = new byte[height, width];
        int index = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                array[y, x] = list[index++];
        return array;
    }

    private byte[,] ConvertVisibilityToByte(VisibilityState[,] visibility)
    {
        byte[,] result = new byte[visibility.GetLength(0), visibility.GetLength(1)];
        for (int y = 0; y < visibility.GetLength(0); y++)
            for (int x = 0; x < visibility.GetLength(1); x++)
                result[y, x] = (byte)visibility[y, x];
        return result;
    }
}