namespace OpenCode.Music.Api.Dtos;

public record CreateGenreRequest(
    string Name,
    string? Description
);

public record UpdateGenreRequest(
    string Name,
    string? Description
);