using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Entities;

namespace OpenCode.Domain.Data;

public class MusicContext : DbContext
{
    public MusicContext(DbContextOptions<MusicContext> options) : base(options) { }

    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<ArtistGenre> ArtistGenres => Set<ArtistGenre>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("music");

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.ToTable("Genres");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(g => g.Description)
                .HasMaxLength(1000);
            entity.Property(g => g.CreatedAt)
                .HasDefaultValueSql("NOW()");
            entity.Property(g => g.UpdatedAt)
                .HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.ToTable("Artists");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(a => a.Biography)
                .HasMaxLength(4000);
            entity.Property(a => a.CreatedAt)
                .HasDefaultValueSql("NOW()");
            entity.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<ArtistGenre>(entity =>
        {
            entity.ToTable("ArtistGenres");
            entity.HasKey(ag => new { ag.ArtistId, ag.GenreId });

            entity.HasOne(ag => ag.Artist)
                .WithMany(a => a.ArtistGenres)
                .HasForeignKey(ag => ag.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ag => ag.Genre)
                .WithMany(g => g.ArtistGenres)
                .HasForeignKey(ag => ag.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.ToTable("Albums");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(a => a.CoverUrl)
                .HasMaxLength(500);
            entity.Property(a => a.ReleaseDate)
                .HasColumnType("date");
            entity.Property(a => a.CreatedAt)
                .HasDefaultValueSql("NOW()");
            entity.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasOne(a => a.Artist)
                .WithMany(ar => ar.Albums)
                .HasForeignKey(a => a.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.ToTable("Tracks");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(t => t.Duration)
                .HasColumnType("interval");
            entity.Property(t => t.Lyrics)
                .HasColumnType("text");
            entity.Property(t => t.CreatedAt)
                .HasDefaultValueSql("NOW()");
            entity.Property(t => t.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasOne(t => t.Album)
                .WithMany(al => al.Tracks)
                .HasForeignKey(t => t.AlbumId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
