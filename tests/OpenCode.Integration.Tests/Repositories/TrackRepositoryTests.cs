using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Data;
using OpenCode.Integration.Tests.Fixtures;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Integration.Tests.Repositories;

[Collection("PostgresIntegration")]
public class TrackRepositoryTests : IntegrationTestBase
{
    public TrackRepositoryTests(PostgresFixture fixture) : base(fixture) { }

    private static ITrackRepository CreateRepo(MusicContext ctx)
    {
        return new TrackRepository(ctx);
    }

    [Fact]
    public async Task AddAndGetById_ReturnsTrack()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var t = new Track { Name = "Come Together", TrackNumber = 1 };
        var created = await repo.AddAsync(t);
        Assert.True(created.Id > 0);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Come Together", fetched.Name);
    }

    [Fact]
    public async Task GetAll_Pagination_CorrectCount()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        for (int i = 0; i < 5; i++)
            ctx.Tracks.Add(new Track { Name = $"Track {i}", TrackNumber = i + 1 });
        await ctx.SaveChangesAsync();
        var result = await repo.GetAllAsync(null, null);
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task FilterByName_ReturnsMatches()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        ctx.Tracks.Add(new Track { Name = "Song One", TrackNumber = 1 });
        ctx.Tracks.Add(new Track { Name = "Song Two", TrackNumber = 2 });
        ctx.Tracks.Add(new Track { Name = "Another", TrackNumber = 3 });
        await ctx.SaveChangesAsync();
        var result = await repo.GetAllAsync("Song", null);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetByAlbumId_ReturnsTracks()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var artist = new Artist { Name = "Test" };
        ctx.Artists.Add(artist);
        await ctx.SaveChangesAsync();
        var album = new Album { Title = "Album", ArtistId = artist.Id };
        ctx.Albums.Add(album);
        await ctx.SaveChangesAsync();
        ctx.Tracks.Add(new Track { Name = "T1", TrackNumber = 1, AlbumId = album.Id });
        ctx.Tracks.Add(new Track { Name = "T2", TrackNumber = 2, AlbumId = album.Id });
        ctx.Tracks.Add(new Track { Name = "Standalone", TrackNumber = 1, AlbumId = null, IsStandalone = true });
        await ctx.SaveChangesAsync();
        var result = await repo.GetByAlbumIdAsync(album.Id);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var t = new Track { Name = "Original", TrackNumber = 1 };
        var created = await repo.AddAsync(t);
        created.Name = "Updated";
        await repo.UpdateAsync(created);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Updated", fetched.Name);
    }

    [Fact]
    public async Task Delete_RemovesTrack()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var t = new Track { Name = "Temp", TrackNumber = 1 };
        var created = await repo.AddAsync(t);
        await repo.DeleteAsync(created);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.Null(fetched);
    }
}
