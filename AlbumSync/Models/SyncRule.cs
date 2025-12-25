namespace ImmichAlbumSync.Models;

public record SyncRule(
    string PersonId,
    string AlbumId,
    string Name
);
