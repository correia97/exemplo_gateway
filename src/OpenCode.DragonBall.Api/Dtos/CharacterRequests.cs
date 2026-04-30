namespace OpenCode.DragonBall.Api.Dtos;

public record CreateCharacterRequest(
    string Name,
    string Race,
    string Ki,
    string? MaxKi,
    string? Description,
    string? PictureUrl,
    int? PlanetId,
    bool IsEarthling = false,
    string? IntroductionPhase = null
);

public record UpdateCharacterRequest(
    string Name,
    string Race,
    string Ki,
    string? MaxKi,
    string? Description,
    string? PictureUrl,
    int? PlanetId,
    bool IsEarthling = false,
    string? IntroductionPhase = null
);
