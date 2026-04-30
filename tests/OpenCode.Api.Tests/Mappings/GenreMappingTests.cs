using OpenCode.Domain.Entities;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Api.Tests.Mappings;

public class GenreMappingTests
{
    [Fact]
    public void ToResponse_Maps_All_Properties()
    {
        var genre = new Genre
        {
            Id = 1,
            Name = "Rock",
            Description = "Rock music",
            CreatedAt = new DateTime(2024, 1, 1),
            UpdatedAt = new DateTime(2024, 1, 1)
        };
        var response = genre.ToResponse();

        Assert.Equal(genre.Id, response.Id);
        Assert.Equal(genre.Name, response.Name);
        Assert.Equal(genre.Description, response.Description);
        Assert.Equal(genre.CreatedAt, response.CreatedAt);
        Assert.Equal(genre.UpdatedAt, response.UpdatedAt);
    }

    [Fact]
    public void ToResponse_When_Description_Null_Maps_Null()
    {
        var genre = new Genre { Id = 2, Name = "Jazz", Description = null };
        var response = genre.ToResponse();

        Assert.Null(response.Description);
    }
}
