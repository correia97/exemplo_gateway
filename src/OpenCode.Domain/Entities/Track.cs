namespace OpenCode.Domain.Entities;

public class Track : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int TrackNumber { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Lyrics { get; set; }
    public int? AlbumId { get; set; }
    public Album? Album { get; set; }
    public bool IsStandalone { get; set; } = false;
}
