namespace ImmichAlbumSync.Auth;

public class ImmichAuthHandler(string apiKey) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("x-api-key", apiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
