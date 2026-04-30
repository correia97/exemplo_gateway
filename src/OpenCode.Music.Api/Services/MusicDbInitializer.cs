using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;

namespace OpenCode.Music.Api.Services;

public class MusicDbInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public MusicDbInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken); // Wait for the database to be ready
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MusicContext>();
        if (!context.Database.CanConnect())
        {
            throw new InvalidOperationException($"Unable to connect to the database. Please check your connection settings.\r\n {context.Database.GetConnectionString()}");
        }
        await context.Database.MigrateAsync(cancellationToken);
        var seedService = scope.ServiceProvider.GetRequiredService<MusicSeedService>();
        await seedService.SeedAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
