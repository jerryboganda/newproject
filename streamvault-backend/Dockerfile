# StreamVault Backend - ASP.NET Core 8.0
# Multi-stage: build → runtime with security hardening

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies for better caching
COPY ["src/StreamVault.API/StreamVault.API.csproj", "src/StreamVault.API/"]
COPY ["src/StreamVault.Application/StreamVault.Application.csproj", "src/StreamVault.Application/"]
COPY ["src/StreamVault.Domain/StreamVault.Domain.csproj", "src/StreamVault.Domain/"]
COPY ["src/StreamVault.Infrastructure/StreamVault.Infrastructure.csproj", "src/StreamVault.Infrastructure/"]
RUN dotnet restore "src/StreamVault.API/StreamVault.API.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish with optimizations
WORKDIR "/src/src/StreamVault.API"
RUN dotnet publish "StreamVault.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install security updates and required packages
RUN apt-get update && apt-get upgrade -y && \
    apt-get install -y --no-install-recommends \
    ca-certificates \
    curl \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean

# Create non-root user for security
RUN groupadd -r streamvault && useradd -r -g streamvault streamvault

# Copy published app
COPY --from=build /app/publish .

# Create necessary directories with proper permissions
RUN mkdir -p /app/uploads /app/logs /app/temp && \
    chown -R streamvault:streamvault /app

# Switch to non-root user
USER streamvault

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost/api/health || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Expose ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "StreamVault.API.dll"]
