using OpenCode.Domain.Entities;

namespace OpenCode.Music.Api.Dtos;

public record TrackResponse(
    int Id,
    string Name,
    int TrackNumber,
    TimeSpan? Duration,
    string? Lyrics,
    int? AlbumId,
    string? AlbumTitle,
    bool IsStandalone,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public static class TrackMapping
{
    public static TrackResponse ToResponse(this Track track)
        => new TrackResponse(
            track.Id,
            track.Name,
            track.TrackNumber,
            track.Duration,
            track.Lyrics,
            track.AlbumId,
            track.Album?.Title,
            track.IsStandalone,
            track.CreatedAt,
            track.UpdatedAt);
}