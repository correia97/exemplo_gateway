using OpenCode.Domain.Entities;

namespace OpenCode.DragonBall.Api.Dtos;

public record TransformationResponse(
    int Id,
    string Name,
    string Ki,
    string? Description,
    string? ImageUrl
);

public record PlanetResponse(
    int Id,
    string Name
);

public record CharacterResponse(
    int Id,
    string Name,
    string Race,
    string Ki,
    string? MaxKi,
    string? Description,
    string? ImageUrl,
    PlanetResponse? Planet,
    List<TransformationResponse> Transformations,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public static class CharacterMapping
{
    public static TransformationResponse ToResponse(this Transformation t)
        => new TransformationResponse(t.Id, t.Name, t.Ki, t.Description, t.ImageUrl);

    public static PlanetResponse ToResponse(this Planet p)
        => new PlanetResponse(p.Id, p.Name);

    public static CharacterResponse ToResponse(this Character character)
        => new CharacterResponse(
            character.Id,
            character.Name,
            character.Race,
            character.Ki,
            character.MaxKi,
            character.Description,
            character.PictureUrl,
            character.Planet?.ToResponse(),
            character.Transformations.Select(t => t.ToResponse()).ToList(),
            character.CreatedAt,
            character.UpdatedAt);
}
