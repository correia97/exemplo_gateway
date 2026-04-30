using Microsoft.EntityFrameworkCore;
using OpenCode.DragonBall.Api.Repositories;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Integration.Tests.Fixtures;

namespace OpenCode.Integration.Tests.Repositories;

[Collection("PostgresIntegration")]
public class CharacterRepositoryTests : IntegrationTestBase
{
    public CharacterRepositoryTests(PostgresFixture fixture) : base(fixture) { }

    private ICharacterRepository CreateRepo()
    {
        return new CharacterRepository(CreateDragonBallContext());
    }

    [Fact]
    public async Task AddAndGetById_ReturnsWithIncludes()
    {
        var repo = CreateRepo();
        using var ctx = CreateDragonBallContext();
        var planet = new Planet { Name = "Namek" };
        ctx.Planets.Add(planet);
        await ctx.SaveChangesAsync();
        var c = new Character { Name = "Piccolo", Race = "Namekian", Ki = "8000", PlanetId = planet.Id };
        var created = await repo.AddAsync(c);
        Assert.True(created.Id > 0);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.Planet);
        Assert.Equal("Namek", fetched.Planet.Name);
    }

    [Fact]
    public async Task GetAll_Pagination_CorrectCount()
    {
        var repo = CreateRepo();
        using var ctx = CreateDragonBallContext();
        var planet = new Planet { Name = "Earth" };
        ctx.Planets.Add(planet);
        await ctx.SaveChangesAsync();
        for (int i = 1; i <= 15; i++)
            ctx.Characters.Add(new Character { Name = $"C{i}", Race = "Saiyan", Ki = $"{i}000", PlanetId = planet.Id });
        await ctx.SaveChangesAsync();
        var page1 = await repo.GetAllAsync(null, null, null, null, null, 1, 10);
        Assert.True(page1.TotalCount >= 15);
        Assert.Equal(10, page1.Data.Count());
        var page2 = await repo.GetAllAsync(null, null, null, null, null, 2, 10);
        Assert.True(page2.Data.Count() >= 5);
    }

    [Fact]
    public async Task FilterByName_ReturnsMatches()
    {
        var repo = CreateRepo();
        using var ctx = CreateDragonBallContext();
        var planet = new Planet { Name = "Earth" };
        ctx.Planets.Add(planet);
        await ctx.SaveChangesAsync();
        ctx.Characters.Add(new Character { Name = "Gohan", Race = "Saiyan", Ki = "500", PlanetId = planet.Id });
        ctx.Characters.Add(new Character { Name = "Goten", Race = "Saiyan", Ki = "300", PlanetId = planet.Id });
        ctx.Characters.Add(new Character { Name = "Piccolo", Race = "Namekian", Ki = "800", PlanetId = planet.Id });
        await ctx.SaveChangesAsync();
        var result = await repo.GetAllAsync("Go", null, null, null, null);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var repo = CreateRepo();
        using var ctx = CreateDragonBallContext();
        var planet = new Planet { Name = "Earth" };
        ctx.Planets.Add(planet);
        await ctx.SaveChangesAsync();
        var c = new Character { Name = "Vegeta", Race = "Saiyan", Ki = "9000", PlanetId = planet.Id };
        var created = await repo.AddAsync(c);
        created.Ki = "9.000.000";
        await repo.UpdateAsync(created);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("9.000.000", fetched.Ki);
    }

    [Fact]
    public async Task Delete_RemovesCharacter()
    {
        var repo = CreateRepo();
        using var ctx = CreateDragonBallContext();
        var planet = new Planet { Name = "Earth" };
        ctx.Planets.Add(planet);
        await ctx.SaveChangesAsync();
        var c = new Character { Name = "Frieza", Race = "Frieza", Ki = "1000000", PlanetId = planet.Id };
        var created = await repo.AddAsync(c);
        await repo.DeleteAsync(created);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.Null(fetched);
    }
}
