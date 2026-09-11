using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RealEstate.Mcp.Contracts;

namespace RealEstate.Mcp.Clients;

// Thin typed client over the RTHomePropertyManagement REST API.
// The MCP server never touches the database directly - it only calls
// endpoints the API already exposes, so auth/validation stay centralized
// there and this service is "just another client" of your API.
public class PropertyApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public PropertyApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PropertyDto>> GetAllPropertiesAsync(CancellationToken ct = default)
    {
        var properties = await _http.GetFromJsonAsync<List<PropertyDto>>("/api/properties", JsonOptions, ct);
        return properties ?? [];
    }

    public async Task<PropertyDto?> GetPropertyByIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/api/properties/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PropertyDto>(JsonOptions, ct);
    }

    public async Task<List<LocationDto>> GetAllLocationsAsync(CancellationToken ct = default)
    {
        var locations = await _http.GetFromJsonAsync<List<LocationDto>>("/api/getlocations", JsonOptions, ct);
        return locations ?? [];
    }
}
