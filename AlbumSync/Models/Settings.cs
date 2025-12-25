namespace ImmichAlbumSync.Models;

public class Settings
{
    public ImmichSettings Immich { get; set; } = new();
    public List<SyncRule> SyncRules { get; set; } = [];
    public SyncOptions Options { get; set; } = new();
}

public class ImmichSettings
{
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public int RequestTimeoutSeconds { get; set; } = 60;
}

public class SyncOptions
{
    public bool DryRun { get; set; }
    public int BatchSize { get; set; } = 500;
}
