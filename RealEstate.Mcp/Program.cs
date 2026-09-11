using RealEstate.Mcp.Clients;

var builder = WebApplication.CreateBuilder(args);

// Base URL of the RTHomePropertyManagement API this MCP server calls into.
// Override via appsettings.{Environment}.json, an env var (RTHomeApi__BaseUrl),
// or user-secrets in development.
var apiBaseUrl = builder.Configuration["RTHomeApi:BaseUrl"] ?? "http://localhost:5161";

builder.Services.AddHttpClient<PropertyApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// MCP endpoint is mounted at /mcp so it doesn't collide with anything else
// you might add to this service later (health checks, etc).
app.MapMcp("/mcp");

app.Run();
