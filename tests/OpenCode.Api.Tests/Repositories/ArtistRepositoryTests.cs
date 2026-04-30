using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Api.Tests.Repositories;

public class ArtistRepositoryTests
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
    public async Task AddAsync_CreatesArtist_WithGeneratedId()
    {
        using var context = CreateContext();
        var repo = new ArtistRepository(context);
        var artist = new Artist { Name = "Test Band" };
        var created = await repo.AddAsync(artist);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsArtist_WhenExists()
    {
        using var context = CreateContext();
        var repo = new ArtistRepository(context);
        var artist = new Artist { Name = "The Beatles" };
        var created = await repo.AddAsync(artist);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("The Beatles", retrieved.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContext();
        var repo = new ArtistRepository(context);
        var result = await repo.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        using var context = CreateContext();
        var repo = new ArtistRepository(context);
        await repo.AddAsync(new Artist { Name = "Alpha" });
        await repo.AddAsync(new Artist { Name = "Beta" });
        await repo.AddAsync(new Artist { Name = "Gamma" });
        var result = await repo.GetAllAsync(null, null);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByName()
    {
        using var context = CreateContext();
        var repo = new ArtistRepository(context);
        await repo.AddAsync(new Artist { Name = "Alpha" });
        await repo.AddAsync(new Artist { Name = "Beta" });
        await repo.AddAsync(new Artist { Name = "Gamma" });
        var result = await repo.GetAllAsync("Be", null);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesArtist()
    {
        using var context = CreateContext();
        var repo = new ArtistRepository(context);
        var artist = new Artist { Name = "Queen", Biography = "Rock band" };
        var created = await repo.AddAsync(artist);
        created.Biography = "Legendary rock band";
        await repo.UpdateAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Legendary rock band", retrieved.Biography);
    }

    [Fact]
    public async Task DeleteAsync_RemovesArtist()
    {
        using var context = CreateContext();
        var repo = new ArtistRepository(context);
        var artist = new Artist { Name = "Test" };
        var created = await repo.AddAsync(artist);
        await repo.DeleteAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.Null(retrieved);
    }
}
