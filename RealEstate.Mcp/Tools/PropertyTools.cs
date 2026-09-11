using System.ComponentModel;
using ModelContextProtocol.Server;
using RealEstate.Mcp.Clients;

namespace RealEstate.Mcp.Tools;

[McpServerToolType]
public static class PropertyTools
{
    [McpServerTool(Name = "search_properties", ReadOnly = true, Idempotent = true)]
    [Description(
        "Search property listings. All filters are optional - omit any filter you don't need. " +
        "Returns a compact summary per match; call get_property_details for the full listing. " +
        "Note: filtering currently happens in this MCP server, not the underlying API, so it " +
        "loads the full listing set on every call - fine for a small catalog, worth revisiting " +
        "(server-side filtering/pagination on the API) if the listing count grows a lot.")]
    public static async Task<IEnumerable<object>> SearchProperties(
        PropertyApiClient api,
        [Description("City to filter by, e.g. 'Manchester'. Case-insensitive, partial match. Omit for any city.")]
        string? city = null,
        [Description("true = only for-rent listings, false = only for-sale listings. Omit for either.")]
        bool? isForRent = null,
        [Description("Minimum number of bedrooms.")]
        int? minBedrooms = null,
        [Description("Maximum price.")]
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var properties = await api.GetAllPropertiesAsync(cancellationToken);

        var matches = properties.Where(p =>
            (city is null || (p.Location?.City?.Contains(city, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (isForRent is null || p.IsForRent == isForRent) &&
            (minBedrooms is null || (p.Bedrooms ?? 0) >= minBedrooms) &&
            (maxPrice is null || p.Price <= maxPrice));

        return matches.Select(p => new
        {
            p.Id,
            p.Title,
            p.Address,
            city = p.Location?.City,
            p.Price,
            p.PricePeriod,
            p.IsForRent,
            p.Bedrooms,
            p.Bathrooms,
            p.SquareFeet,
            p.Status
        });
    }

    [McpServerTool(Name = "get_property_details", ReadOnly = true, Idempotent = true)]
    [Description("Get the full details of a single property listing by its ID.")]
    public static async Task<object?> GetPropertyDetails(
        PropertyApiClient api,
        [Description("The property's ID.")] int id,
        CancellationToken cancellationToken = default)
    {
        var property = await api.GetPropertyByIdAsync(id, cancellationToken);
        if (property is null)
            return null;

        return new
        {
            property.Id,
            property.Title,
            property.Address,
            property.Description,
            city = property.Location?.City,
            country = property.Location?.Country,
            priceRange = property.PriceRange?.DisplayLabel,
            property.Price,
            property.PricePeriod,
            property.IsForRent,
            property.Bedrooms,
            property.Bathrooms,
            property.SquareFeet,
            property.Status,
            property.CreatedAt,
            property.ImageUrls
        };
    }
}
