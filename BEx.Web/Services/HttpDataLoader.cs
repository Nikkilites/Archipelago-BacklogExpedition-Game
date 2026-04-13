using System.Text.Json;
using BEx.Core.Model;
using BEx.Core;

public class HttpDataLoader : IDataLoader
{
    public T Load<T>(string path)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, path);
        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<T>(json)!;
    }

    public GameData LoadData()
    {
        return Load<GameData>("DataStorage/data.json");
    }

    public StoryData LoadStory()
    {
        return Load<StoryData>("DataStorage/story.json");
    }
}