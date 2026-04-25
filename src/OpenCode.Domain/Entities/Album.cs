namespace OpenCode.Domain.Entities;

public class Album : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public string? CoverUrl { get; set; }
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;
    public ICollection<Track> Tracks { get; set; } = new List<Track>();
}
