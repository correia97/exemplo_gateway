using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Integration.Tests.Fixtures;

namespace OpenCode.Integration.Tests.Schema;

[Collection("PostgresIntegration")]
public class SchemaIsolationTests : IntegrationTestBase
{
    public SchemaIsolationTests(PostgresFixture fixture) : base(fixture) { }

    // EF Core positive tests (D-08): Verify HasDefaultSchema() scopes queries correctly

    [Fact]
    public async Task DragonBallContext_QueriesOnlyDragonballSchema()
    {
        using var ctx = CreateDragonBallContext();
        var chars = await ctx.Characters.Take(10).ToListAsync();
        Assert.NotEmpty(chars);
        Assert.Contains(chars, c => c.Name == "Goku");
    }

    [Fact]
    public async Task MusicContext_QueriesOnlyMusicSchema()
    {
        using var ctx = CreateMusicContext();
        var genres = await ctx.Genres.Take(10).ToListAsync();
        Assert.NotEmpty(genres);
        Assert.Contains(genres, g => g.Name == "Rock");
    }

    [Fact]
    public async Task DragonBallContext_CharactersTableExistsInDragonballSchema()
    {
        using var ctx = CreateDragonBallContext();
        var hasChars = await ctx.Characters.AnyAsync();
        Assert.True(hasChars);
    }

    [Fact]
    public async Task DragonBallContext_CannotAccessMusicTables()
    {
        using var ctx = CreateDragonBallContext();
        var ex = await Assert.ThrowsAnyAsync<InvalidOperationException>(() =>
            ctx.Set<Genre>().AnyAsync());
        Assert.NotNull(ex);
    }

    // Raw SQL negative tests with user credentials (D-06, D-07)

    [Fact]
    public async Task DragonBallUser_CannotQueryMusicSchema()
    {
        await using var conn = await CreateDragonBallUserConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM music.\"Genres\"";
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(() =>
            cmd.ExecuteScalarAsync());
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task MusicUser_CannotQueryDragonballSchema()
    {
        await using var conn = await CreateMusicUserConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM dragonball.\"Characters\"";
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(() =>
            cmd.ExecuteScalarAsync());
        Assert.NotNull(ex);
    }
}
