using System.Net.Http.Headers;

namespace RealEstate.Mcp.Auth;

/// <summary>
/// Attaches a valid Auth0 M2M access token to every outgoing request made
/// through the HttpClient it's registered on. Plugged into PropertyApiClient's
/// pipeline in Program.cs.
/// </summary>
public class Auth0AuthHandler : DelegatingHandler
{
    private readonly Auth0TokenService _tokenService;

    public Auth0AuthHandler(Auth0TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
