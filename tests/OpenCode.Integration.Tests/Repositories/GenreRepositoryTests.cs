using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Data;
using OpenCode.Integration.Tests.Fixtures;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Integration.Tests.Repositories;

[Collection("PostgresIntegration")]
public class GenreRepositoryTests : IntegrationTestBase
{
    public GenreRepositoryTests(PostgresFixture fixture) : base(fixture) { }

    private static IGenreRepository CreateRepo(MusicContext ctx)
    {
        return new GenreRepository(ctx);
    }

    [Fact]
    public async Task AddAndGetById_ReturnsGenre()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var g = new Genre { Name = "Jazz", Description = "Smooth jazz" };
        var created = await repo.AddAsync(g);
        Assert.True(created.Id > 0);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Jazz", fetched.Name);
    }

    [Fact]
    public async Task GetAll_Pagination_CorrectCount()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        ctx.Genres.Add(new Genre { Name = "Pop" });
        ctx.Genres.Add(new Genre { Name = "Classical" });
        ctx.Genres.Add(new Genre { Name = "Electronic" });
        await ctx.SaveChangesAsync();
        var result = await repo.GetAllAsync();
        Assert.True(result.TotalCount >= 3);
    }

    [Fact]
    public async Task FilterByName_ReturnsMatches()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        ctx.Genres.Add(new Genre { Name = "Pop" });
        ctx.Genres.Add(new Genre { Name = "Punk" });
        ctx.Genres.Add(new Genre { Name = "Classical" });
        await ctx.SaveChangesAsync();
        var result = await repo.GetAllAsync(name: "Po");
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task Delete_RemovesGenre()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var g = new Genre { Name = "Temporary" };
        var created = await repo.AddAsync(g);
        await repo.DeleteAsync(created);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.Null(fetched);
    }
}
