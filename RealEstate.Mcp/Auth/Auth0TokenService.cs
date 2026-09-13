using System.Text.Json.Serialization;

namespace RealEstate.Mcp.Auth;

/// <summary>
/// Fetches and caches an Auth0 machine-to-machine access token (client
/// credentials grant) so this MCP server can call the RTHomePropertyManagement
/// API as an authenticated client - the server never talks to the database
/// directly, and now it can't even reach the API without a valid token.
/// Thread-safe; refreshes shortly before the cached token expires.
/// </summary>
public class Auth0TokenService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public Auth0TokenService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _http = httpClientFactory.CreateClient("Auth0");
        _config = config;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        // 60s safety margin so we never hand out a token that expires mid-flight.
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-60))
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            // Re-check after acquiring the lock in case another request already refreshed it.
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-60))
                return _cachedToken;

            var domain = _config["Auth0:Domain"];
            var clientId = _config["Auth0:ClientId"];
            var clientSecret = _config["Auth0:ClientSecret"];
            var audience = _config["Auth0:Audience"];

            if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(clientId) ||
                string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException(
                    "Auth0:Domain, Auth0:ClientId, Auth0:ClientSecret and Auth0:Audience must all be " +
                    "configured for the MCP server to authenticate to the API. Set Auth0:ClientSecret via " +
                    "user-secrets locally or the Auth0__ClientSecret env var in deployment - never commit it.");
            }

            using var response = await _http.PostAsJsonAsync($"https://{domain}/oauth/token", new
            {
                client_id = clientId,
                client_secret = clientSecret,
                audience,
                grant_type = "client_credentials"
            }, ct);

            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct)
                ?? throw new InvalidOperationException("Auth0 token endpoint returned an empty response.");

            _cachedToken = payload.AccessToken;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(payload.ExpiresIn);
            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
