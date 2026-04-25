namespace OpenCode.Domain.Entities;

public class Artist : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
    public ICollection<Album> Albums { get; set; } = new List<Album>();
}
