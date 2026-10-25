# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and central package management manifests
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["global.json", "./"]
COPY ["dotnet-web-foundation.slnx", "./"]

# Copy project files for caching dependency restore layer
COPY ["src/Foundation.Domain/Foundation.Domain.csproj", "src/Foundation.Domain/"]
COPY ["src/Foundation.Application/Foundation.Application.csproj", "src/Foundation.Application/"]
COPY ["src/Foundation.Infrastructure/Foundation.Infrastructure.csproj", "src/Foundation.Infrastructure/"]
COPY ["src/Foundation.Migrations.SqlServer/Foundation.Migrations.SqlServer.csproj", "src/Foundation.Migrations.SqlServer/"]
COPY ["src/Foundation.Migrations.MySql/Foundation.Migrations.MySql.csproj", "src/Foundation.Migrations.MySql/"]
COPY ["src/Foundation.Web/Foundation.Web.csproj", "src/Foundation.Web/"]

RUN dotnet restore src/Foundation.Web/Foundation.Web.csproj

# Copy source code
COPY src/ src/

# Publish Release build
WORKDIR /src/src/Foundation.Web
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Stage 2: Runtime (Hardened Non-Root Chiseled Ubuntu Noble)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

# Run as non-root user (built-in $APP_UID 1654 in .NET 10 chiseled images)
USER $APP_UID

ENTRYPOINT ["dotnet", "Foundation.Web.dll"]
