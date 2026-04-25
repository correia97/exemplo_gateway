using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenCode.Domain.Data;

public class MusicContextFactory : IDesignTimeDbContextFactory<MusicContext>
{
    public MusicContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MusicContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=opencode;Username=postgres;Password=postgres");
        return new MusicContext(optionsBuilder.Options);
    }
}
