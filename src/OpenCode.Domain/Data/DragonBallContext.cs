using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Entities;

namespace OpenCode.Domain.Data;

public class DragonBallContext : DbContext
{
    public DragonBallContext(DbContextOptions<DragonBallContext> options) : base(options) { }

    public DbSet<Character> Characters => Set<Character>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dragonball");

        modelBuilder.Entity<Character>(entity =>
        {
            entity.ToTable("Characters");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(c => c.IntroductionPhase)
                .HasMaxLength(100);
            entity.Property(c => c.PictureUrl)
                .HasMaxLength(500);
            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");
            entity.Property(c => c.UpdatedAt)
                .HasDefaultValueSql("NOW()");
        });
    }
}
