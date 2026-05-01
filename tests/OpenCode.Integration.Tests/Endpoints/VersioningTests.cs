using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenCode.Domain.Data;
using OpenCode.Domain.Interfaces;
using OpenCode.DragonBall.Api.Repositories;
using OpenCode.DragonBall.Api.Endpoints;
using OpenCode.Integration.Tests.Fixtures;

namespace OpenCode.Integration.Tests.Endpoints;

[Collection("PostgresIntegration")]
public class VersioningTests : IntegrationTestBase
{
    public VersioningTests(PostgresFixture fixture) : base(fixture) { }

    private async Task<IHost> CreateVersionedTestHost()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContextPool<DragonBallContext>(options =>
            options.UseNpgsql(ConnectionString));
        builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();

        // URL segment versioning with route parameter for version extraction
        builder.Services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        var app = builder.Build();
        app.UseCorrelationId();

        // Use version route parameter for proper UrlSegmentApiVersionReader integration
        var charactersApi = app.NewVersionedApi("Characters");
        var charactersV1 = charactersApi.MapGroup("api/v{version:apiVersion}/characters").HasApiVersion(1.0);
        charactersV1.MapCharacterEndpoints();

        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task VersionedRequest_WithV1_Returns200()
    {
        using var host = await CreateVersionedTestHost();
        var client = host.GetTestClient();
        var response = await client.GetAsync("/api/v1/characters?Page=1&PageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnversionedRequest_WithoutApiVersion_Returns404()
    {
        using var host = await CreateVersionedTestHost();
        var client = host.GetTestClient();
        // Without URL segment version (/api/v1/...), the route does not exist because
        // only versioned routes are registered (api/v{version:apiVersion}/characters).
        var response = await client.GetAsync("/api/characters");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
