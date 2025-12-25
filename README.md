# Immich Person Album Sync

Automatically sync Immich face-recognition people into albums.

## Features

- Multi-person → multi-album
- Dry-run mode
- Docker & single-file exe
- Safe, idempotent sync

## Usage

1. Create album(s) in Immich
2. Copy personId + albumId into appsettings.json
3. Run the tool (dry-run first)

## Running the Project

### Prerequisites

- .NET 10 SDK (for development/building)
- Docker (for containerized deployment)
- Immich instance with API access

### Configuration

Create or edit `appsettings.json` in the `AlbumSync` folder:

```json
{
  "Immich": {
    "ApiUrl": "http://immich-server:2283",
    "ApiKey": "your-api-key-here"
  },
  "SyncRules": [
    {
      "PersonId": "person-uuid",
      "AlbumIds": ["album-uuid-1", "album-uuid-2"]
    }
  ]
}
```

### Option 1: Direct Execution (Windows/Linux)

**Build the project:**

```bash
dotnet build
```

**Run with default settings:**

```bash
dotnet run --project AlbumSync
```

**Test with dry-run (preview changes without applying):**

```bash
dotnet run --project AlbumSync -- --dry-run
```

**List available people and albums:**

```bash
dotnet run --project AlbumSync -- --list-all
dotnet run --project AlbumSync -- --list-people
dotnet run --project AlbumSync -- --list-albums
```

### Option 2: Docker Deployment

**Build the Docker image:**

```bash
docker build -f docker/Dockerfile -t immich-album-sync .
```

**Run the container:**

```bash
docker compose -f docker/docker-compose.yml up -d
```

**View logs:**

```bash
docker logs -f immich-album-sync
```

### Option 3: Self-Contained Executable

**Publish as a single-file executable:**

```bash
dotnet publish -c Release -o dist
```

Run the executable from the `dist` folder.
