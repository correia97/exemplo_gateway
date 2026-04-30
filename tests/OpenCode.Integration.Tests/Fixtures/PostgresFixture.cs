using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using Testcontainers.PostgreSql;

namespace OpenCode.Integration.Tests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();
    public string DragonBallUserConnectionString { get; private set; } = string.Empty;
    public string MusicUserConnectionString { get; private set; } = string.Empty;

    public PostgresFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("opencode")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithPortBinding(5432, true)
            .WithCleanUp(true)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var dbOpts = new DbContextOptionsBuilder<DragonBallContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        using (var db = new DragonBallContext(dbOpts))
        {
            await db.Database.MigrateAsync();
        }

        var mOpts = new DbContextOptionsBuilder<MusicContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        using (var mdb = new MusicContext(mOpts))
        {
            await mdb.Database.MigrateAsync();
        }

        await SeedBaselineAsync();
        await CreateTestUsersAsync();
    }

    private async Task SeedBaselineAsync()
    {
        var dbOpts = new DbContextOptionsBuilder<DragonBallContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        using (var ctx = new DragonBallContext(dbOpts))
        {
            if (!await ctx.Planets.AnyAsync())
            {
                var earth = new Planet { Name = "Earth" };
                ctx.Planets.Add(earth);
                await ctx.SaveChangesAsync();

                ctx.Characters.Add(new Character
                {
                    Name = "Goku",
                    Race = "Saiyan",
                    Ki = "10.000.000",
                    MaxKi = "100.000.000.000.000",
                    IntroductionPhase = "Dragon Ball",
                    Description = "Main protagonist",
                    PlanetId = earth.Id
                });
                await ctx.SaveChangesAsync();
            }
        }

        var mOpts = new DbContextOptionsBuilder<MusicContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        using (var ctx = new MusicContext(mOpts))
        {
            if (!await ctx.Genres.AnyAsync())
            {
                ctx.Genres.Add(new Genre { Name = "Rock", Description = "Rock music genre" });
                await ctx.SaveChangesAsync();
            }
        }
    }

    private async Task CreateTestUsersAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = """
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'dragonball_user') THEN
                        CREATE ROLE dragonball_user WITH LOGIN PASSWORD 'test_password';
                    END IF;
                END $$;
                GRANT USAGE ON SCHEMA dragonball TO dragonball_user;
                GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA dragonball TO dragonball_user;
                ALTER DEFAULT PRIVILEGES IN SCHEMA dragonball GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO dragonball_user;
                """;
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = """
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'music_user') THEN
                        CREATE ROLE music_user WITH LOGIN PASSWORD 'test_password';
                    END IF;
                END $$;
                GRANT USAGE ON SCHEMA music TO music_user;
                GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA music TO music_user;
                ALTER DEFAULT PRIVILEGES IN SCHEMA music GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO music_user;
                """;
            await cmd.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
        builder.Username = "dragonball_user";
        builder.Password = "test_password";
        DragonBallUserConnectionString = builder.ConnectionString;

        builder.Username = "music_user";
        builder.Password = "test_password";
        MusicUserConnectionString = builder.ConnectionString;
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
