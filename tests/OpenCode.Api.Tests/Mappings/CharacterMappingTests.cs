using OpenCode.Domain.Entities;
using OpenCode.DragonBall.Api.Dtos;

namespace OpenCode.Api.Tests.Mappings;

public class CharacterMappingTests
{
    private static Character CreateCharacter() => new()
    {
        Id = 1,
        Name = "Goku",
        Race = "Saiyan",
        Ki = "60.000.000",
        MaxKi = "100.000.000.000",
        IsEarthling = false,
        IntroductionPhase = "Z",
        Description = "Main protagonist",
        PictureUrl = "https://example.com/goku.png",
        Planet = new Planet { Id = 1, Name = "Earth" },
        Transformations = new List<Transformation>
        {
            new() { Id = 1, Name = "Super Saiyan", Ki = "150.000.000", Description = "First transformation", ImageUrl = "https://example.com/ssj.png" }
        },
        CreatedAt = new DateTime(2024, 1, 1),
        UpdatedAt = new DateTime(2024, 1, 1)
    };

    [Fact]
    public void ToResponse_Maps_All_Properties()
    {
        var character = CreateCharacter();
        var response = character.ToResponse();

        Assert.Equal(character.Id, response.Id);
        Assert.Equal(character.Name, response.Name);
        Assert.Equal(character.Race, response.Race);
        Assert.Equal(character.Ki, response.Ki);
        Assert.Equal(character.MaxKi, response.MaxKi);
        Assert.Equal(character.Description, response.Description);
        Assert.Equal(character.PictureUrl, response.ImageUrl);
        Assert.Equal(character.CreatedAt, response.CreatedAt);
        Assert.Equal(character.UpdatedAt, response.UpdatedAt);
    }

    [Fact]
    public void ToResponse_Maps_Planet()
    {
        var character = CreateCharacter();
        var response = character.ToResponse();

        Assert.NotNull(response.Planet);
        Assert.Equal(character.Planet!.Id, response.Planet.Id);
        Assert.Equal(character.Planet.Name, response.Planet.Name);
    }

    [Fact]
    public void ToResponse_Maps_Transformations()
    {
        var character = CreateCharacter();
        var response = character.ToResponse();

        Assert.Single(response.Transformations);
        Assert.Equal(character.Transformations.First().Name, response.Transformations[0].Name);
        Assert.Equal(character.Transformations.First().Ki, response.Transformations[0].Ki);
    }

    [Fact]
    public void ToResponse_When_Planet_Null_Sets_Null()
    {
        var character = CreateCharacter();
        character.Planet = null;
        var response = character.ToResponse();

        Assert.Null(response.Planet);
    }

    [Fact]
    public void ToResponse_When_No_Transformations_Returns_Empty_List()
    {
        var character = CreateCharacter();
        character.Transformations = new List<Transformation>();
        var response = character.ToResponse();

        Assert.Empty(response.Transformations);
    }

    [Fact]
    public void Planet_ToResponse_Maps_Correctly()
    {
        var planet = new Planet { Id = 2, Name = "Vegeta" };
        var response = planet.ToResponse();

        Assert.Equal(planet.Id, response.Id);
        Assert.Equal(planet.Name, response.Name);
    }

    [Fact]
    public void Transformation_ToResponse_Maps_Correctly()
    {
        var transformation = new Transformation
        {
            Id = 1,
            Name = "Super Saiyan God",
            Ki = "1.000.000.000.000",
            Description = "God form",
            ImageUrl = "https://example.com/ssg.png"
        };
        var response = transformation.ToResponse();

        Assert.Equal(transformation.Id, response.Id);
        Assert.Equal(transformation.Name, response.Name);
        Assert.Equal(transformation.Ki, response.Ki);
        Assert.Equal(transformation.Description, response.Description);
        Assert.Equal(transformation.ImageUrl, response.ImageUrl);
    }
}
