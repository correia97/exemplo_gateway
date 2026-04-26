using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Entities;

namespace OpenCode.Domain.Data;

public class DragonBallContext : DbContext
{
    public DragonBallContext(DbContextOptions<DragonBallContext> options) : base(options) { }

    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Planet> Planets => Set<Planet>();
    public DbSet<Transformation> Transformations => Set<Transformation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dragonball");

        modelBuilder.Entity<Planet>(entity =>
        {
            entity.ToTable("Planets");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Character>(entity =>
        {
            entity.ToTable("Characters");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(c => c.Race)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(c => c.Ki)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(c => c.MaxKi)
                .HasMaxLength(50);
            entity.Property(c => c.IntroductionPhase)
                .HasMaxLength(100);
            entity.Property(c => c.Description)
                .HasMaxLength(2000);
            entity.Property(c => c.PictureUrl)
                .HasMaxLength(500);
            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");
            entity.Property(c => c.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasOne(c => c.Planet)
                .WithMany(p => p.Characters)
                .HasForeignKey(c => c.PlanetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Transformation>(entity =>
        {
            entity.ToTable("Transformations");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(t => t.Ki)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(t => t.Description)
                .HasMaxLength(1000);
            entity.Property(t => t.ImageUrl)
                .HasMaxLength(500);

            entity.HasOne(t => t.Character)
                .WithMany(c => c.Transformations)
                .HasForeignKey(t => t.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
