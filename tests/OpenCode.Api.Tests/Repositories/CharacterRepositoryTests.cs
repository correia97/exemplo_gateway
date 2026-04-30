using Microsoft.EntityFrameworkCore;
using OpenCode.DragonBall.Api.Repositories;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;

namespace OpenCode.Api.Tests.Repositories;

public class CharacterRepositoryTests
{
    private static DragonBallContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DragonBallContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new DragonBallContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task AddAsync_CreatesCharacter_WithGeneratedId()
    {
        using var context = CreateContext();
        var repo = new CharacterRepository(context);
        var character = new Character { Name = "Goku", Race = "Saiyan", Ki = "60.000.000" };
        var created = await repo.AddAsync(character);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCharacter_WhenExists()
    {
        using var context = CreateContext();
        var repo = new CharacterRepository(context);
        var character = new Character { Name = "Vegeta", Race = "Saiyan", Ki = "80.000.000" };
        var created = await repo.AddAsync(character);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Vegeta", retrieved.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var context = CreateContext();
        var repo = new CharacterRepository(context);
        var result = await repo.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults_WithDefaultPageSize()
    {
        using var context = CreateContext();
        var repo = new CharacterRepository(context);
        for (int i = 0; i < 3; i++)
            await repo.AddAsync(new Character { Name = $"Char{i}", Race = "Saiyan", Ki = "1000" });
        var result = await repo.GetAllAsync(null, null, null, null, null);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Data.Count());
    }

    [Fact]
    public async Task GetAllAsync_RespectsPageSizeClipping()
    {
        using var context = CreateContext();
        var repo = new CharacterRepository(context);
        for (int i = 0; i < 150; i++)
            await repo.AddAsync(new Character { Name = $"Char{i}", Race = "Saiyan", Ki = "1000" });
        var result = await repo.GetAllAsync(null, null, null, null, null, 1, 150);
        Assert.Equal(150, result.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByName()
    {
        using var context = CreateContext();
        var repo = new CharacterRepository(context);
        await repo.AddAsync(new Character { Name = "Goku", Race = "Saiyan", Ki = "1000" });
        await repo.AddAsync(new Character { Name = "Vegeta", Race = "Saiyan", Ki = "1000" });
        await repo.AddAsync(new Character { Name = "Gohan", Race = "Saiyan", Ki = "1000" });
        var result = await repo.GetAllAsync("Go", null, null, null, null);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByRace()
    {
        using var context = CreateContext();
        var repo = new CharacterRepository(context);
        await repo.AddAsync(new Character { Name = "Goku", Race = "Saiyan", Ki = "1000" });
        await repo.AddAsync(new Character { Name = "Piccolo", Race = "Namekian", Ki = "1000" });
        var result = await repo.GetAllAsync(null, "Saiyan", null, null, null);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesCharacter()
    {
        using var context = CreateContext();
        var repo = new CharacterRepository(context);
        var character = new Character { Name = "Goku", Race = "Saiyan", Ki = "1000" };
        var created = await repo.AddAsync(character);
        created.Name = "Goku SSJ";
        await repo.UpdateAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Goku SSJ", retrieved.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCharacter()
    {
        using var context = CreateContext();
        var repo = new CharacterRepository(context);
        var character = new Character { Name = "Goku", Race = "Saiyan", Ki = "1000" };
        var created = await repo.AddAsync(character);
        await repo.DeleteAsync(created);
        var retrieved = await repo.GetByIdAsync(created.Id);
        Assert.Null(retrieved);
    }
}
