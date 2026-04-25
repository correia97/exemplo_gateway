namespace OpenCode.Domain.Entities;

public class ArtistGenre
{
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;
    public int GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}
