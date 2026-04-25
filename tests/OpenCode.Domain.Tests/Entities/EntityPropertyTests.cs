using System.Reflection;
using OpenCode.Domain.Entities;

namespace OpenCode.Domain.Tests.Entities;

public class EntityPropertyTests
{
    [Fact]
    public void Character_HasExpectedProperties()
    {
        var properties = typeof(Character).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (p.Name, p.PropertyType))
            .ToDictionary(p => p.Name, p => p.PropertyType);

        Assert.Equal(7, properties.Count);

        Assert.Equal(typeof(int), properties["Id"]);
        Assert.Equal(typeof(string), properties["Name"]);
        Assert.Equal(typeof(bool), properties["IsEarthling"]);
        Assert.Equal(typeof(string), properties["IntroductionPhase"]);
        Assert.Equal(typeof(string), properties["PictureUrl"]);
        Assert.Equal(typeof(DateTime), properties["CreatedAt"]);
        Assert.Equal(typeof(DateTime), properties["UpdatedAt"]);
    }

    [Fact]
    public void Genre_HasExpectedProperties()
    {
        var properties = typeof(Genre).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (p.Name, p.PropertyType))
            .ToDictionary(p => p.Name, p => p.PropertyType);

        Assert.Equal(6, properties.Count);

        Assert.Equal(typeof(string), properties["Name"]);
        Assert.Equal(typeof(string), properties["Description"]);
        Assert.Equal(typeof(ICollection<ArtistGenre>), properties["ArtistGenres"]);
    }

    [Fact]
    public void Artist_HasExpectedProperties()
    {
        var properties = typeof(Artist).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (p.Name, p.PropertyType))
            .ToDictionary(p => p.Name, p => p.PropertyType);

        Assert.Equal(7, properties.Count);

        Assert.Equal(typeof(string), properties["Name"]);
        Assert.Equal(typeof(string), properties["Biography"]);
        Assert.Equal(typeof(ICollection<ArtistGenre>), properties["ArtistGenres"]);
        Assert.Equal(typeof(ICollection<Album>), properties["Albums"]);
    }

    [Fact]
    public void ArtistGenre_HasExpectedProperties()
    {
        var properties = typeof(ArtistGenre).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (p.Name, p.PropertyType))
            .ToDictionary(p => p.Name, p => p.PropertyType);

        Assert.Equal(4, properties.Count);

        Assert.Equal(typeof(int), properties["ArtistId"]);
        Assert.Equal(typeof(Artist), properties["Artist"]);
        Assert.Equal(typeof(int), properties["GenreId"]);
        Assert.Equal(typeof(Genre), properties["Genre"]);
    }

    [Fact]
    public void Album_HasExpectedProperties()
    {
        var properties = typeof(Album).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (p.Name, p.PropertyType))
            .ToDictionary(p => p.Name, p => p.PropertyType);

        Assert.Equal(9, properties.Count);

        Assert.Equal(typeof(string), properties["Title"]);
        Assert.Equal(typeof(DateOnly?), properties["ReleaseDate"]);
        Assert.Equal(typeof(string), properties["CoverUrl"]);
        Assert.Equal(typeof(int), properties["ArtistId"]);
        Assert.Equal(typeof(Artist), properties["Artist"]);
        Assert.Equal(typeof(ICollection<Track>), properties["Tracks"]);
    }

    [Fact]
    public void Track_HasExpectedProperties()
    {
        var properties = typeof(Track).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (p.Name, p.PropertyType))
            .ToDictionary(p => p.Name, p => p.PropertyType);

        Assert.Equal(10, properties.Count);

        Assert.Equal(typeof(string), properties["Name"]);
        Assert.Equal(typeof(int), properties["TrackNumber"]);
        Assert.Equal(typeof(TimeSpan?), properties["Duration"]);
        Assert.Equal(typeof(string), properties["Lyrics"]);
        Assert.Equal(typeof(int?), properties["AlbumId"]);
        Assert.Equal(typeof(Album), properties["Album"]);
        Assert.Equal(typeof(bool), properties["IsStandalone"]);
    }

    [Fact]
    public void AllEntities_InheritBaseEntity()
    {
        var entityTypes = new[]
        {
            typeof(Character),
            typeof(Genre),
            typeof(Artist),
            typeof(Album),
            typeof(Track)
        };

        foreach (var type in entityTypes)
        {
            Assert.True(typeof(BaseEntity).IsAssignableFrom(type),
                $"{type.Name} should inherit from BaseEntity");
        }
    }

    [Fact]
    public void ArtistGenre_DoesNotInheritBaseEntity()
    {
        Assert.False(typeof(BaseEntity).IsAssignableFrom(typeof(ArtistGenre)),
            "ArtistGenre is a join entity and should not inherit BaseEntity");
    }
}
