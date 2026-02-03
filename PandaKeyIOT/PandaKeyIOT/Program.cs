using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = config.GetSection("IoT").Get<IoTSettings>() ?? new IoTSettings();

using var http = new HttpClient
{
    BaseAddress = new Uri(settings.ServerBaseUrl),
    Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
};
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var queue = new EventQueue(settings.QueueFile);
var client = new IoTClient(http, settings, queue);

Console.WriteLine("PandaKey IoT Client started.");
Console.WriteLine($"Server: {settings.ServerBaseUrl}");
Console.WriteLine($"AccessPointId: {settings.AccessPointId}");
Console.WriteLine("Commands: scan <userId> | flush | config | reset | exit");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

while (!cts.IsCancellationRequested)
{
    Console.Write("> ");
    var line = Console.ReadLine();
    if (line is null) continue;

    var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0) continue;

    var cmd = parts[0].ToLowerInvariant();

    if (cmd == "exit") break;

    if (cmd == "config")
    {
        Console.WriteLine($"ServerBaseUrl: {settings.ServerBaseUrl}");
        Console.WriteLine($"AccessPointId: {settings.AccessPointId}");
        Console.WriteLine($"TimeoutSeconds: {settings.TimeoutSeconds}");
        Console.WriteLine($"RetryCount: {settings.RetryCount}");
        Console.WriteLine($"QueueFile: {settings.QueueFile}");
        continue;
    }

    if (cmd == "flush")
    {
        await client.TryFlushQueueAsync(cts.Token);
        continue;
    }

    if (cmd == "reset")
    {

        queue.Clear();
        Console.WriteLine("[RESET] queue cleared. (config reset can be done by restoring appsettings.json)");
        continue;
    }

    if (cmd == "scan")
    {
        if (parts.Length < 2 || !int.TryParse(parts[1], out var userId) || userId <= 0)
        {
            Console.WriteLine("Usage: scan <userId>");
            continue;
        }

        await client.HandleScanAsync(userId, cts.Token);
        continue;
    }

    Console.WriteLine("Unknown command.");
}
