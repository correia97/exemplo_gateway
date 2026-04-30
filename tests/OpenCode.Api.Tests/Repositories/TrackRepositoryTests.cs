using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Api.Tests.Repositories;

public class TrackRepositoryTests
{
    private static MusicContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MusicContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new MusicContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task AddAsync_CreatesTrack_WithGeneratedId()
    {
        using var context = CreateContext();
        var repo = new TrackRepository(context);
        var track = new Track { Name = "Test Track", TrackNumber = 1 };
        var created = await repo.AddAsync(track);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTrack_WhenExists()
    {
        using var context = CreateContext();
        var repo = new TrackRepository(context);
        var track = new Track { Name = "Come Together", TrackNumber = 1 };
        var created = await repo.AddAsync(track);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Come Together", retrieved.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContext();
        var repo = new TrackRepository(context);
        var result = await repo.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        using var context = CreateContext();
        var repo = new TrackRepository(context);
        for (int i = 0; i < 3; i++)
            await repo.AddAsync(new Track { Name = $"Track {i}", TrackNumber = i + 1 });
        var result = await repo.GetAllAsync(null, null);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByName()
    {
        using var context = CreateContext();
        var repo = new TrackRepository(context);
        await repo.AddAsync(new Track { Name = "Song One", TrackNumber = 1 });
        await repo.AddAsync(new Track { Name = "Song Two", TrackNumber = 2 });
        await repo.AddAsync(new Track { Name = "Another Track", TrackNumber = 3 });
        var result = await repo.GetAllAsync("Song", null);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetByAlbumIdAsync_ReturnsTracksForAlbum()
    {
        using var context = CreateContext();
        var repo = new TrackRepository(context);
        var artist = new Artist { Name = "Test" };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        var album = new Album { Title = "Album 1", ArtistId = artist.Id };
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        await repo.AddAsync(new Track { Name = "Track 1", TrackNumber = 1, AlbumId = album.Id });
        await repo.AddAsync(new Track { Name = "Track 2", TrackNumber = 2, AlbumId = album.Id });
        await repo.AddAsync(new Track { Name = "Standalone", TrackNumber = 1, AlbumId = null, IsStandalone = true });
        var result = await repo.GetByAlbumIdAsync(album.Id);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesTrack()
    {
        using var context = CreateContext();
        var repo = new TrackRepository(context);
        var track = new Track { Name = "Original", TrackNumber = 1, Duration = TimeSpan.FromSeconds(180) };
        var created = await repo.AddAsync(track);
        created.Duration = TimeSpan.FromSeconds(200);
        await repo.UpdateAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(TimeSpan.FromSeconds(200), retrieved.Duration);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTrack()
    {
        using var context = CreateContext();
        var repo = new TrackRepository(context);
        var track = new Track { Name = "Test", TrackNumber = 1 };
        var created = await repo.AddAsync(track);
        await repo.DeleteAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.Null(retrieved);
    }
}
