# RealEstate.Mcp

An MCP (Model Context Protocol) server that exposes RTHomePropertyManagement's
property data as tools an LLM can call. It's a standalone ASP.NET Core service
that talks to the existing API over HTTP - it holds no database connection and
no business logic of its own, so RTHomePropertyManagement stays the single
source of truth (and the single place enforcing auth/validation).

## Tools exposed

- `search_properties` - filter listings by city, for-rent/for-sale, min
  bedrooms, max price.
- `get_property_details` - full details for one listing by ID.
- `list_locations` - known cities/locations to search by.

Filtering for `search_properties` currently happens inside this MCP server
(it fetches `GET /api/properties` and filters in memory), because the API
doesn't yet expose query parameters for it. That's fine for a small catalog;
if the listing count grows, the better fix is adding filter/pagination
support to `GET /api/properties` itself and having this tool pass the
filters straight through.

## Prerequisites

- RTHomePropertyManagement API running locally (defaults to
  `http://localhost:5161` - see `RTHomeApi:BaseUrl` in appsettings.json).
- .NET 9 SDK.

## Running it

```
dotnet restore
dotnet run
```

By default this listens on `http://localhost:5280`, with the MCP endpoint at
`http://localhost:5280/mcp`.

## Connecting a client

**Claude Desktop / Claude Code (HTTP transport)** - add to your MCP client
config:

```json
{
  "mcpServers": {
    "real-estate": {
      "type": "http",
      "url": "http://localhost:5280/mcp"
    }
  }
}
```

**Your own app's LLM integration** - point whatever MCP client library your
chat backend uses at the same URL. The LLM will discover `search_properties`,
`get_property_details`, and `list_locations` automatically and can call them
as part of answering a user's question.

## Next steps worth considering

- Add auth: right now this calls RTHomePropertyManagement's endpoints
  unauthenticated (matching how they're currently exposed). If you add
  `RequireAuthorization()` to the API's property endpoints later, this
  client will need to attach a token too.
- Add write tools (create/update a listing, log a lead) once you've decided
  which actions are safe to let an LLM trigger, and behind what confirmation
  step.
- Add pagination/server-side filtering to `GET /api/properties` if the
  catalog grows past a few hundred listings.
