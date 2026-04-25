using OpenCode.Domain.Entities;

namespace OpenCode.Music.Api.Dtos;

public record ArtistResponse(
    int Id,
    string Name,
    string? Biography,
    List<GenreSummary> Genres,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record GenreSummary(
    int Id,
    string Name
);

public static class ArtistMapping
{
    public static ArtistResponse ToResponse(this Artist artist)
        => new ArtistResponse(
            artist.Id,
            artist.Name,
            artist.Biography,
            artist.ArtistGenres
                .Select(ag => new GenreSummary(ag.Genre.Id, ag.Genre.Name))
                .ToList(),
            artist.CreatedAt,
            artist.UpdatedAt);
}