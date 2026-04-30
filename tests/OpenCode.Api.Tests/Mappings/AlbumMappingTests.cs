using OpenCode.Domain.Entities;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Api.Tests.Mappings;

public class AlbumMappingTests
{
    [Fact]
    public void ToResponse_Maps_All_Properties()
    {
        var artist = new Artist { Id = 1, Name = "The Beatles" };
        var album = new Album
        {
            Id = 1,
            Title = "Abbey Road",
            ReleaseDate = new DateOnly(1969, 9, 26),
            CoverUrl = "https://example.com/cover.jpg",
            ArtistId = 1,
            Artist = artist,
            CreatedAt = new DateTime(2024, 1, 1),
            UpdatedAt = new DateTime(2024, 1, 1)
        };
        var response = album.ToResponse();

        Assert.Equal(album.Id, response.Id);
        Assert.Equal(album.Title, response.Title);
        Assert.Equal(album.ReleaseDate, response.ReleaseDate);
        Assert.Equal(album.CoverUrl, response.CoverUrl);
        Assert.Equal(album.ArtistId, response.ArtistId);
        Assert.Equal(album.Artist.Name, response.ArtistName);
        Assert.Equal(album.CreatedAt, response.CreatedAt);
        Assert.Equal(album.UpdatedAt, response.UpdatedAt);
    }

    [Fact]
    public void ToResponse_When_Artist_Null_Sets_Null_ArtistName()
    {
        var album = new Album
        {
            Id = 1,
            Title = "Test",
            ArtistId = 1,
            Artist = null
        };
        var response = album.ToResponse();

        Assert.Null(response.ArtistName);
    }

    [Fact]
    public void ToResponse_When_ReleaseDate_Null_Maps_Null()
    {
        var album = new Album { Id = 1, Title = "Test", ArtistId = 1, Artist = new Artist { Name = "Test" }, ReleaseDate = null };
        var response = album.ToResponse();

        Assert.Null(response.ReleaseDate);
    }
}
