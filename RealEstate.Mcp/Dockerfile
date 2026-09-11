# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RealEstate.Mcp/RealEstate.Mcp.csproj", "RealEstate.Mcp/"]
RUN dotnet restore "RealEstate.Mcp/RealEstate.Mcp.csproj"
COPY . .
WORKDIR "/src/RealEstate.Mcp"
RUN dotnet publish "RealEstate.Mcp.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RealEstate.Mcp.dll"]
