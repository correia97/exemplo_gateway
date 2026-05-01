using System.Net;
using System.Net.Http.Json;
using Asp.Versioning;
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

        builder.Services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        var app = builder.Build();
        app.UseCorrelationId();

        var genresApi = app.NewVersionedApi("Genres");
        var genresV1 = genresApi.MapGroup("api/v1/genres").HasApiVersion(1.0);
        genresV1.MapGenreEndpoints();

        var artistsApi = app.NewVersionedApi("Artists");
        var artistsV1 = artistsApi.MapGroup("api/v1/artists").HasApiVersion(1.0);
        artistsV1.MapArtistEndpoints();

        var albumsApi = app.NewVersionedApi("Albums");
        var albumsV1 = albumsApi.MapGroup("api/v1/albums").HasApiVersion(1.0);
        albumsV1.MapAlbumEndpoints();

        var tracksApi = app.NewVersionedApi("Tracks");
        var tracksV1 = tracksApi.MapGroup("api/v1/tracks").HasApiVersion(1.0);
        tracksV1.MapTrackEndpoints();

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
