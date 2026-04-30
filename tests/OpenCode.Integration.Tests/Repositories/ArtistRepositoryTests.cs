using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Data;
using OpenCode.Integration.Tests.Fixtures;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Integration.Tests.Repositories;

[Collection("PostgresIntegration")]
public class ArtistRepositoryTests : IntegrationTestBase
{
    public ArtistRepositoryTests(PostgresFixture fixture) : base(fixture) { }

    private static ArtistRepository CreateRepo(MusicContext ctx)
    {
        return new ArtistRepository(ctx);
    }

    [Fact]
    public async Task AddAndGetById_ReturnsArtist()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var a = new Artist { Name = "The Beatles", Biography = "British rock band" };
        var created = await repo.AddAsync(a);
        Assert.True(created.Id > 0);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("The Beatles", fetched.Name);
    }

    [Fact]
    public async Task CreateWithGenreAssociations()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var rock = await ctx.Genres.FirstAsync(g => g.Name == "Rock");
        var artist = new Artist { Name = "Queen", ArtistGenres = new List<ArtistGenre> { new() { GenreId = rock.Id } } };
        var created = await repo.AddAsync(artist);
        var withGenres = await repo.GetByIdWithGenresAsync(created.Id);
        Assert.NotNull(withGenres);
        Assert.NotEmpty(withGenres.ArtistGenres);
    }

    [Fact]
    public async Task GetAll_Pagination_CorrectCount()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        ctx.Artists.Add(new Artist { Name = "Artist A" });
        ctx.Artists.Add(new Artist { Name = "Artist B" });
        ctx.Artists.Add(new Artist { Name = "Artist C" });
        await ctx.SaveChangesAsync();
        var result = await repo.GetAllAsync(null, null);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var a = new Artist { Name = "Queen" };
        var created = await repo.AddAsync(a);
        created.Biography = "Legendary";
        await repo.UpdateAsync(created);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Legendary", fetched.Biography);
    }

    [Fact]
    public async Task Delete_RemovesArtist()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var a = new Artist { Name = "Temporary" };
        var created = await repo.AddAsync(a);
        await repo.DeleteAsync(created);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.Null(fetched);
    }
}
