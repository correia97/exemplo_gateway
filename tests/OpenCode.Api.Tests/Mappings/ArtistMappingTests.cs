using OpenCode.Domain.Entities;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Api.Tests.Mappings;

public class ArtistMappingTests
{
    [Fact]
    public void ToResponse_Maps_All_Properties()
    {
        var genre = new Genre { Id = 1, Name = "Rock" };
        var artist = new Artist
        {
            Id = 1,
            Name = "The Beatles",
            Biography = "British rock band",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { Genre = genre, GenreId = 1 }
            },
            CreatedAt = new DateTime(2024, 1, 1),
            UpdatedAt = new DateTime(2024, 1, 1)
        };
        var response = artist.ToResponse();

        Assert.Equal(artist.Id, response.Id);
        Assert.Equal(artist.Name, response.Name);
        Assert.Equal(artist.Biography, response.Biography);
        Assert.Equal(artist.CreatedAt, response.CreatedAt);
        Assert.Equal(artist.UpdatedAt, response.UpdatedAt);
    }

    [Fact]
    public void ToResponse_Maps_Genres()
    {
        var rock = new Genre { Id = 1, Name = "Rock" };
        var pop = new Genre { Id = 2, Name = "Pop" };
        var artist = new Artist
        {
            Id = 1,
            Name = "Test",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { Genre = rock, GenreId = 1 },
                new() { Genre = pop, GenreId = 2 }
            }
        };
        var response = artist.ToResponse();

        Assert.Equal(2, response.Genres.Count);
        Assert.Contains(response.Genres, g => g.Id == 1 && g.Name == "Rock");
        Assert.Contains(response.Genres, g => g.Id == 2 && g.Name == "Pop");
    }

    [Fact]
    public void ToResponse_When_No_Genres_Returns_Empty_List()
    {
        var artist = new Artist { Id = 1, Name = "Test", ArtistGenres = new List<ArtistGenre>() };
        var response = artist.ToResponse();

        Assert.Empty(response.Genres);
    }
}
