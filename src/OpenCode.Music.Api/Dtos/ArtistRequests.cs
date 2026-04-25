namespace OpenCode.Music.Api.Dtos;

public record CreateArtistRequest(
    string Name,
    string? Biography,
    List<int>? GenreIds
);

public record UpdateArtistRequest(
    string Name,
    string? Biography,
    List<int>? GenreIds
);