using RealEstate.Mcp.Auth;
using RealEstate.Mcp.Clients;

var builder = WebApplication.CreateBuilder(args);

// Base URL of the RTHomePropertyManagement API this MCP server calls into.
// Override via appsettings.{Environment}.json, an env var (RTHomeApi__BaseUrl),
// or user-secrets in development.
var apiBaseUrl = builder.Configuration["RTHomeApi:BaseUrl"] ?? "http://localhost:5161";
var auth0Domain = builder.Configuration["Auth0:Domain"];

// Named client used only to talk to Auth0's token endpoint.
builder.Services.AddHttpClient("Auth0", client =>
{
    if (!string.IsNullOrEmpty(auth0Domain))
        client.BaseAddress = new Uri($"https://{auth0Domain}");
});

builder.Services.AddSingleton<Auth0TokenService>();
builder.Services.AddTransient<Auth0AuthHandler>();

// The API now requires a valid Auth0 access token on every /api call, so this
// client fetches its own M2M token (client-credentials grant) via
// Auth0AuthHandler and attaches it as a Bearer header on every request.
builder.Services.AddHttpClient<PropertyApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<Auth0AuthHandler>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// MCP endpoint is mounted at /mcp so it doesn't collide with anything else
// you might add to this service later (health checks, etc).
app.MapMcp("/mcp");

app.Run();
