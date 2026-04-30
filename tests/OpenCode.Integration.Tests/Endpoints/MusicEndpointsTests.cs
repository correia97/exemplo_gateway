using System.Net;
using System.Net.Http.Json;
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
        var app = builder.Build();
        app.MapGroup("/api/genres").MapGenreEndpoints();
        app.MapGroup("/api/artists").MapArtistEndpoints();
        app.MapGroup("/api/albums").MapAlbumEndpoints();
        app.MapGroup("/api/tracks").MapTrackEndpoints();
        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task GetGenres_ReturnsOk()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.GetAsync("/api/genres");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateGenre_Returns201()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.PostAsJsonAsync("/api/genres", new { Name = "Pop" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetArtists_ReturnsOk()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.GetAsync("/api/artists");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateArtist_Returns201()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.PostAsJsonAsync("/api/artists", new { Name = "New Band" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNonExistentGenre_Returns404()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.DeleteAsync("/api/genres/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNonExistentArtist_Returns404()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.DeleteAsync("/api/artists/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
