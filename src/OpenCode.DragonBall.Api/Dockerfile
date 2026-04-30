FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props .
COPY OpenCode.slnx .
COPY src/OpenCode.Domain/OpenCode.Domain.csproj src/OpenCode.Domain/
COPY src/OpenCode.ServiceDefaults/OpenCode.ServiceDefaults.csproj src/OpenCode.ServiceDefaults/
COPY src/OpenCode.DragonBall.Api/OpenCode.DragonBall.Api.csproj src/OpenCode.DragonBall.Api/
RUN dotnet restore src/OpenCode.DragonBall.Api/OpenCode.DragonBall.Api.csproj

COPY src/OpenCode.Domain/ src/OpenCode.Domain/
COPY src/OpenCode.ServiceDefaults/ src/OpenCode.ServiceDefaults/
COPY src/OpenCode.DragonBall.Api/ src/OpenCode.DragonBall.Api/
RUN dotnet publish src/OpenCode.DragonBall.Api/OpenCode.DragonBall.Api.csproj \
    -c Release \
    -o /app \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Use the built-in app user for security hardening
COPY --from=build /app .

USER app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OpenCode.DragonBall.Api.dll"]
