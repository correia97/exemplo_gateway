using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using System.Diagnostics;

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
        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken); // Wait for the database to be ready
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DragonBallContext>();
        Debug.WriteLine($"\r\n\r\n\r\n\r\nConnection string : {context.Database.GetConnectionString()} \r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n");
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
