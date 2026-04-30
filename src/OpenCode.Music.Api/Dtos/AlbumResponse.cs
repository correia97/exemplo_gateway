using OpenCode.Domain.Entities;

namespace OpenCode.Music.Api.Dtos;

public record AlbumResponse(
    int Id,
    string Title,
    DateOnly? ReleaseDate,
    string? CoverUrl,
    int ArtistId,
    string? ArtistName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public static class AlbumMapping
{
    public static AlbumResponse ToResponse(this Album album)
        => new AlbumResponse(
            album.Id,
            album.Title,
            album.ReleaseDate,
            album.CoverUrl,
            album.ArtistId,
            album.Artist?.Name ?? $"Artist #{album.ArtistId}",
            album.CreatedAt,
            album.UpdatedAt);
}