namespace OpenCode.Domain.Tests.Migrations;

public class SchemaIsolationTests
{
    private static readonly string ProjectDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "OpenCode.Domain"));

    [Fact]
    public void DragonBallMigration_OnlyReferencesDragonballSchema()
    {
        var migrationFiles = Directory.GetFiles(
            Path.Combine(ProjectDir, "Migrations", "DragonBall"), "*InitialCreate*.cs");

        Assert.NotEmpty(migrationFiles);

        foreach (var file in migrationFiles)
        {
            var content = File.ReadAllText(file);
            Assert.Matches("dragonball", content);
            Assert.DoesNotMatch("music", content);
            Assert.DoesNotMatch("keycloak", content);
        }
    }

    [Fact]
    public void MusicMigration_OnlyReferencesMusicSchema()
    {
        var migrationFiles = Directory.GetFiles(
            Path.Combine(ProjectDir, "Migrations", "Music"), "*InitialCreate*.cs");

        Assert.NotEmpty(migrationFiles);

        foreach (var file in migrationFiles)
        {
            var content = File.ReadAllText(file);
            Assert.Matches("music", content);
            Assert.DoesNotMatch("dragonball", content);
            Assert.DoesNotMatch("keycloak", content);
        }
    }

    [Fact]
    public void DragonBallMigration_CreatesOnlyCharacterTable()
    {
        var migrationFiles = Directory.GetFiles(
            Path.Combine(ProjectDir, "Migrations", "DragonBall"), "*InitialCreate*.cs");

        Assert.NotEmpty(migrationFiles);

        foreach (var file in migrationFiles)
        {
            var content = File.ReadAllText(file);
            Assert.Matches("Characters", content);
            Assert.DoesNotMatch("Genres", content);
            Assert.DoesNotMatch("Artists", content);
            Assert.DoesNotMatch("Albums", content);
            Assert.DoesNotMatch("Tracks", content);
        }
    }

    [Fact]
    public void MusicMigration_CreatesAllMusicTables()
    {
        var migrationFiles = Directory.GetFiles(
            Path.Combine(ProjectDir, "Migrations", "Music"), "*InitialCreate*.cs");

        Assert.NotEmpty(migrationFiles);

        foreach (var file in migrationFiles)
        {
            var content = File.ReadAllText(file);
            Assert.Matches("Genres", content);
            Assert.Matches("Artists", content);
            Assert.Matches("ArtistGenres", content);
            Assert.Matches("Albums", content);
            Assert.Matches("Tracks", content);
            Assert.DoesNotMatch("Characters", content);
        }
    }
}
