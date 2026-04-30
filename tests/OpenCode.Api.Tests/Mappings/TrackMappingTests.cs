using OpenCode.Domain.Entities;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Api.Tests.Mappings;

public class TrackMappingTests
{
    [Fact]
    public void ToResponse_Maps_All_Properties()
    {
        var album = new Album { Id = 1, Title = "Abbey Road" };
        var track = new Track
        {
            Id = 1,
            Name = "Come Together",
            TrackNumber = 1,
            Duration = TimeSpan.FromSeconds(259),
            Lyrics = "Here come old flat top...",
            AlbumId = 1,
            Album = album,
            IsStandalone = false,
            CreatedAt = new DateTime(2024, 1, 1),
            UpdatedAt = new DateTime(2024, 1, 1)
        };
        var response = track.ToResponse();

        Assert.Equal(track.Id, response.Id);
        Assert.Equal(track.Name, response.Name);
        Assert.Equal(track.TrackNumber, response.TrackNumber);
        Assert.Equal(track.Duration, response.Duration);
        Assert.Equal(track.Lyrics, response.Lyrics);
        Assert.Equal(track.AlbumId, response.AlbumId);
        Assert.Equal(track.Album.Title, response.AlbumTitle);
        Assert.Equal(track.IsStandalone, response.IsStandalone);
        Assert.Equal(track.CreatedAt, response.CreatedAt);
        Assert.Equal(track.UpdatedAt, response.UpdatedAt);
    }

    [Fact]
    public void ToResponse_When_Album_Null_Sets_Null_AlbumTitle()
    {
        var track = new Track
        {
            Id = 1,
            Name = "Standalone Single",
            TrackNumber = 1,
            AlbumId = null,
            Album = null,
            IsStandalone = true
        };
        var response = track.ToResponse();

        Assert.Null(response.AlbumId);
        Assert.Null(response.AlbumTitle);
        Assert.True(response.IsStandalone);
    }

    [Fact]
    public void ToResponse_When_Duration_Null_Maps_Null()
    {
        var track = new Track { Id = 1, Name = "Test", TrackNumber = 1, Duration = null };
        var response = track.ToResponse();

        Assert.Null(response.Duration);
    }
}
