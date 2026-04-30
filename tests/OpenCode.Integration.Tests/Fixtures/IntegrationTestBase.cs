using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenCode.Domain.Data;

namespace OpenCode.Integration.Tests.Fixtures;

public abstract class IntegrationTestBase
{
    protected PostgresFixture Fixture { get; }
    protected string ConnectionString => Fixture.ConnectionString;
    protected string DragonBallUserConnectionString => Fixture.DragonBallUserConnectionString;
    protected string MusicUserConnectionString => Fixture.MusicUserConnectionString;

    protected IntegrationTestBase(PostgresFixture fixture)
    {
        Fixture = fixture;
    }

    protected DragonBallContext CreateDragonBallContext()
    {
        var options = new DbContextOptionsBuilder<DragonBallContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new DragonBallContext(options);
    }

    protected MusicContext CreateMusicContext()
    {
        var options = new DbContextOptionsBuilder<MusicContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new MusicContext(options);
    }

    protected async Task<NpgsqlConnection> CreateDragonBallUserConnectionAsync()
    {
        var conn = new NpgsqlConnection(DragonBallUserConnectionString);
        await conn.OpenAsync();
        return conn;
    }

    protected async Task<NpgsqlConnection> CreateMusicUserConnectionAsync()
    {
        var conn = new NpgsqlConnection(MusicUserConnectionString);
        await conn.OpenAsync();
        return conn;
    }
}
