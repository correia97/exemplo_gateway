using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Music.Api.Repositories;
using OpenCode.Music.Api.Endpoints;
using OpenCode.Integration.Tests.Fixtures;

namespace OpenCode.Integration.Tests.Endpoints;

[Collection("PostgresIntegration")]
public class MusicEndpointsTests : IntegrationTestBase
{
    public MusicEndpointsTests(PostgresFixture fixture) : base(fixture) { }

    private async Task<IHost> CreateTestHost()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContextPool<MusicContext>(options =>
            options.UseNpgsql(ConnectionString));
        builder.Services.AddScoped<IGenreRepository, GenreRepository>();
        builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
        builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
        builder.Services.AddScoped<ITrackRepository, TrackRepository>();

        // Auth middleware required by endpoint RequireAuthorization("ApiPolicy")
        builder.Services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, null);
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("editor");
            });
        });

        var app = builder.Build();
        app.UseCorrelationId();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGroup("/api/v1/genres").MapGenreEndpoints();
        app.MapGroup("/api/v1/artists").MapArtistEndpoints();
        app.MapGroup("/api/v1/albums").MapAlbumEndpoints();
        app.MapGroup("/api/v1/tracks").MapTrackEndpoints();
        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task GetGenres_ReturnsOk()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.GetAsync("/api/v1/genres");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateGenre_Returns201()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.PostAsJsonAsync("/api/v1/genres", new { Name = "Pop" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetArtists_ReturnsOk()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.GetAsync("/api/v1/artists");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateArtist_Returns201()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.PostAsJsonAsync("/api/v1/artists", new { Name = "New Band" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNonExistentGenre_Returns404()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.DeleteAsync("/api/v1/genres/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNonExistentArtist_Returns404()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.DeleteAsync("/api/v1/artists/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
