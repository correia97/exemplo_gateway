using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;

namespace OpenCode.DragonBall.Api.Services;

public class DragonBallDbInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DragonBallDbInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Thread.Sleep(TimeSpan.FromSeconds(15)); // Wait for the database to be ready
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DragonBallContext>();
        if (!context.Database.CanConnect())
        {
            throw new InvalidOperationException($"Unable to connect to the database. Please check your connection settings.\r\n {context.Database.GetConnectionString()}");
        }
        await context.Database.MigrateAsync(cancellationToken);
        var seedService = scope.ServiceProvider.GetRequiredService<DragonBallSeedService>();
        await seedService.SeedAsync();

    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
