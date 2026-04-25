using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenCode.Domain.Data;

public class DragonBallContextFactory : IDesignTimeDbContextFactory<DragonBallContext>
{
    public DragonBallContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DragonBallContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=opencode;Username=postgres;Password=postgres");
        return new DragonBallContext(optionsBuilder.Options);
    }
}
