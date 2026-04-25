using OpenCode.Domain.Entities;

namespace OpenCode.Music.Api.Dtos;

public record GenreResponse(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public static class GenreMapping
{
    public static GenreResponse ToResponse(this Genre genre)
        => new GenreResponse(
            genre.Id,
            genre.Name,
            genre.Description,
            genre.CreatedAt,
            genre.UpdatedAt);
}