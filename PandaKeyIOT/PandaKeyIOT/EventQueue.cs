using System.Text.Json;

public sealed class EventQueue
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false
    };

    public EventQueue(string filePath) => _filePath = filePath;

    public async Task EnqueueAsync(QueuedEvent e, CancellationToken ct)
    {
        var line = JsonSerializer.Serialize(e, JsonOpts);
        await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, ct);
    }

    public async Task<List<QueuedEvent>> ReadAllAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath)) return new List<QueuedEvent>();

        var lines = await File.ReadAllLinesAsync(_filePath, ct);
        var list = new List<QueuedEvent>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var obj = JsonSerializer.Deserialize<QueuedEvent>(line, JsonOpts);
                if (obj != null) list.Add(obj);
            }
            catch { }
        }
        return list;
    }

    public void Clear()
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);
    }
}
