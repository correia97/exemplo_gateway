using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Api.Tests.Repositories;

public class AlbumRepositoryTests
{
    private static (MusicContext, Artist) CreateContextWithArtist()
    {
        var options = new DbContextOptionsBuilder<MusicContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new MusicContext(options);
        context.Database.EnsureCreated();
        var artist = new Artist { Name = "Test Artist" };
        context.Artists.Add(artist);
        context.SaveChanges();
        return (context, artist);
    }

    [Fact]
    public async Task AddAsync_CreatesAlbum_WithGeneratedId()
    {
        var (context, artist) = CreateContextWithArtist();
        using var ctx = context;
        var repo = new AlbumRepository(context);
        var album = new Album { Title = "Test Album", ArtistId = artist.Id };
        var created = await repo.AddAsync(album);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAlbum_WhenExists()
    {
        var (context, artist) = CreateContextWithArtist();
        using var ctx = context;
        var repo = new AlbumRepository(context);
        var album = new Album { Title = "Abbey Road", ArtistId = artist.Id };
        var created = await repo.AddAsync(album);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Abbey Road", retrieved.Title);
    }

    [Fact]
    public async Task GetByIdAsyncWithArtist_ReturnsAlbumWithArtist()
    {
        var (context, artist) = CreateContextWithArtist();
        using var ctx = context;
        var repo = new AlbumRepository(context);
        var album = new Album { Title = "Test", ArtistId = artist.Id };
        var created = await repo.AddAsync(album);
        var retrieved = await repo.GetByIdWithArtistAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.NotNull(retrieved.Artist);
        Assert.Equal("Test Artist", retrieved.Artist.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var (context, _) = CreateContextWithArtist();
        using var ctx = context;
        var repo = new AlbumRepository(context);
        var result = await repo.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var (context, artist) = CreateContextWithArtist();
        using var ctx = context;
        var repo = new AlbumRepository(context);
        for (int i = 0; i < 3; i++)
            await repo.AddAsync(new Album { Title = $"Album {i}", ArtistId = artist.Id });
        var result = await repo.GetAllAsync(null, null, null, null, null);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByTitle()
    {
        var (context, artist) = CreateContextWithArtist();
        using var ctx = context;
        var repo = new AlbumRepository(context);
        await repo.AddAsync(new Album { Title = "Greatest Hits", ArtistId = artist.Id });
        await repo.AddAsync(new Album { Title = "Best of 2020", ArtistId = artist.Id });
        await repo.AddAsync(new Album { Title = "Live Concert", ArtistId = artist.Id });
        var result = await repo.GetAllAsync("Best", null, null, null, null);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetByArtistIdAsync_ReturnsAlbumsForArtist()
    {
        var (context, artist1) = CreateContextWithArtist();
        using var ctx = context;
        var artist2 = new Artist { Name = "Artist 2" };
        context.Artists.Add(artist2);
        context.SaveChanges();
        var repo = new AlbumRepository(context);
        await repo.AddAsync(new Album { Title = "A1", ArtistId = artist1.Id });
        await repo.AddAsync(new Album { Title = "A2", ArtistId = artist1.Id });
        await repo.AddAsync(new Album { Title = "B1", ArtistId = artist2.Id });
        var result = await repo.GetByArtistIdAsync(artist1.Id);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesAlbum()
    {
        var (context, artist) = CreateContextWithArtist();
        using var ctx = context;
        var repo = new AlbumRepository(context);
        var album = new Album { Title = "Original", ArtistId = artist.Id };
        var created = await repo.AddAsync(album);
        created.Title = "Updated";
        await repo.UpdateAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Updated", retrieved.Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAlbum()
    {
        var (context, artist) = CreateContextWithArtist();
        using var ctx = context;
        var repo = new AlbumRepository(context);
        var album = new Album { Title = "Test", ArtistId = artist.Id };
        var created = await repo.AddAsync(album);
        await repo.DeleteAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.Null(retrieved);
    }
}
