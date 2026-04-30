using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Data;
using OpenCode.Integration.Tests.Fixtures;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Integration.Tests.Repositories;

[Collection("PostgresIntegration")]
public class AlbumRepositoryTests : IntegrationTestBase
{
    public AlbumRepositoryTests(PostgresFixture fixture) : base(fixture) { }

    private static AlbumRepository CreateRepo(MusicContext ctx)
    {
        return new AlbumRepository(ctx);
    }

    [Fact]
    public async Task AddAndGetById_ReturnsAlbum()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var artist = new Artist { Name = "Test" };
        ctx.Artists.Add(artist);
        await ctx.SaveChangesAsync();
        var a = new Album { Title = "Abbey Road", ArtistId = artist.Id };
        var created = await repo.AddAsync(a);
        Assert.True(created.Id > 0);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Abbey Road", fetched.Title);
    }

    [Fact]
    public async Task GetByIdWithArtist_LoadsArtist()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var artist = new Artist { Name = "The Beatles" };
        ctx.Artists.Add(artist);
        await ctx.SaveChangesAsync();
        var a = new Album { Title = "Let It Be", ArtistId = artist.Id };
        var created = await repo.AddAsync(a);
        var fetched = await repo.GetByIdWithArtistAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.NotNull(fetched.Artist);
        Assert.Equal("The Beatles", fetched.Artist.Name);
    }

    [Fact]
    public async Task GetAll_Pagination_CorrectCount()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var artist = new Artist { Name = "Test" };
        ctx.Artists.Add(artist);
        await ctx.SaveChangesAsync();
        for (int i = 0; i < 3; i++)
            ctx.Albums.Add(new Album { Title = $"Album {i}", ArtistId = artist.Id });
        await ctx.SaveChangesAsync();
        var result = await repo.GetAllAsync(null, null, null, null, null);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetByArtistId_ReturnsAlbumsForArtist()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var a1 = new Artist { Name = "A1" };
        var a2 = new Artist { Name = "A2" };
        ctx.Artists.AddRange(a1, a2);
        await ctx.SaveChangesAsync();
        ctx.Albums.Add(new Album { Title = "A1 Album", ArtistId = a1.Id });
        ctx.Albums.Add(new Album { Title = "A1 Album 2", ArtistId = a1.Id });
        ctx.Albums.Add(new Album { Title = "A2 Album", ArtistId = a2.Id });
        await ctx.SaveChangesAsync();
        var result = await repo.GetByArtistIdAsync(a1.Id);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task Delete_RemovesAlbum()
    {
        using var ctx = CreateMusicContext();
        var repo = CreateRepo(ctx);
        var artist = new Artist { Name = "Test" };
        ctx.Artists.Add(artist);
        await ctx.SaveChangesAsync();
        var a = new Album { Title = "Test", ArtistId = artist.Id };
        var created = await repo.AddAsync(a);
        await repo.DeleteAsync(created);
        var fetched = await repo.GetByIdAsync(created.Id);
        Assert.Null(fetched);
    }
}
