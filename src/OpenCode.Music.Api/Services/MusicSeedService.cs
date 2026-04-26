using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;

namespace OpenCode.Music.Api.Services;

public class MusicSeedService
{
    private readonly MusicContext _context;

    public MusicSeedService(MusicContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Artists.AnyAsync())
            return;

        var rock = new Genre { Name = "Rock", Description = "Classic rock music" };
        var pop = new Genre { Name = "Pop", Description = "Popular music" };
        var classicRock = new Genre { Name = "Classic Rock", Description = "Classic rock from the 60s-80s" };
        var hardRock = new Genre { Name = "Hard Rock", Description = "Hard rock and heavy metal" };
        var popRock = new Genre { Name = "Pop Rock", Description = "Pop-infused rock music" };
        var soul = new Genre { Name = "Soul", Description = "Soul and R&B music" };
        var alternative = new Genre { Name = "Alternative", Description = "Alternative rock" };

        _context.Genres.AddRange(rock, pop, classicRock, hardRock, popRock, soul, alternative);
        await _context.SaveChangesAsync();

        // --- Queen ---
        var queen = new Artist
        {
            Name = "Queen",
            Biography = "British rock band formed in 1970, known for their theatrical style and diverse musical influences. Led by the legendary Freddie Mercury.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = rock.Id }, new() { GenreId = classicRock.Id }, new() { GenreId = popRock.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "A Night at the Opera", ReleaseDate = new DateOnly(1975, 11, 21),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Bohemian Rhapsody", TrackNumber = 1, Duration = TimeSpan.FromSeconds(355) },
                        new() { Name = "You're My Best Friend", TrackNumber = 2, Duration = TimeSpan.FromSeconds(171) },
                        new() { Name = "Love of My Life", TrackNumber = 3, Duration = TimeSpan.FromSeconds(219) },
                        new() { Name = "I'm in Love with My Car", TrackNumber = 4, Duration = TimeSpan.FromSeconds(185) }
                    }
                },
                new()
                {
                    Title = "The Game", ReleaseDate = new DateOnly(1980, 6, 30),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Another One Bites the Dust", TrackNumber = 1, Duration = TimeSpan.FromSeconds(216) },
                        new() { Name = "Crazy Little Thing Called Love", TrackNumber = 2, Duration = TimeSpan.FromSeconds(164) },
                        new() { Name = "Play the Game", TrackNumber = 3, Duration = TimeSpan.FromSeconds(210) },
                        new() { Name = "Dragon Attack", TrackNumber = 4, Duration = TimeSpan.FromSeconds(258) }
                    }
                }
            }
        };

        // --- Michael Jackson ---
        var michaelJackson = new Artist
        {
            Name = "Michael Jackson",
            Biography = "The King of Pop, Michael Jackson revolutionized music video and pop culture. His album Thriller remains the best-selling album of all time.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = pop.Id }, new() { GenreId = soul.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "Thriller", ReleaseDate = new DateOnly(1982, 11, 30),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Thriller", TrackNumber = 1, Duration = TimeSpan.FromSeconds(358) },
                        new() { Name = "Billie Jean", TrackNumber = 2, Duration = TimeSpan.FromSeconds(294) },
                        new() { Name = "Beat It", TrackNumber = 3, Duration = TimeSpan.FromSeconds(258) },
                        new() { Name = "Wanna Be Startin' Somethin'", TrackNumber = 4, Duration = TimeSpan.FromSeconds(363) }
                    }
                },
                new()
                {
                    Title = "Bad", ReleaseDate = new DateOnly(1987, 8, 31),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Bad", TrackNumber = 1, Duration = TimeSpan.FromSeconds(247) },
                        new() { Name = "Smooth Criminal", TrackNumber = 2, Duration = TimeSpan.FromSeconds(258) },
                        new() { Name = "The Way You Make Me Feel", TrackNumber = 3, Duration = TimeSpan.FromSeconds(298) },
                        new() { Name = "Man in the Mirror", TrackNumber = 4, Duration = TimeSpan.FromSeconds(321) }
                    }
                }
            }
        };

        // --- Pink Floyd ---
        var pinkFloyd = new Artist
        {
            Name = "Pink Floyd",
            Biography = "English progressive rock band known for philosophical lyrics, sonic experimentation, and elaborate live shows. One of the most influential bands in history.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = rock.Id }, new() { GenreId = classicRock.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "The Dark Side of the Moon", ReleaseDate = new DateOnly(1973, 3, 1),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Speak to Me / Breathe", TrackNumber = 1, Duration = TimeSpan.FromSeconds(231) },
                        new() { Name = "Time", TrackNumber = 2, Duration = TimeSpan.FromSeconds(412) },
                        new() { Name = "Money", TrackNumber = 3, Duration = TimeSpan.FromSeconds(382) },
                        new() { Name = "Us and Them", TrackNumber = 4, Duration = TimeSpan.FromSeconds(465) }
                    }
                },
                new()
                {
                    Title = "The Wall", ReleaseDate = new DateOnly(1979, 11, 30),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Another Brick in the Wall, Pt. 2", TrackNumber = 1, Duration = TimeSpan.FromSeconds(239) },
                        new() { Name = "Comfortably Numb", TrackNumber = 2, Duration = TimeSpan.FromSeconds(383) },
                        new() { Name = "Hey You", TrackNumber = 3, Duration = TimeSpan.FromSeconds(281) },
                        new() { Name = "Run Like Hell", TrackNumber = 4, Duration = TimeSpan.FromSeconds(264) }
                    }
                }
            }
        };

        // --- Madonna ---
        var madonna = new Artist
        {
            Name = "Madonna",
            Biography = "The Queen of Pop, Madonna is known for reinventing her music and image. One of the best-selling female artists of all time.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = pop.Id }, new() { GenreId = popRock.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "Like a Virgin", ReleaseDate = new DateOnly(1984, 11, 12),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Like a Virgin", TrackNumber = 1, Duration = TimeSpan.FromSeconds(218) },
                        new() { Name = "Material Girl", TrackNumber = 2, Duration = TimeSpan.FromSeconds(240) },
                        new() { Name = "Angel", TrackNumber = 3, Duration = TimeSpan.FromSeconds(211) },
                        new() { Name = "Dress You Up", TrackNumber = 4, Duration = TimeSpan.FromSeconds(240) }
                    }
                },
                new()
                {
                    Title = "True Blue", ReleaseDate = new DateOnly(1986, 6, 30),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Papa Don't Preach", TrackNumber = 1, Duration = TimeSpan.FromSeconds(267) },
                        new() { Name = "True Blue", TrackNumber = 2, Duration = TimeSpan.FromSeconds(244) },
                        new() { Name = "Open Your Heart", TrackNumber = 3, Duration = TimeSpan.FromSeconds(250) },
                        new() { Name = "La Isla Bonita", TrackNumber = 4, Duration = TimeSpan.FromSeconds(243) }
                    }
                }
            }
        };

        // --- Nirvana ---
        var nirvana = new Artist
        {
            Name = "Nirvana",
            Biography = "American grunge band formed in 1987 by Kurt Cobain and Krist Novoselic. Pioneered the grunge movement and defined 90s alternative rock.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = rock.Id }, new() { GenreId = alternative.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "Nevermind", ReleaseDate = new DateOnly(1991, 9, 24),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Smells Like Teen Spirit", TrackNumber = 1, Duration = TimeSpan.FromSeconds(301) },
                        new() { Name = "Come as You Are", TrackNumber = 2, Duration = TimeSpan.FromSeconds(219) },
                        new() { Name = "Lithium", TrackNumber = 3, Duration = TimeSpan.FromSeconds(257) },
                        new() { Name = "In Bloom", TrackNumber = 4, Duration = TimeSpan.FromSeconds(255) }
                    }
                },
                new()
                {
                    Title = "In Utero", ReleaseDate = new DateOnly(1993, 9, 21),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Heart-Shaped Box", TrackNumber = 1, Duration = TimeSpan.FromSeconds(281) },
                        new() { Name = "Rape Me", TrackNumber = 2, Duration = TimeSpan.FromSeconds(170) },
                        new() { Name = "All Apologies", TrackNumber = 3, Duration = TimeSpan.FromSeconds(231) },
                        new() { Name = "Pennyroyal Tea", TrackNumber = 4, Duration = TimeSpan.FromSeconds(218) }
                    }
                }
            }
        };

        // --- U2 ---
        var u2 = new Artist
        {
            Name = "U2",
            Biography = "Irish rock band formed in 1976. Known for their anthemic sound and Bono's passionate vocals and activism. One of the most successful bands in history.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = rock.Id }, new() { GenreId = popRock.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "The Joshua Tree", ReleaseDate = new DateOnly(1987, 3, 9),
                    Tracks = new List<Track>
                    {
                        new() { Name = "With or Without You", TrackNumber = 1, Duration = TimeSpan.FromSeconds(296) },
                        new() { Name = "I Still Haven't Found What I'm Looking For", TrackNumber = 2, Duration = TimeSpan.FromSeconds(278) },
                        new() { Name = "Where the Streets Have No Name", TrackNumber = 3, Duration = TimeSpan.FromSeconds(317) },
                        new() { Name = "Bullet the Blue Sky", TrackNumber = 4, Duration = TimeSpan.FromSeconds(268) }
                    }
                },
                new()
                {
                    Title = "Achtung Baby", ReleaseDate = new DateOnly(1991, 11, 19),
                    Tracks = new List<Track>
                    {
                        new() { Name = "One", TrackNumber = 1, Duration = TimeSpan.FromSeconds(276) },
                        new() { Name = "Mysterious Ways", TrackNumber = 2, Duration = TimeSpan.FromSeconds(244) },
                        new() { Name = "The Fly", TrackNumber = 3, Duration = TimeSpan.FromSeconds(269) },
                        new() { Name = "Even Better Than the Real Thing", TrackNumber = 4, Duration = TimeSpan.FromSeconds(221) }
                    }
                }
            }
        };

        // --- Fleetwood Mac ---
        var fleetwoodMac = new Artist
        {
            Name = "Fleetwood Mac",
            Biography = "British-American rock band formed in 1967. Their 1977 album Rumours is one of the best-selling albums of all time.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = rock.Id }, new() { GenreId = popRock.Id }, new() { GenreId = classicRock.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "Rumours", ReleaseDate = new DateOnly(1977, 2, 4),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Go Your Own Way", TrackNumber = 1, Duration = TimeSpan.FromSeconds(218) },
                        new() { Name = "Dreams", TrackNumber = 2, Duration = TimeSpan.FromSeconds(257) },
                        new() { Name = "Don't Stop", TrackNumber = 3, Duration = TimeSpan.FromSeconds(192) },
                        new() { Name = "The Chain", TrackNumber = 4, Duration = TimeSpan.FromSeconds(270) }
                    }
                }
            }
        };

        // --- Guns N' Roses ---
        var gunsNRoses = new Artist
        {
            Name = "Guns N' Roses",
            Biography = "American hard rock band formed in 1985. Known for their rebellious attitude and explosive live performances led by Axl Rose and Slash.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = rock.Id }, new() { GenreId = hardRock.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "Appetite for Destruction", ReleaseDate = new DateOnly(1987, 7, 21),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Sweet Child o' Mine", TrackNumber = 1, Duration = TimeSpan.FromSeconds(356) },
                        new() { Name = "Welcome to the Jungle", TrackNumber = 2, Duration = TimeSpan.FromSeconds(276) },
                        new() { Name = "Paradise City", TrackNumber = 3, Duration = TimeSpan.FromSeconds(406) },
                        new() { Name = "Mr. Brownstone", TrackNumber = 4, Duration = TimeSpan.FromSeconds(222) }
                    }
                }
            }
        };

        // --- Eagles ---
        var eagles = new Artist
        {
            Name = "Eagles",
            Biography = "American rock band formed in Los Angeles in 1971. One of the most successful musical acts of the 1970s, known for their harmonies and country-rock sound.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = rock.Id }, new() { GenreId = classicRock.Id }, new() { GenreId = popRock.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "Hotel California", ReleaseDate = new DateOnly(1976, 12, 8),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Hotel California", TrackNumber = 1, Duration = TimeSpan.FromSeconds(391) },
                        new() { Name = "New Kid in Town", TrackNumber = 2, Duration = TimeSpan.FromSeconds(305) },
                        new() { Name = "Life in the Fast Lane", TrackNumber = 3, Duration = TimeSpan.FromSeconds(286) },
                        new() { Name = "Victim of Love", TrackNumber = 4, Duration = TimeSpan.FromSeconds(251) }
                    }
                }
            }
        };

        // --- Prince ---
        var prince = new Artist
        {
            Name = "Prince",
            Biography = "American singer, songwriter, and multi-instrumentalist. A musical genius who blended funk, rock, pop, and soul into an unmistakable sound.",
            ArtistGenres = new List<ArtistGenre>
            {
                new() { GenreId = pop.Id }, new() { GenreId = rock.Id }, new() { GenreId = soul.Id }
            },
            Albums = new List<Album>
            {
                new()
                {
                    Title = "Purple Rain", ReleaseDate = new DateOnly(1984, 6, 25),
                    Tracks = new List<Track>
                    {
                        new() { Name = "Purple Rain", TrackNumber = 1, Duration = TimeSpan.FromSeconds(521) },
                        new() { Name = "When Doves Cry", TrackNumber = 2, Duration = TimeSpan.FromSeconds(353) },
                        new() { Name = "Let's Go Crazy", TrackNumber = 3, Duration = TimeSpan.FromSeconds(280) },
                        new() { Name = "I Would Die 4 U", TrackNumber = 4, Duration = TimeSpan.FromSeconds(176) }
                    }
                }
            }
        };

        _context.Artists.AddRange(queen, michaelJackson, pinkFloyd, madonna, nirvana, u2, fleetwoodMac, gunsNRoses, eagles, prince);
        await _context.SaveChangesAsync();
    }
}
