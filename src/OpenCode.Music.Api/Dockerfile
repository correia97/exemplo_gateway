FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props .
COPY OpenCode.slnx .
COPY src/OpenCode.Domain/OpenCode.Domain.csproj src/OpenCode.Domain/
COPY src/OpenCode.ServiceDefaults/OpenCode.ServiceDefaults.csproj src/OpenCode.ServiceDefaults/
COPY src/OpenCode.Music.Api/OpenCode.Music.Api.csproj src/OpenCode.Music.Api/
RUN dotnet restore src/OpenCode.Music.Api/OpenCode.Music.Api.csproj

COPY src/OpenCode.Domain/ src/OpenCode.Domain/
COPY src/OpenCode.ServiceDefaults/ src/OpenCode.ServiceDefaults/
COPY src/OpenCode.Music.Api/ src/OpenCode.Music.Api/
RUN dotnet publish src/OpenCode.Music.Api/OpenCode.Music.Api.csproj \
    -c Release \
    -o /app \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN addgroup --system --gid 1000 appgroup \
    && adduser --system --uid 1000 --gid 1000 appuser

COPY --from=build /app .
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OpenCode.Music.Api.dll"]
