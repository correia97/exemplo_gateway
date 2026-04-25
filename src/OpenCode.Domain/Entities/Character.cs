namespace OpenCode.Domain.Entities;

public class Character : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsEarthling { get; set; }
    public string? IntroductionPhase { get; set; }
    public string? PictureUrl { get; set; }
}
