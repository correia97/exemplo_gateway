using OpenCode.Domain.Entities;

namespace OpenCode.DragonBall.Api.Dtos;

public record CharacterResponse(
    int Id,
    string Name,
    bool IsEarthling,
    string? IntroductionPhase,
    string? PictureUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public static class CharacterMapping
{
    public static CharacterResponse ToResponse(this Character character)
        => new CharacterResponse(
            character.Id,
            character.Name,
            character.IsEarthling,
            character.IntroductionPhase,
            character.PictureUrl,
            character.CreatedAt,
            character.UpdatedAt);
}