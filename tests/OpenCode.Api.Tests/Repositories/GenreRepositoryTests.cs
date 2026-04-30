using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Api.Tests.Repositories;

public class GenreRepositoryTests
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
    public async Task AddAsync_CreatesGenre_WithGeneratedId()
    {
        using var context = CreateContext();
        var repo = new GenreRepository(context);
        var genre = new Genre { Name = "Rock" };
        var created = await repo.AddAsync(genre);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsGenre_WhenExists()
    {
        using var context = CreateContext();
        var repo = new GenreRepository(context);
        var genre = new Genre { Name = "Jazz" };
        var created = await repo.AddAsync(genre);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Jazz", retrieved.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContext();
        var repo = new GenreRepository(context);
        var result = await repo.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        using var context = CreateContext();
        var repo = new GenreRepository(context);
        await repo.AddAsync(new Genre { Name = "Rock" });
        await repo.AddAsync(new Genre { Name = "Jazz" });
        await repo.AddAsync(new Genre { Name = "Classical" });
        var result = await repo.GetAllAsync(null);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByName()
    {
        using var context = CreateContext();
        var repo = new GenreRepository(context);
        await repo.AddAsync(new Genre { Name = "Rock" });
        await repo.AddAsync(new Genre { Name = "Jazz" });
        await repo.AddAsync(new Genre { Name = "Classical" });
        var result = await repo.GetAllAsync("Ja");
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesGenre()
    {
        using var context = CreateContext();
        var repo = new GenreRepository(context);
        var genre = new Genre { Name = "Rock" };
        var created = await repo.AddAsync(genre);
        created.Name = "Hard Rock";
        await repo.UpdateAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Hard Rock", retrieved.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesGenre()
    {
        using var context = CreateContext();
        var repo = new GenreRepository(context);
        var genre = new Genre { Name = "Rock" };
        var created = await repo.AddAsync(genre);
        await repo.DeleteAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.Null(retrieved);
    }
}
