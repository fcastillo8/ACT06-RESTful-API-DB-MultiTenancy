# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy solution and project files
COPY MultiTenantApi.sln .
COPY Api/Api.csproj Api/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Tests/Tests.csproj Tests/

# Restore dependencies
RUN dotnet restore MultiTenantApi.sln

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish Api/Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

COPY --from=build /app/publish .

# Railway uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}

ENTRYPOINT ["dotnet", "Api.dll"]
