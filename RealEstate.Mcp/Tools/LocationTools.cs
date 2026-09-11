using System.ComponentModel;
using ModelContextProtocol.Server;
using RealEstate.Mcp.Clients;

namespace RealEstate.Mcp.Tools;

[McpServerToolType]
public static class LocationTools
{
    [McpServerTool(Name = "list_locations", ReadOnly = true, Idempotent = true)]
    [Description("List all known locations/cities that properties can be searched by.")]
    public static async Task<IEnumerable<object>> ListLocations(
        PropertyApiClient api,
        CancellationToken cancellationToken = default)
    {
        var locations = await api.GetAllLocationsAsync(cancellationToken);
        return locations.Select(l => new { l.Id, l.DisplayName, l.City, l.Country });
    }
}
