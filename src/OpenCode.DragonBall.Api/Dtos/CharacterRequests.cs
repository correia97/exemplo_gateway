namespace OpenCode.DragonBall.Api.Dtos;

public record CreateCharacterRequest(
    string Name,
    bool IsEarthling,
    string? IntroductionPhase,
    string? PictureUrl
);

public record UpdateCharacterRequest(
    string Name,
    bool IsEarthling,
    string? IntroductionPhase,
    string? PictureUrl
);