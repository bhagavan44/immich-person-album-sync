using ImmichAlbumSync.Models;
using Microsoft.Extensions.Logging;

namespace ImmichAlbumSync.Services;

public class AlbumSyncService(
    ImmichClient client,
    ILogger logger,
    SyncOptions options)
{
    public async Task RunAsync(IEnumerable<SyncRule> rules)
    {
        foreach (var rule in rules)
        {
            logger.LogInformation("▶ {Rule}", rule.Name);

            var personAssets = await client.GetPersonAssets(rule.PersonId);
            var albumAssets = await client.GetAlbumAssets(rule.AlbumId);

            var missing = personAssets.Except(albumAssets).ToList();

            if (!missing.Any())
            {
                logger.LogInformation("✔ Album already up to date");
                continue;
            }

            if (options.DryRun)
            {
                logger.LogWarning("[DRY-RUN] Would add {Count} assets", missing.Count);
                continue;
            }

            foreach (var batch in missing.Chunk(options.BatchSize))
            {
                await client.AddAssets(rule.AlbumId, batch);
                logger.LogInformation("Added {Count} assets", batch.Length);
            }
        }
    }
}
