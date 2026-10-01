# ==============================================================================
# SportChain.Client - Blazor WebAssembly Dockerfile (.NET 8.0 + Nginx)
# ==============================================================================

# Stage 1: Build Blazor WebAssembly static assets
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copy csproj files for optimal layer caching
COPY ["src/shared/SportChain.Shared/SportChain.Shared.csproj", "src/shared/SportChain.Shared/"]
COPY ["src/frontend/SportChain.Client/SportChain.Client.csproj", "src/frontend/SportChain.Client/"]

# 2. Restore NuGet dependencies
RUN dotnet restore "src/frontend/SportChain.Client/SportChain.Client.csproj"

# 3. Copy source code
COPY ["src/shared/SportChain.Shared/", "src/shared/SportChain.Shared/"]
COPY ["src/frontend/SportChain.Client/", "src/frontend/SportChain.Client/"]

# 4. Publish Blazor WebAssembly release bundle
WORKDIR "/src/src/frontend/SportChain.Client"
RUN dotnet publish "SportChain.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Serve static WebAssembly distribution files with Nginx
FROM nginx:alpine AS final

# Remove default nginx html template
RUN rm -rf /usr/share/nginx/html/*

# Copy custom Nginx configuration for Blazor SPA routing
COPY src/frontend/SportChain.Client/nginx.conf /etc/nginx/conf.d/default.conf

# Copy static published web assets (wwwroot containing _framework, index.html, css, js)
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Expose HTTP port
EXPOSE 80

# Run Nginx in foreground
CMD ["nginx", "-g", "daemon off;"]
