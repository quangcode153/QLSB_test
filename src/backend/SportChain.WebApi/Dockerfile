# ==============================================================================
# SportChain.WebApi - Multi-stage Production Dockerfile (.NET 8.0)
# ==============================================================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copy csproj files for optimal layer caching during dotnet restore
COPY ["src/shared/SportChain.Shared/SportChain.Shared.csproj", "src/shared/SportChain.Shared/"]
COPY ["src/backend/SportChain.WebApi/SportChain.WebApi.csproj", "src/backend/SportChain.WebApi/"]

# 2. Restore NuGet packages
RUN dotnet restore "src/backend/SportChain.WebApi/SportChain.WebApi.csproj"

# 3. Copy source code
COPY ["src/shared/SportChain.Shared/", "src/shared/SportChain.Shared/"]
COPY ["src/backend/SportChain.WebApi/", "src/backend/SportChain.WebApi/"]

# 4. Build and publish release bundle
WORKDIR "/src/src/backend/SportChain.WebApi"
RUN dotnet publish "SportChain.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published outputs from build stage
COPY --from=build /app/publish .

# Default ASP.NET Core environment configurations
ENV ASPNETCORE_URLS=http://+:5097
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose backend port
EXPOSE 5097

# Start ASP.NET Core WebApi
ENTRYPOINT ["dotnet", "SportChain.WebApi.dll"]
