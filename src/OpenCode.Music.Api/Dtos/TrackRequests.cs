namespace OpenCode.Music.Api.Dtos;

public record CreateTrackRequest(
    string Name,
    int TrackNumber,
    TimeSpan? Duration,
    string? Lyrics,
    int? AlbumId,
    bool IsStandalone
);

public record UpdateTrackRequest(
    string Name,
    int TrackNumber,
    TimeSpan? Duration,
    string? Lyrics,
    int? AlbumId,
    bool IsStandalone
);