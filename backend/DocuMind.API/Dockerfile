# Multi-stage build for ASP.NET Core 9 Web API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and csproj files for layer caching
COPY DocuMind.sln ./
COPY backend/DocuMind.Domain/DocuMind.Domain.csproj backend/DocuMind.Domain/
COPY backend/DocuMind.Application/DocuMind.Application.csproj backend/DocuMind.Application/
COPY backend/DocuMind.Infrastructure/DocuMind.Infrastructure.csproj backend/DocuMind.Infrastructure/
COPY backend/DocuMind.API/DocuMind.API.csproj backend/DocuMind.API/

RUN dotnet restore backend/DocuMind.API/DocuMind.API.csproj

# Copy complete backend source code
COPY backend/ ./backend/

# Build and publish release binaries
WORKDIR /src/backend/DocuMind.API
RUN dotnet publish DocuMind.API.csproj -c Release -o /app/publish --no-restore

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DocuMind.API.dll"]

