using System.Text.Json;
using domain;

namespace data;

class JsonDataService(string filePath = "game_save.json") : IDataService
{
    private readonly string _filePath = filePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public async Task<(GameStatistics, Level)?> LoadAsync()
    {
        if (!File.Exists(_filePath)) return null;

        try
        {
            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<(GameStatistics, Level)>(json, _options);
            // return JsonSerializer.Deserialize<SaveData>(json, _options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading save: {ex.Message}");
            return null;
        }
    }

    public async Task SaveAsync(GameSession gameSession)
    {
        try
        {
            string json = JsonSerializer.Serialize(gameSession.Player, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game: {ex.Message}");
        }
    }
}
