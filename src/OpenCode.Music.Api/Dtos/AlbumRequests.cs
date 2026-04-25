namespace OpenCode.Music.Api.Dtos;

public record CreateAlbumRequest(
    string Title,
    DateOnly? ReleaseDate,
    string? CoverUrl,
    int ArtistId
);

public record UpdateAlbumRequest(
    string Title,
    DateOnly? ReleaseDate,
    string? CoverUrl
);