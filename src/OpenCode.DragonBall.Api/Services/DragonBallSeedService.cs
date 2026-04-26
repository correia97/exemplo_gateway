using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;

namespace OpenCode.DragonBall.Api.Services;

public class DragonBallSeedService
{
    private readonly DragonBallContext _context;

    public DragonBallSeedService(DragonBallContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Characters.AnyAsync())
            return;

        var earth = new Planet { Name = "Earth" };
        var namek = new Planet { Name = "Namek" };
        var vegeta = new Planet { Name = "Planet Vegeta" };
        var beerusPlanet = new Planet { Name = "Beerus's Planet" };
        var unknown = new Planet { Name = "Unknown" };

        _context.Planets.AddRange(earth, namek, vegeta, beerusPlanet, unknown);
        await _context.SaveChangesAsync();

        var goku = new Character
        {
            Name = "Goku", Race = "Saiyan", Ki = "60 septillion", MaxKi = "2.5 sextillion",
            IsEarthling = true, IntroductionPhase = "Dragon Ball", Description = "The main protagonist of Dragon Ball, a Saiyan raised on Earth. Constantly pushing his limits to protect his friends and the universe.",
            PictureUrl = "https://dragonball-api.com/characters/goku.webp", PlanetId = earth.Id,
            Transformations = new List<Transformation>
            {
                new() { Name = "Base", Ki = "60 septillion" },
                new() { Name = "Super Saiyan", Ki = "3 quintillion", Description = "First Super Saiyan transformation, achieved through rage" },
                new() { Name = "Super Saiyan 2", Ki = "6 quintillion", Description = "Ascended Super Saiyan with increased speed and power" },
                new() { Name = "Super Saiyan 3", Ki = "24 quintillion", Description = "Ultimate Super Saiyan form with long hair and no eyebrows" },
                new() { Name = "Super Saiyan God", Ki = "600 quintillion", Description = "Godly ki transformation, slim red-haired form" },
                new() { Name = "Ultra Instinct", Ki = "2.5 sextillion", Description = "The ultimate technique, allowing the body to move independently of thought" }
            }
        };

        var vegetaChar = new Character
        {
            Name = "Vegeta", Race = "Saiyan", Ki = "54 septillion", MaxKi = "2.3 sextillion",
            IsEarthling = false, IntroductionPhase = "Dragon Ball Z", Description = "Prince of the Saiyans, rival of Goku. Proud warrior who eventually becomes a hero and family man.",
            PictureUrl = "https://dragonball-api.com/characters/vegeta.webp", PlanetId = vegeta.Id,
            Transformations = new List<Transformation>
            {
                new() { Name = "Base", Ki = "54 septillion" },
                new() { Name = "Super Saiyan", Ki = "2.7 quintillion" },
                new() { Name = "Super Saiyan 2", Ki = "5.4 quintillion" },
                new() { Name = "Super Saiyan God", Ki = "540 quintillion" },
                new() { Name = "Ultra Ego", Ki = "2.3 sextillion", Description = "God of Destruction technique that grows stronger as the user takes damage" }
            }
        };

        var piccolo = new Character
        {
            Name = "Piccolo", Race = "Namekian", Ki = "28 septillion", MaxKi = "480 quintillion",
            IsEarthling = false, IntroductionPhase = "Dragon Ball Z", Description = "A Namekian warrior, formerly Goku's enemy, now one of Earth's greatest protectors and Gohan's mentor.",
            PictureUrl = "https://dragonball-api.com/characters/piccolo.webp", PlanetId = namek.Id,
            Transformations = new List<Transformation>
            {
                new() { Name = "Base", Ki = "28 septillion" },
                new() { Name = "Potential Unleashed", Ki = "240 quintillion", Description = "Unlocked hidden potential through Guru's power" },
                new() { Name = "Orange Piccolo", Ki = "480 quintillion", Description = "Awakened form granted by Shenron, vastly increases power" }
            }
        };

        var gohan = new Character
        {
            Name = "Gohan", Race = "Half-Saiyan", Ki = "45 septillion", MaxKi = "900 quintillion",
            IsEarthling = true, IntroductionPhase = "Dragon Ball Z", Description = "Goku's eldest son, a Half-Saiyan with incredible latent potential. Prefers academics over fighting but rises when needed.",
            PictureUrl = "https://dragonball-api.com/characters/gohan.webp", PlanetId = earth.Id,
            Transformations = new List<Transformation>
            {
                new() { Name = "Base", Ki = "45 septillion" },
                new() { Name = "Super Saiyan", Ki = "2.25 quintillion" },
                new() { Name = "Super Saiyan 2", Ki = "4.5 quintillion", Description = "First achieved during the Cell Games against Perfect Cell" },
                new() { Name = "Beast", Ki = "900 quintillion", Description = "Ultimate form that pushes beyond Super Saiyan 2 limits" }
            }
        };

        var trunks = new Character
        {
            Name = "Trunks", Race = "Half-Saiyan", Ki = "30 septillion", MaxKi = "360 quintillion",
            IsEarthling = true, IntroductionPhase = "Dragon Ball Z", Description = "Vegeta's son from a future timeline. Travels back in time to warn the Z-Fighters of the Android threat.",
            PictureUrl = "https://dragonball-api.com/characters/trunks.webp", PlanetId = earth.Id,
            Transformations = new List<Transformation>
            {
                new() { Name = "Base", Ki = "30 septillion" },
                new() { Name = "Super Saiyan", Ki = "1.5 quintillion" },
                new() { Name = "Super Saiyan 2", Ki = "3 quintillion" },
                new() { Name = "Super Saiyan Rage", Ki = "360 quintillion", Description = "A form unique to Future Trunks, born from rage and desperation" }
            }
        };

        var frieza = new Character
        {
            Name = "Frieza", Race = "Frieza Race", Ki = "48 septillion", MaxKi = "2.8 sextillion",
            IsEarthling = false, IntroductionPhase = "Dragon Ball Z", Description = "Emperor of the universe, responsible for the destruction of Planet Vegeta. A ruthless tyrant who rules through fear.",
            PictureUrl = "https://dragonball-api.com/characters/frieza.webp", PlanetId = unknown.Id,
            Transformations = new List<Transformation>
            {
                new() { Name = "First Form", Ki = "530 thousand", Description = "Suppressed form to control power" },
                new() { Name = "Second Form", Ki = "1 million+" },
                new() { Name = "Third Form", Ki = "2 million+" },
                new() { Name = "Final Form", Ki = "48 septillion", Description = "True form, compact and extremely powerful" },
                new() { Name = "Golden Frieza", Ki = "2.8 sextillion", Description = "God-like transformation achieved through training in Hell" }
            }
        };

        var cell = new Character
        {
            Name = "Cell", Race = "Bio-Android", Ki = "40 septillion", MaxKi = "400 quintillion",
            IsEarthling = false, IntroductionPhase = "Dragon Ball Z", Description = "A biological android created by Dr. Gero's computer, combining cells of the greatest warriors. Seeks perfection through absorbing Androids 17 and 18.",
            PictureUrl = "https://dragonball-api.com/characters/cell.webp", PlanetId = unknown.Id,
            Transformations = new List<Transformation>
            {
                new() { Name = "Imperfect Cell", Ki = "10 septillion" },
                new() { Name = "Semi-Perfect Cell", Ki = "25 septillion", Description = "After absorbing Android 17" },
                new() { Name = "Perfect Cell", Ki = "40 septillion", Description = "Perfect form after absorbing Android 18" },
                new() { Name = "Super Perfect Cell", Ki = "400 quintillion", Description = "Regenerated after self-destruction, with Saiyan power boost" }
            }
        };

        var beerus = new Character
        {
            Name = "Beerus", Race = "God of Destruction", Ki = "7 quintillion", MaxKi = "25 quintillion",
            IsEarthling = false, IntroductionPhase = "Dragon Ball Super", Description = "The God of Destruction of Universe 7. A whimsical deity who destroys planets on a whim but has a soft spot for Earth's food.",
            PictureUrl = "https://dragonball-api.com/characters/beerus.webp", PlanetId = beerusPlanet.Id
        };

        var whis = new Character
        {
            Name = "Whis", Race = "Angel", Ki = "15 quintillion", MaxKi = "60 quintillion",
            IsEarthling = false, IntroductionPhase = "Dragon Ball Super", Description = "An angel and the attendant of Beerus. The strongest being in Universe 7, serves as Goku and Vegeta's martial arts teacher.",
            PictureUrl = "https://dragonball-api.com/characters/whis.webp", PlanetId = beerusPlanet.Id
        };

        var android18 = new Character
        {
            Name = "Android 18", Race = "Android", Ki = "20 septillion", MaxKi = "175 quintillion",
            IsEarthling = true, IntroductionPhase = "Dragon Ball Z", Description = "A former human turned cyborg by Dr. Gero. Eventually marries Krillin and becomes one of Earth's defenders.",
            PictureUrl = "https://dragonball-api.com/characters/android18.webp", PlanetId = earth.Id
        };

        _context.Characters.AddRange(goku, vegetaChar, piccolo, gohan, trunks, frieza, cell, beerus, whis, android18);
        await _context.SaveChangesAsync();
    }
}
