namespace OpenCode.Domain.Entities;

public class Character : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Race { get; set; } = string.Empty;
    public string Ki { get; set; } = "0";
    public string? MaxKi { get; set; }
    public bool IsEarthling { get; set; }
    public string? IntroductionPhase { get; set; }
    public string? Description { get; set; }
    public string? PictureUrl { get; set; }
    public int? PlanetId { get; set; }
    public Planet? Planet { get; set; }
    public ICollection<Transformation> Transformations { get; set; } = new List<Transformation>();
}
