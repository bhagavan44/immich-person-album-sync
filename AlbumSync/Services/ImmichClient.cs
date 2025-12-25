using System.Text;
using System.Text.Json;
using System.Linq;
using ImmichAlbumSync.Auth;
using ImmichAlbumSync.Models;

namespace ImmichAlbumSync.Services;

public class ImmichClient(HttpClient http)
{
    public static ImmichClient Create(ImmichSettings s)
    {
        var handler = new ImmichAuthHandler(s.ApiKey)
        {
            InnerHandler = new HttpClientHandler()
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(s.BaseUrl),
            Timeout = TimeSpan.FromSeconds(s.RequestTimeoutSeconds)
        };

        return new ImmichClient(client);
    }

    public async Task<List<PersonAsset>> GetPersonAssets(
    string personId,
    double minConfidence)
    {
        var json = await http.GetStringAsync($"/api/people/{personId}/assets");
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .EnumerateArray()
            .Select(x => new PersonAsset(
                AssetId: x.GetProperty("id").GetString()!,
                Confidence: x.GetProperty("confidence").GetDouble()
            ))
            .Where(x => x.Confidence >= minConfidence)
            .ToList();
    }


    public async Task<HashSet<string>> GetAlbumAssets(string albumId)
    {
        var json = await http.GetStringAsync($"/api/albums/{albumId}");
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("assets")
            .EnumerateArray()
            .Select(x => x.GetProperty("id").GetString()!)
            .ToHashSet();
    }

    public async Task AddAssets(string albumId, IEnumerable<string> ids)
    {
        var payload = JsonSerializer.Serialize(new { ids });
        var res = await http.PutAsync(
            $"/api/albums/{albumId}/assets",
            new StringContent(payload, Encoding.UTF8, "application/json")
        );
        res.EnsureSuccessStatusCode();
    }

    // Returns a sequence of (Id, Name) tuples for people
    public async Task<IEnumerable<(string Id, string Name)>> GetPeople()
    {
        var results = new List<(string Id, string Name)>();
        var page = 0;
        var hasNextPage = true;

        while (hasNextPage)
        {
            var url = page == 0 ? "/api/people" : $"/api/people?page={page}";
            var json = await http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            // API returns { "people": [...], "hasNextPage": bool, "total": int, "hidden": int }
            var peopleArray = doc.RootElement.GetProperty("people");

            var people = peopleArray.EnumerateArray()
                .Select(el =>
                {
                    var id = el.GetProperty("id").GetString()!;
                    string name;
                    if (el.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                    {
                        name = nameProp.GetString()!;
                    }
                    else
                    {
                        var first = el.TryGetProperty("firstName", out var f) && f.ValueKind == JsonValueKind.String ? f.GetString() : null;
                        var last = el.TryGetProperty("lastName", out var l) && l.ValueKind == JsonValueKind.String ? l.GetString() : null;
                        name = string.Join(' ', new[] { first, last }.Where(x => !string.IsNullOrWhiteSpace(x)));
                    }

                    return (Id: id, Name: name ?? string.Empty);
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .ToList();

            results.AddRange(people);

            // Check if there's a next page
            if (doc.RootElement.TryGetProperty("hasNextPage", out var nextPageProp) && nextPageProp.GetBoolean())
            {
                page++;
            }
            else
            {
                hasNextPage = false;
            }
        }

        return results;
    }

    // Returns a sequence of (Id, Name) tuples for albums
    public async Task<IEnumerable<(string Id, string Name)>> GetAlbums()
    {
        var json = await http.GetStringAsync("/api/albums");
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray()
            .Select(el =>
            {
                var id = el.GetProperty("id").GetString()!;
                string name;
                if (el.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                {
                    name = nameProp.GetString()!;
                }
                else if (el.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                {
                    name = titleProp.GetString()!;
                }
                else
                {
                    name = string.Empty;
                }

                return (Id: id, Name: name);
            })
            .ToList();
    }
}
