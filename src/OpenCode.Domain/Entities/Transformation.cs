namespace OpenCode.Domain.Entities;

public class Transformation
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Ki { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
}
