namespace RealEstate.Mcp.Contracts;

// Mirrors the shape of RTHomePropertyManagement's DTOs (PropertyDto,
// LocationDto, PriceRangeDto). Deserialized with PropertyNameCaseInsensitive,
// so these PascalCase properties bind fine against the API's camelCase JSON.
// Keep this in sync manually if the API's DTOs change shape.

public class PropertyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string? Description { get; set; }
    public LocationDto? Location { get; set; }
    public PriceRangeDto? PriceRange { get; set; }
    public bool IsForRent { get; set; }
    public decimal Price { get; set; }
    public string? PricePeriod { get; set; }
    public int? SquareFeet { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}

public class LocationDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class PriceRangeDto
{
    public int Id { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string DisplayLabel { get; set; } = "";
}
