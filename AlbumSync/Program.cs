using System.Linq;
using ImmichAlbumSync.Models;
using ImmichAlbumSync.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false)
    .AddJsonFile("appsettings.development.json", true)
    .AddEnvironmentVariables()
    .Build();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger("ImmichAlbumSync");

var settings = config.Get<Settings>()!;

// Command-line overrides
var dryRunArg = args.Contains("--dry-run");
if (dryRunArg)
{
    settings.Options.DryRun = true;
    Console.WriteLine("Running in dry-run mode (command line override)");
}

// Support command-line options to list people/albums
var listPeople = args.Contains("--list-people");
var listAlbums = args.Contains("--list-albums");
var listAll = args.Contains("--list-all");

if (listAll) { listPeople = true; listAlbums = true; }

// Create client once and reuse
var client = ImmichClient.Create(settings.Immich);

if (listPeople || listAlbums)
{
    if (listPeople)
    {
        var people = await client.GetPeople();
        Console.WriteLine("People (id\tname):");
        foreach (var p in people)
        {
            Console.WriteLine($"{p.Id}\t{p.Name}");
        }
    }

    if (listAlbums)
    {
        var albums = await client.GetAlbums();
        Console.WriteLine("Albums (id\tname):");
        foreach (var a in albums)
        {
            Console.WriteLine($"{a.Id}\t{a.Name}");
        }
    }

    return;
}

var syncService = new AlbumSyncService(client, logger, settings.Options);

await syncService.RunAsync(settings.SyncRules);
