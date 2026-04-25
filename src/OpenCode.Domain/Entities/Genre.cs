namespace OpenCode.Domain.Entities;

public class Genre : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
}
